using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YouTrackSharp;
using YouTrackSharp.Issues;
using Newtonsoft.Json;

namespace ytpmd.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("[action]")]
        //public IEnumerable<WeatherForecast> WeatherForecasts()
        public IEnumerable<ResultData> WeatherForecasts()
        {
            var connection = new UsernamePasswordConnection("", "", "");
            var issues_service = connection.CreateIssuesService();
            var issuse_project = issues_service.GetIssuesInProject("ba", "#{Sprint 4} #me", 0, 100);
            var issues = issuse_project.GetAwaiter().GetResult();
            List<ResultData> res = new List<ResultData>();
            foreach (var i in issues) {
                //Console.WriteLine("Issue id: " + i.Id);// + ", desc : " + i.Description);
                var issues_changes = issues_service.GetChangeHistoryForIssue(i.Id);
                foreach (var c in issues_changes.GetAwaiter().GetResult()) {
                    bool has = false;
                    String state_name = "";
                    foreach (var f in c.Fields) {
                        if (f.Name.Equals("State") || f.Name.Equals("Статус")) {
                            state_name = f.Name;
                            break;
                        }
                    }
                    if (state_name.Length == 0)
                        continue;
                    res.Add(new ResultData {
                        Id = i.Id,
                        Status = c.ForField(state_name).To.AsString(),
                        Start = c.ForField("updated").To.AsDateTime().ToString("dd.MM.yy hh:mm"),
                        End = c.ForField("updated").To.AsDateTime().ToString("dd.MM.yy hh:mm")
                    });
                }
            }
            return res;

            /*var rng = new Random();
            return Enumerable.Range(1, 7).Select(index => new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index).ToString("D"),
                /*TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });*/
        }

        public class ResultData
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
        }
    }
}
