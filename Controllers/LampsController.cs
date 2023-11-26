using ActiveLights_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Runtime.Intrinsics.X86;

namespace ActiveLights_MVC.Controllers
{
    public class LampsController : Controller
    {
        // Liste til lamper som skal returneres til View
        private List<LampViewModel> lampsViewModel = new();
        // Liste til at opbevare alle lamper fra databasen i en sorteret liste (lamp ID)
        private List<Lamp> lamps = new();
        // Liste med filtrerede status beskeder som bruges til at bygge "LampViewModel" 
        private List<LampStatus> filteredStatuses = new();
        // Liste med filtrerede fejl beskeder som bruges til at bygge "LampViewModel"
        private List<LampError> filteredErrorList = new();

        // "Index" side som viser listen med alle lamper 
        public async Task<IActionResult> AllLamps()
        {
            using (var httpClient = new HttpClient())
            {
                // Hent all lampe informationer fra databasen og gem i en liste
                // Brug http clienten til at lave en "Get" request til API'ens lampe tabel
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Items"))
                {
                    // Læs svaret og gem i en tekstsstreng
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Midlertidig liste til at holde lamperne som de kommer fra API'en
                    List<Lamp> unsortedList = new();
                    // Deserialize tekststrengen fra JSON til en liste af lamper
                    unsortedList = JsonConvert.DeserializeObject<List<Lamp>>(responseContent);
                    // Hvis listen ikke er tom
                    if (unsortedList != null)
                    {
                        // Sorter den midlertidige liste til en ny liste
                        lamps = unsortedList.OrderBy(x => x.LampItemId).ToList();
                    }
                }

                // Hent den seneste status for alle lamper og gem det i en liste
                // Brug http clienten til at lave en "Get" request til API'ens status tabel
                using (var statusResponse = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Status"))
                {
                    // Midlertidige lister som benyttes til at hente oplysninger om alle statusser og til at sortere statusserne
                    List<LampStatus> allLampStatuses = new();
                    List<LampStatus> unsortedStatusList = new();
                    // Læs svaret og gem i en tekstsstreng
                    string responseContent = await statusResponse.Content.ReadAsStringAsync();
                    // Deserialize tekststrengen fra JSON til en liste af statusser
                    unsortedStatusList = JsonConvert.DeserializeObject<List<LampStatus>>(responseContent);
                    // Hvis listen ikke er tom
                    if (unsortedStatusList != null)
                    {
                        // Sorter den midlertidige liste til en ny liste
                        allLampStatuses = unsortedStatusList.OrderByDescending(x => x.StatusId).ToList();
                        // En midlertidig liste med lampe ID'er
                        List<string> uniqueLampIds = new List<string>();
                        // Løb gennem den midlertige liste med sorterede statusser
                        foreach (var status in allLampStatuses)
                        {
                            // Gem denne statusses lampe ID i en variabel 
                            var tempLampId = status.LampId;
                            // Hvis denne lampe ID IKKE findes i listen med ID'er
                            if (!uniqueLampIds.Contains(tempLampId))
                            {
                                // Tilføj lampe ID til listen
                                uniqueLampIds.Add(tempLampId);
                                // Tilføj statussen til listen med nyeste statusser
                                filteredStatuses.Add(status);
                            }
                        }
                    }
                }

                // Hent alle aktive fejl og gem dem i en liste
                // Brug http clienten til at lave en "Get" request til API'ens fejl tabel
                using (var errorResponse = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Errors"))
                {
                    // Midlertidige lister som benyttes til at hente oplysninger om alle fejl og til at sortere fejlene
                    List<LampError> allLampErrors = new();
                    List<LampError> unsortedErrorList = new();
                    // Læs svaret og gem i en tekstsstreng
                    string responseContent = await errorResponse.Content.ReadAsStringAsync();
                    // Deserialize tekststrengen fra JSON til en liste af fejl
                    unsortedErrorList = JsonConvert.DeserializeObject<List<LampError>>(responseContent);
                    // Hvis listen ikke er tom
                    if (unsortedErrorList != null)
                    {
                        // Sorter den midlertidige liste til en ny liste
                        allLampErrors = unsortedErrorList.OrderByDescending(x => x.ErrorID).ToList();
                        // En midlertidig liste med lampe ID'er
                        List<string> uniqueLampIdsErrors = new();
                        // Løb gennem den midlertige liste med sorterede fejl
                        foreach (var error in allLampErrors)
                        {
                            // Gem denne fejl lampeID i en variabel 
                            var tempLampId = error.LampId;
                            // Hvis denne lampe ID IKKE findes i listen med ID'er
                            if (!uniqueLampIdsErrors.Contains(tempLampId))
                            {
                                // Tilføj lampe ID til listen
                                uniqueLampIdsErrors.Add(tempLampId);
                                // Tilføj fejlen til listen med aktive fejl HVIS der er en aktiv fejl
                                if (error.ErrorPower || error.Errorlamp)
                                {
                                    filteredErrorList.Add(error);
                                }
                            }
                        }
                    }
                }

                // Byg en liste af 'LampViewModel' objekter med værdier fra hvert objekt i 'lamps' 
                // Hvis listen med lamper ikke er tom
                if (lamps.Count > 0)
                {
                    // Løb igennem hvert objekt i 'lamps'
                    foreach (var item in lamps)
                    {
                        // Lav et midlertidig 'LampViewModel' objekt
                        LampViewModel newLamp = new();
                        // Gem de relevante værdier i det midlertidige objekt
                        newLamp.Name = item.Name;
                        newLamp.LampItemId = item.LampItemId;
                        newLamp.Location = item.Location;
                        newLamp.TimeLightOnEvening = (item.TimeLightOnEvening / 60).ToString() + ":" + (item.TimeLightOnEvening % 60).ToString("D2");
                        newLamp.TimeLightLowNight = (item.TimeLightLowNight / 60).ToString() + ":" + (item.TimeLightLowNight % 60).ToString("D2");
                        newLamp.TimeLightOnMorning = (item.TimeLightOnMorning / 60).ToString() + ":" + (item.TimeLightOnMorning % 60).ToString("D2");
                        newLamp.TimeLightOffMorning = (item.TimeLightOffMorning / 60).ToString() + ":" + (item.TimeLightOffMorning % 60).ToString("D2");
                        
                        // Hent statussen for denne lampe ind i et midlertidigt 'LampStatus' objekt
                        LampStatus latestStatus = filteredStatuses.Find(x => x.LampId == item.LampItemId);
                        bool statusError = false;
                        // Hvis der er en status så lav en meningsfyldt tekstreng
                        if (latestStatus != null)
                        {
                            // Hvis statussen er for gammel, så tilføj '(Expired)'
                            if (latestStatus.CreateTime.AddHours(2) < DateTime.Now)
                            {
                                statusError = true;
                                newLamp.CurrentLightLevel = latestStatus.LampLevel.ToString() + " (Expired)";
                            }
                            newLamp.CurrentLightLevel = latestStatus.LampLevel.ToString();
                        }
                        // Hvis der ikke er en status så sæt tekststrengen til 'Unknown'
                        else
                        {
                            newLamp.CurrentLightLevel = "Unknown";
                            statusError = true;
                        }

                        // Lav en defult fejl streng
                        string errorString = "Ok";
                        // Lav en ny fejl streng hvis der ikke er en fejl besked
                        if (statusError)
                        {
                            errorString = "LOST LAMP";
                        }
                        // Hent den aktuelle fejl besked for denne lampe og modificer fejl tekststrengen hvis der er en fejl.
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
                        // Tilføj den samlede fejl tekststreng til det midlertidige lampe objekt
                        newLamp.CurrentErrorStatus = errorString;
                        // Tilføj lampe objektet til listen 'lampsViewModel'
                        lampsViewModel.Add(newLamp);
                    }
                    return View(lampsViewModel.ToList());
                }
                return View(lampsViewModel.ToList());
            }

        }
        // Denne action bruges til at kalde ett view som bruges til at oprette en ny lampe
        [HttpGet]
        public async Task<IActionResult> LampAdd()
        {
            // Opret et midlertige 'LampEditViewModel' objekt
            LampEditViewModel newLamp = new();
            // Tilføj default værdier
            newLamp.InstallationDate = DateTime.Now;
            newLamp.LampGroup = "All";
            // Send objektet til view
            return View(newLamp);
        }

        // Denne action bruges når viewet sender objektet tilbage til controller med en Post fra formen
        [HttpPost]
        public async Task<IActionResult> LampAdd(LampEditViewModel lampModel)
        {
            // Lav et midlertidigt 'Lamp' objekt som ender emd at skulle sendes til API'en og lampen
            Lamp tempDbLamp = new();
            // Skriv værdier fra view model til 'lamp' model
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
            // Send 'lamp' objektet til API'en som en 'Post' og til lampen som en 'Put'
            using (var httpClient = new HttpClient())
            {
                await httpClient.PostAsJsonAsync<Lamp>("https://activelights-webapp.azurewebsites.net/api/Items/", tempDbLamp);
                // Skrivningen til lampen sættes i en try/catch for at undgå en exception fejl hvis der er problemer med IPadressen eller forbindelsen
                try
                {
                    if (lampModel.IpAddress != null)
                    {
                        await httpClient.PutAsJsonAsync<Lamp>(lampModel.IpAddress + "/config", tempDbLamp);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to send to lamp");
                }
            }
            // Returner til AllLamps action som laver viewet med alle lamper
            return RedirectToAction("AllLamps");
        }

        // Denne action kaldes når der skal vises detaljer om en lampe
        public async Task<IActionResult> LampDetails(string id)
        {
            // Lav et midlertidig 'LampEditViewModel' objekt
            LampEditViewModel tempLampViewModel = new();
            // Opret en ny HttpClient
            using (var httpClient = new HttpClient())
            {
                // Brug clienten til at hente information om en specifik lampe og returnere den til view
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Items/" + id))
                {
                    // Gem svaret fra API'en i en tekststreng
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Lav et nyt midlertidige 'Lamp' objekt
                    Lamp tempLamp = new();
                    // Deserialize tekststrengen fra JSON til det midlertige objekt
                    tempLamp = JsonConvert.DeserializeObject<Lamp>(responseContent);
                    // Hvis objektet ikke er tomt
                    if (tempLamp != null)
                    {
                        // Skriv værdierne fra det midlertidige objekt til det midlertidige 'LampEditViewModel'
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

        // Denne action kaldes når der skal rettes i en lampe
        [HttpGet]
        public async Task<IActionResult> EditLamp(string id)
        {
            // Lav et midlertig 'LampEditViewModel' objekt som skal sendes til view
            LampEditViewModel tempLampViewModel = new();
            // Opret en ny HttpClient
            using (var httpClient = new HttpClient())
            {
                // Lav et midlertidig 'Lamp' objekt
                Lamp tempLamp = new();
                // Brug klienten til at hente information om den specifikke lampe fra API'en
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Items/" + id))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // Læs svaret og gem i en tekstsstreng
                        string responseContent = await response.Content.ReadAsStringAsync();
                        // Deserialize tekststrengen fra JSON til det midlertige 'Lamp' objekt
                        tempLamp = JsonConvert.DeserializeObject<Lamp>(responseContent);
                    }
                }
                // Skriv værdier fra 'Lamp' objekt til 'LampEditViewModel' objekt
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

        // Denne action bruges når viewet sender objektet tilbage til controller med en Post fra formen
        [HttpPost]
        public async Task<IActionResult> EditLamp(LampEditViewModel lampModel)
        {
            // Lav et midlertidigt 'Lamp' objekt som skal sendes til API og lampe 
            Lamp tempDbLamp = new();
            // Skriv værdier fra det returnerede 'LampEditViewModel' objekt til det midlertidige 'Lamp' objekt
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
            // Opret en HttpClient og brug det til at sende lampe objektet til databasen og lampen 
            using (var httpClient = new HttpClient())
            {
                await httpClient.PutAsJsonAsync<Lamp>("https://activelights-webapp.azurewebsites.net/api/Items/"+lampModel.LampItemId, tempDbLamp);
                // Skrivningen til lampen sættes i en try/catch for at undgå en exception fejl hvis der er problemer med IPadressen eller forbindelsen
                try
                {
                    if (lampModel.IpAddress != null)
                    {
                        await httpClient.PutAsJsonAsync<Lamp>(tempDbLamp.IpAddress + "/config", tempDbLamp);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to send to lamp");
                }
            }
            // Returner til 'Details' viewet for denne lampe
            return RedirectToAction("LampDetails", new { id = lampModel.LampItemId });
        }

        // Denne action kaldes når man vil slette en lampe. Den kalder et view hvor lampens detaljer vises og man bliver spurgt om man vil virkelig vil slette
        public async Task<IActionResult> LampDelete(string id)
        {
            // Lan et midlertidige 'LampEditViewModel' objekt som skal sendes til view
            LampEditViewModel tempLampViewModel = new();
            // Opret en HttpClient
            using (var httpClient = new HttpClient())
            {
                // Opret et midlertidigt 'Lamp' objekt
                Lamp tempLamp = new();
                // Hent imformation om den specifikke lampe med API'en
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Items/" + id))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // Læs svaret og gem i en tekstsstreng
                        string responseContent = await response.Content.ReadAsStringAsync();
                        // Deserialize tekststrengen fra JSON til det midlertige 'Lamp' objekt
                        tempLamp = JsonConvert.DeserializeObject<Lamp>(responseContent);
                    }
                }
                // Skriv værdier fra 'Lamp' objekt til 'LampEditViewModel' objekt
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

        // Denne action bruges når delete viewet sender objektet tilbage til controller med en Post fra formen (med navnet Delete)
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(LampEditViewModel lamp)
        {
            // Opret en HttpClient og brug den til at sende en 'Delete' request til API'en
            using (var httpClient = new HttpClient())
            {
                await httpClient.DeleteAsync("https://activelights-webapp.azurewebsites.net/api/Items/" + lamp.LampItemId);
            }
            // Return til 'AllLamps' action
            return RedirectToAction("AllLamps");
        }

        // Denne action bruges til at kalde 'Override' view
        public IActionResult LampOverride(string id)
        {
            // Opret et 'LampOverrideViewModel' objekt
            LampOverrideViewModel lamp = new();
            // Overfør ID til objektet og send til til viewet 
            lamp.LampItemId = id;
            return View(lamp);
        }

        // Denne action bruges når override viewet sender objektet tilbage til controlleren med en Post fra formen 
        [HttpPost]
        public async Task<IActionResult> LampOverride(LampOverrideViewModel lamp)
        {
            // Hvis det returnerede lampe objekt ikke er tomt
            if (lamp != null)
            {
                // Opret en HttpClient
                using (var httpClient = new HttpClient())
                {
                    // Lav et midlertidig 'Lamp' objekt
                    Lamp tempLamp = new();
                    // Brug klienten til at hente information om den specifikke lampe fra API'en
                    using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Items/" + lamp.LampItemId))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            // Læs svaret og gem i en tekstsstreng
                            string responseContent = await response.Content.ReadAsStringAsync();
                            // Deserialize tekststrengen fra JSON til det midlertige 'Lamp' objekt
                            tempLamp = JsonConvert.DeserializeObject<Lamp>(responseContent);
                        }
                    }
                    // Brug IP adresen fra den hentede lampe til at sende det returnede 'LampOverrideViewModel' objekt til lampen
                    await httpClient.PutAsJsonAsync<LampOverrideViewModel>(tempLamp.IpAddress + "/override", lamp);
                }
                // Returner til 'AllLamps' action som viser viewt emd alle lamper
                return RedirectToAction("AllLamps");
            }
            return NotFound();
        }
    }

}