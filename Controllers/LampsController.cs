using ActiveLights_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ActiveLights_MVC.Controllers
{
    public class LampsController : Controller
    {
        private List<LampViewModel> lampsViewModel = new();
        private List<Lamp> lamps = new();
        private List<LampStatus> filteredStatuses = new();
        private List<LampError> filteredErrorList = new();

        public async Task<IActionResult> AllLamps()
        {
            using (var httpClient = new HttpClient())
            {
                //Get all lamp information and store in list
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Items"))
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<Lamp> unsortedList = new();
                    unsortedList = JsonConvert.DeserializeObject<List<Lamp>>(responseContent);
                    if (unsortedList != null)
                    {
                        lamps = unsortedList.OrderBy(x => x.LampItemId).ToList();
                    }
                }
                //Get latests statuses for all lamps and store in list
                using (var statusResponse = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Status"))
                {
                    List<LampStatus> allLampStatuses = new();
                    List<LampStatus> unsortedStatusList = new();

                    string responseContent = await statusResponse.Content.ReadAsStringAsync();
                    unsortedStatusList = JsonConvert.DeserializeObject<List<LampStatus>>(responseContent);
                    
                    if (unsortedStatusList != null)
                    {
                        allLampStatuses = unsortedStatusList.OrderByDescending(x => x.StatusId).ToList();
                        List<string> uniqueLampIds = new List<string>();
                        foreach (var status in allLampStatuses)
                        {
                            var tempLampId = status.LampId;
                            if (!uniqueLampIds.Contains(tempLampId))
                            {
                                uniqueLampIds.Add(tempLampId);
                                filteredStatuses.Add(status);
                            }
                        }
                    }
                }
                //Get all active errors and store in list
                using (var errorResponse = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Errors"))
                {
                    List<LampError> allLampErrors = new();
                    List<LampError> unsortedErrorList = new();
                    
                    string responseContent = await errorResponse.Content.ReadAsStringAsync();
                    unsortedErrorList = JsonConvert.DeserializeObject<List<LampError>>(responseContent);
                    
                    if (unsortedErrorList != null)
                    {
                        allLampErrors = unsortedErrorList.OrderByDescending(x => x.ErrorID).ToList();
                        List<string> uniqueLampIdsErrors = new();
                        foreach (var error in allLampErrors)
                        {
                            var tempLampId = error.LampId;
                            if (!uniqueLampIdsErrors.Contains(tempLampId))
                            {
                                uniqueLampIdsErrors.Add(tempLampId);
                                if (error.ErrorPower || error.Errorlamp)
                                {
                                    filteredErrorList.Add(error);
                                }
                            }
                        }
                    }
                }
                //Build a list of 'LampViewModel' objects with values from each object in 'lamps' 
                if (lamps.Count > 0)
                {
                    foreach (var item in lamps)
                    {
                        LampViewModel newLamp = new();
                        newLamp.Name = item.Name;
                        newLamp.LampItemId = item.LampItemId;
                        newLamp.Location = item.Location;
                        newLamp.TimeLightOnEvening = (item.TimeLightOnEvening / 60).ToString() + ":" + (item.TimeLightOnEvening % 60).ToString("D2");
                        newLamp.TimeLightLowNight = (item.TimeLightLowNight / 60).ToString() + ":" + (item.TimeLightLowNight % 60).ToString("D2");
                        newLamp.TimeLightOnMorning = (item.TimeLightOnMorning / 60).ToString() + ":" + (item.TimeLightOnMorning % 60).ToString("D2");
                        newLamp.TimeLightOffMorning = (item.TimeLightOffMorning / 60).ToString() + ":" + (item.TimeLightOffMorning % 60).ToString("D2");
                        
                        //Get the latest status for this item
                        LampStatus latestStatus = filteredStatuses.Find(x => x.LampId == item.LampItemId);
                        bool statusError = false;
                        //If status exists, create a meaningful response string
                        if (latestStatus != null)
                        {
                            if (latestStatus.CreateTime.AddHours(2) < DateTime.Now)
                            {
                                statusError = true;
                                newLamp.CurrentLightLevel = latestStatus.LampLevel.ToString() + " (Expired)";
                            }
                            newLamp.CurrentLightLevel = latestStatus.LampLevel.ToString();
                        }
                        //If status does not exists, set an 'Unkown' value.
                        else
                        {
                            newLamp.CurrentLightLevel = "Unknown";
                            statusError = true;
                        }

                        //Set a default error string
                        string errorString = "Ok";
                        //Set new error string is there is an status error
                        if (statusError)
                        {
                            errorString = "LOST LAMP";
                        }
                        //Get the latest error report for this item, and modify error string if there is an active error
                        LampError thisCurrentError = filteredErrorList.Find(x => x.LampId == item.LampItemId);
                        if(thisCurrentError != null)
                        {
                            if (thisCurrentError.Errorlamp && statusError)
                            {
                                errorString += ", LAMP ERROR";
                            } else if (thisCurrentError.Errorlamp && !statusError)
                            {
                                errorString = "LAMP ERROR";
                            }

                            if (thisCurrentError.ErrorPower && (thisCurrentError.Errorlamp || statusError))
                            {
                                errorString += ", POWER ERROR";
                            } else if (thisCurrentError.ErrorPower && (!thisCurrentError.Errorlamp && !statusError))
                            {
                                errorString = "POWER ERROR";
                            }
                        }
                        newLamp.CurrentErrorStatus = errorString;
                        //Add the lamp object to the 'LampViewModel' list.
                        lampsViewModel.Add(newLamp);
                    }
                    return View(lampsViewModel);
                }
                return View();
            }

        }

        [HttpGet]
        public async Task<IActionResult> LampAdd()
        {
            LampEditViewModel newLamp = new();
            newLamp.InstallationDate = DateTime.Now;
            newLamp.LampGroup = "All";
            return View(newLamp);
        }

        [HttpPost]
        public async Task<IActionResult> LampAdd(LampEditViewModel lampModel)
        {
            Lamp tempDbLamp = new();
            tempDbLamp.LampItemId = lampModel.LampItemId;
            tempDbLamp.Name = lampModel.Name;
            tempDbLamp.Location = lampModel.Location;
            tempDbLamp.InstallationDate = lampModel.InstallationDate;
            tempDbLamp.IpAddress = lampModel.IpAddress;
            tempDbLamp.LampGroup = lampModel.LampGroup;
            tempDbLamp.NeighborsBack = lampModel.NeighborsBack;
            tempDbLamp.NeighborsForward = lampModel.NeighborsForward;
            tempDbLamp.LevelHigh = lampModel.LevelHigh;
            tempDbLamp.LevelLow = lampModel.LevelLow;
            tempDbLamp.LevelOff = lampModel.LevelOff;
            tempDbLamp.TimeLightOnEvening = (lampModel.TimeLightOnEvening.Hour * 60) + lampModel.TimeLightOnEvening.Minute;
            tempDbLamp.TimeLightLowNight = (lampModel.TimeLightLowNight.Hour * 60) + lampModel.TimeLightLowNight.Minute;
            tempDbLamp.TimeLightOnMorning = (lampModel.TimeLightOnMorning.Hour * 60) + lampModel.TimeLightOnMorning.Minute;
            tempDbLamp.TimeLightOffMorning = (lampModel.TimeLightOffMorning.Hour * 60) + lampModel.TimeLightOffMorning.Minute; 

            using (var httpClient = new HttpClient())
            {
                await httpClient.PostAsJsonAsync<Lamp>("https://activelights-webapp.azurewebsites.net/api/Items/", tempDbLamp);
                await httpClient.PutAsJsonAsync<Lamp>(lampModel.IpAddress + "/config", tempDbLamp);
            }

            return RedirectToAction("AllLamps");
        }

        public async Task<IActionResult> LampDetails(string id)
        {
            LampEditViewModel tempLampViewModel = new();
            using (var httpClient = new HttpClient())
            {
                //Get the specific lamp information and return it to view
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Items/" + id))
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Lamp tempLamp = new();
                    tempLamp = JsonConvert.DeserializeObject<Lamp>(responseContent);
                    if (tempLamp != null)
                    {
                        tempLampViewModel.LampItemId = tempLamp.LampItemId;
                        tempLampViewModel.Name = tempLamp.Name;
                        tempLampViewModel.Location = tempLamp.Location;
                        tempLampViewModel.InstallationDate = tempLamp.InstallationDate;
                        tempLampViewModel.IpAddress = tempLamp.IpAddress;
                        tempLampViewModel.LampGroup = tempLamp.LampGroup;
                        tempLampViewModel.NeighborsBack = tempLamp.NeighborsBack;
                        tempLampViewModel.NeighborsForward = tempLamp.NeighborsForward;
                        tempLampViewModel.LevelHigh = tempLamp.LevelHigh;
                        tempLampViewModel.LevelLow = tempLamp.LevelLow;
                        tempLampViewModel.LevelOff = tempLamp.LevelOff;
                        tempLampViewModel.TimeLightOnEvening = new TimeOnly(tempLamp.TimeLightOnEvening / 60, tempLamp.TimeLightOnEvening % 60, 00);
                        tempLampViewModel.TimeLightLowNight = new TimeOnly(tempLamp.TimeLightLowNight / 60, tempLamp.TimeLightLowNight % 60, 00);
                        tempLampViewModel.TimeLightOnMorning = new TimeOnly(tempLamp.TimeLightOnMorning / 60, tempLamp.TimeLightOnMorning % 60, 00);
                        tempLampViewModel.TimeLightOffMorning = new TimeOnly(tempLamp.TimeLightOffMorning / 60, tempLamp.TimeLightOffMorning % 60, 00);
                        return View(tempLampViewModel);
                    }
                }
            }
            return View(tempLampViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditLamp(string id)
        {
            LampEditViewModel tempLampViewModel = new();
            using (var httpClient = new HttpClient())
            {
                Lamp tempLamp = new();
                //Get the specific lamp information and return it to view
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Items/" + id))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        tempLamp = JsonConvert.DeserializeObject<Lamp>(responseContent);
                    }
                }
                tempLampViewModel.LampItemId = tempLamp.LampItemId;
                tempLampViewModel.Name = tempLamp.Name;
                tempLampViewModel.Location = tempLamp.Location;
                tempLampViewModel.InstallationDate = tempLamp.InstallationDate;
                tempLampViewModel.IpAddress = tempLamp.IpAddress;
                tempLampViewModel.LampGroup = tempLamp.LampGroup;
                tempLampViewModel.NeighborsBack = tempLamp.NeighborsBack;
                tempLampViewModel.NeighborsForward = tempLamp.NeighborsForward;
                tempLampViewModel.LevelHigh = tempLamp.LevelHigh;
                tempLampViewModel.LevelLow = tempLamp.LevelLow;
                tempLampViewModel.LevelOff = tempLamp.LevelOff;
                tempLampViewModel.TimeLightOnEvening = new TimeOnly(tempLamp.TimeLightOnEvening / 60, tempLamp.TimeLightOnEvening % 60, 00);
                tempLampViewModel.TimeLightLowNight = new TimeOnly(tempLamp.TimeLightLowNight / 60, tempLamp.TimeLightLowNight % 60, 00);
                tempLampViewModel.TimeLightOnMorning = new TimeOnly(tempLamp.TimeLightOnMorning / 60, tempLamp.TimeLightOnMorning % 60, 00);
                tempLampViewModel.TimeLightOffMorning = new TimeOnly(tempLamp.TimeLightOffMorning / 60, tempLamp.TimeLightOffMorning % 60, 00);
            }
            return View(tempLampViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditLamp(LampEditViewModel lampModel)
        {
            Lamp tempDbLamp = new();
            tempDbLamp.LampItemId = lampModel.LampItemId;
            tempDbLamp.Name = lampModel.Name;
            tempDbLamp.Location = lampModel.Location;
            tempDbLamp.InstallationDate = lampModel.InstallationDate;
            tempDbLamp.IpAddress = lampModel.IpAddress;
            tempDbLamp.LampGroup = lampModel.LampGroup;
            tempDbLamp.NeighborsBack = lampModel.NeighborsBack;
            tempDbLamp.NeighborsForward = lampModel.NeighborsForward;
            tempDbLamp.LevelHigh = lampModel.LevelHigh;
            tempDbLamp.LevelLow = lampModel.LevelLow;
            tempDbLamp.LevelOff = lampModel.LevelOff;
            tempDbLamp.TimeLightOnEvening = (lampModel.TimeLightOnEvening.Hour * 60) + lampModel.TimeLightOnEvening.Minute;
            tempDbLamp.TimeLightLowNight = (lampModel.TimeLightLowNight.Hour * 60) + lampModel.TimeLightLowNight.Minute;
            tempDbLamp.TimeLightOnMorning = (lampModel.TimeLightOnMorning.Hour * 60) + lampModel.TimeLightOnMorning.Minute;
            tempDbLamp.TimeLightOffMorning = (lampModel.TimeLightOffMorning.Hour * 60) + lampModel.TimeLightOffMorning.Minute;
            tempDbLamp.LevelOverride = -1;

            using (var httpClient = new HttpClient())
            {
                await httpClient.PutAsJsonAsync<Lamp>("https://activelights-webapp.azurewebsites.net/api/Items/"+lampModel.LampItemId, tempDbLamp);
                await httpClient.PutAsJsonAsync<Lamp>(tempDbLamp.IpAddress + "/config", tempDbLamp);
            }

            return RedirectToAction("LampDetails", new { id = lampModel.LampItemId });
        }

        public async Task<IActionResult> LampDelete(string id)
        {
            LampEditViewModel tempLampViewModel = new();
            using (var httpClient = new HttpClient())
            {
                Lamp tempLamp = new();
                //Get the specific lamp information and return it to view
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Items/" + id))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        tempLamp = JsonConvert.DeserializeObject<Lamp>(responseContent);
                    }
                }
                tempLampViewModel.LampItemId = tempLamp.LampItemId;
                tempLampViewModel.Name = tempLamp.Name;
                tempLampViewModel.Location = tempLamp.Location;
                tempLampViewModel.InstallationDate = tempLamp.InstallationDate;
                tempLampViewModel.IpAddress = tempLamp.IpAddress;
                tempLampViewModel.LampGroup = tempLamp.LampGroup;
                tempLampViewModel.NeighborsBack = tempLamp.NeighborsBack;
                tempLampViewModel.NeighborsForward = tempLamp.NeighborsForward;
                tempLampViewModel.LevelHigh = tempLamp.LevelHigh;
                tempLampViewModel.LevelLow = tempLamp.LevelLow;
                tempLampViewModel.LevelOff = tempLamp.LevelOff;
                tempLampViewModel.TimeLightOnEvening = new TimeOnly(tempLamp.TimeLightOnEvening / 60, tempLamp.TimeLightOnEvening % 60, 00);
                tempLampViewModel.TimeLightLowNight = new TimeOnly(tempLamp.TimeLightLowNight / 60, tempLamp.TimeLightLowNight % 60, 00);
                tempLampViewModel.TimeLightOnMorning = new TimeOnly(tempLamp.TimeLightOnMorning / 60, tempLamp.TimeLightOnMorning % 60, 00);
                tempLampViewModel.TimeLightOffMorning = new TimeOnly(tempLamp.TimeLightOffMorning / 60, tempLamp.TimeLightOffMorning % 60, 00);
            }

            return View(tempLampViewModel);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(LampEditViewModel lamp)
        {
            using (var httpClient = new HttpClient())
            {
                await httpClient.DeleteAsync("https://activelights-webapp.azurewebsites.net/api/Items/" + lamp.LampItemId);
            }

            return RedirectToAction("AllLamps");
        }


        public IActionResult LampOverride(string id)
        {
            LampOverrideViewModel lamp = new();
            lamp.LampItemId = id;
            return View(lamp);
        }

        [HttpPost]
        public async Task<IActionResult> LampOverride(LampOverrideViewModel lamp)
        {
            if (lamp != null)
            {
                using (var httpClient = new HttpClient())
                {
                    await httpClient.PutAsJsonAsync<LampOverrideViewModel>("https://gubirasp1.serveo.net/override", lamp);
                }
                return RedirectToAction("AllLamps");
            }
            return NotFound();
        }
    }

}
