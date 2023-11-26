using ActiveLights_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel;

namespace ActiveLights_MVC.Controllers
{
    public class ErrorsController : Controller
    {
        private List<LampError> lampErrors = new List<LampError>();

        public async Task<IActionResult> AllErrors()
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Errors"))
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<LampError> unsortedList = new();
                    unsortedList = JsonConvert.DeserializeObject<List<LampError>>(responseContent);
                    if (unsortedList != null)
                    {
                        lampErrors = unsortedList.OrderByDescending(x => x.ErrorID).ToList();
                    }
                }
            }

            if (lampErrors.Count > 0)
            {
                return View(lampErrors);
            }
            return View();
        }

        public async Task<IActionResult> ActiveErrors()
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Errors"))
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<LampError> unsortedList = new();
                    unsortedList = JsonConvert.DeserializeObject<List<LampError>>(responseContent);
                    if (unsortedList != null)
                    {
                        lampErrors = unsortedList.OrderByDescending(x => x.ErrorID).ToList();
                    }
                }
            }

            if (lampErrors.Count > 0)
            {
                List<LampError> filteredErrors = new();
                List<string> uniqueLampIds = new();
                foreach (var error in lampErrors)
                {
                    var tempLampId = error.LampId;
                    if (!uniqueLampIds.Contains(tempLampId))
                    {
                        uniqueLampIds.Add(tempLampId);
                        if (error.ErrorPower || error.Errorlamp)
                        {
                            filteredErrors.Add(error);
                        }
                    }
                }
                return View(filteredErrors.ToList());
            }

            return View(lampErrors);
        }
    }
}
