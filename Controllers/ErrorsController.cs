using ActiveLights_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ActiveLights_MVC.Controllers
{
    public class ErrorsController : Controller
    {
        // Liste som bruges til at holde fejl
        private List<LampError> lampErrors = new List<LampError>();

        // Denne action kaldes når viewet med alle fejl kaldes
        public async Task<IActionResult> AllErrors()
        {
            // Opret en HttpClient
            using (var httpClient = new HttpClient())
            {
                // Brug klienten til at hente alle fejl
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Errors"))
                {
                    // Læs svaret og gem i en tekstsstreng
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Midlertidig liste til at holde alle fejl som de kommer fra API'en
                    List<LampError> unsortedList = new();
                    // Deserialize tekststrengen fra JSON til en liste af fejl
                    unsortedList = JsonConvert.DeserializeObject<List<LampError>>(responseContent);
                    // Hvis listen ikke er tom
                    if (unsortedList != null)
                    {
                        // Lav en sorteret liste baseret på ErrorID fra højeste til laveste
                        lampErrors = unsortedList.OrderByDescending(x => x.ErrorID).ToList();
                    }
                }
            }
            return View(lampErrors);
        }

        // Denne action kaldes når viewet med aktive fejl kaldes
        public async Task<IActionResult> ActiveErrors()
        {
            // Opret en HttpClient
            using (var httpClient = new HttpClient())
            {
                // Brug klienten til at hente alle fejl
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Errors"))
                {
                    // Læs svaret og gem i en tekstsstreng
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Midlertidig liste til at holde alle fejl som de kommer fra API'en
                    List<LampError> unsortedList = new();
                    // Deserialize tekststrengen fra JSON til en liste af fejl
                    unsortedList = JsonConvert.DeserializeObject<List<LampError>>(responseContent);
                    // Hvis listen ikke er tom
                    if (unsortedList != null)
                    {
                        // Lav en sorteret liste baseret på ErrorID fra højeste til laveste
                        lampErrors = unsortedList.OrderByDescending(x => x.ErrorID).ToList();
                    }
                }
            }
            // Hvis listen ikke er tom
            if (lampErrors.Count > 0)
            {
                // Opret midlertidige lister
                List<LampError> filteredErrors = new();
                List<string> uniqueLampIds = new();
                // Løb igennem listen med alle fejl
                foreach (var error in lampErrors)
                {
                    // Gem denne fejl lampeID i en variabel
                    var tempLampId = error.LampId;
                    // Hvis denne lampe ID IKKE findes i listen med ID'er
                    if (!uniqueLampIds.Contains(tempLampId))
                    {
                        // Tilføj lampe ID til listen
                        uniqueLampIds.Add(tempLampId);
                        // Tilføj fejlen til listen med aktive fejl HVIS der er en aktiv fejl
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
