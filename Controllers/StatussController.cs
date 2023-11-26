using ActiveLights_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ActiveLights_MVC.Controllers
{
    public class StatussController : Controller
    {
        // Liste som bruges til at holde statusser
        public List<LampStatus> lampStatuses = new List<LampStatus>();

        // Denne action kaldes når viewet med alle statusser kaldes
        public async Task<IActionResult> AllStatuss()
        {
            // Opret en HttpClient
            using (var httpClient = new HttpClient())
            {
                // Brug klienten til at hente alle fejl
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Status"))
                {
                    // Læs svaret og gem i en tekstsstreng
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Midlertidig liste til at holde alle statusser som de kommer fra API'en
                    List<LampStatus> unsortedList = new List<LampStatus>();
                    // Deserialize tekststrengen fra JSON til en liste af statusser
                    unsortedList = JsonConvert.DeserializeObject<List<LampStatus>>(responseContent);
                    // Hvis listen ikke er tom
                    if (unsortedList != null)
                    {
                        // Lav en sorteret liste baseret på StatusID fra højeste til laveste
                        lampStatuses = unsortedList.OrderByDescending(x => x.StatusId).ToList();
                    }
                }
            }
            return View(lampStatuses);
        }

        // Denne action kaldes når viewet med nyeste statusser kaldes
        public async Task<IActionResult> LatestStatuss()
        {
            // Opret en HttpClient
            using (var httpClient = new HttpClient())
            {
                // Brug klienten til at hente alle fejl
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Status"))
                {
                    // Læs svaret og gem i en tekstsstreng
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Midlertidig liste til at holde alle statusser som de kommer fra API'en
                    List<LampStatus> unsortedList = new();
                    // Deserialize tekststrengen fra JSON til en liste af statusser
                    unsortedList = JsonConvert.DeserializeObject<List<LampStatus>>(responseContent);
                    // Hvis listen ikke er tom
                    if (unsortedList != null)
                    {
                        // Lav en sorteret liste baseret på StatusID fra højeste til laveste
                        lampStatuses = unsortedList.OrderByDescending(x => x.StatusId).ToList();
                    }
                }
            }
            // Hvis listen ikke er tom
            if (lampStatuses.Count > 0)
            {
                // Opret midlertidige lister
                List<LampStatus> filteredStatuss = new();
                List<LampStatus> filteredSortedStatuses = new();
                List<string> uniqueLampIds = new List<string>();
                // Løb igennem listen med alle statusser
                foreach (var status in lampStatuses)
                {
                    // Gem denne status lampeID i en variabel
                    var tempLampId = status.LampId;
                    // Hvis denne lampe ID IKKE findes i listen med ID'er
                    if (!uniqueLampIds.Contains(tempLampId))
                    {
                        // Tilføj lampe ID til listen
                        uniqueLampIds.Add(tempLampId);
                        // Tilføj statussen til listen med de nyeste statusser
                        filteredStatuss.Add(status);
                    }
                }
                // Sorter listen til en ny liste baseret på lampe ID
                filteredSortedStatuses = filteredStatuss.OrderBy(x => x.LampId).ToList();

                return View(filteredSortedStatuses);
            }
            return View(lampStatuses);
        }

    }
}
