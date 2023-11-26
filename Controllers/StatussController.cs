using ActiveLights_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ActiveLights_MVC.Controllers
{
    public class StatussController : Controller
    {
        public List<LampStatus> lampStatuses = new List<LampStatus>();

        public async Task<IActionResult> AllStatuss()
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Status"))
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<LampStatus> unsortedList = new List<LampStatus>();
                    unsortedList = JsonConvert.DeserializeObject<List<LampStatus>>(responseContent);
                    if (unsortedList != null)
                    {
                        lampStatuses = unsortedList.OrderByDescending(x => x.StatusId).ToList();
                    }
                }
            }

            if (lampStatuses.Count > 0)
            {
                return View(lampStatuses);
            }
            return View();
        }

        public async Task<IActionResult> LatestStatuss()
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://activelights-webapp.azurewebsites.net/api/Status"))
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<LampStatus> unsortedList = new();
                    unsortedList = JsonConvert.DeserializeObject<List<LampStatus>>(responseContent);
                    if (unsortedList != null)
                    {
                        lampStatuses = unsortedList.OrderByDescending(x => x.StatusId).ToList();
                    }
                }
            }

            if (lampStatuses.Count > 0)
            {
                List<LampStatus> filteredStatuss = new();
                List<LampStatus> filteredSortedStatuses = new();
                List<string> uniqueLampIds = new List<string>();
                foreach (var status in lampStatuses)
                {
                    var tempLampId = status.LampId;
                    if (!uniqueLampIds.Contains(tempLampId))
                    {
                        uniqueLampIds.Add(tempLampId);
                        filteredStatuss.Add(status);
                    }
                }
                
                filteredSortedStatuses = filteredStatuss.OrderBy(x => x.LampId).ToList();

                return View(filteredSortedStatuses);
            }
            return View(lampStatuses);
        }

    }
}
