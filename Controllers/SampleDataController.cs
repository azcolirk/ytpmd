using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YouTrackSharp;
using YouTrackSharp.Issues;
using YouTrackSharp.AgileBoards;
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
            const string version_str = "Sprint 4";
            var connection = new UsernamePasswordConnection("http://youtrack.ispsystem.net:8080", "s.arlyapov", "0J9c5V0c9C7j3V8w");
            var issues_service = connection.CreateIssuesService();
            var issuse_project = issues_service.GetIssuesInProject("ba", "#{" + version_str + "} ", 0, 100);
            var issues = issuse_project.GetAwaiter().GetResult();
            
            DateTime version_start = new DateTime();
            var agilesettings = new AgileSettings();
            var boards_service = connection.CreateAgileBoardService();
            var boards = boards_service.GetAgileBoards().GetAwaiter().GetResult();
            foreach (var b in boards) {
                if (!b.Name.Equals("Разработка проекта BILL-admin"))
                    continue;
                Console.WriteLine("b : " + b.Name);
                foreach (var s in b.Sprints) {
                    var ss = boards_service.GetSprint(b.Id, s.Id).GetAwaiter().GetResult();
                    /*Console.WriteLine("s : " + ss.Start.ToString());
                    Console.WriteLine("v : " + ss.Version);*/
                    if (ss.Version.Equals(version_str)) {
                        version_start = new DateTime(ss.Start.Value.DateTime.Year, ss.Start.Value.DateTime.Month, ss.Start.Value.DateTime.Day, 0, 0, 0);
                    }
                    /*foreach (var p in b.Projects) {
                        Console.WriteLine("p : " + p.Id);
                        if (p.Id.Equals("ba") && s.Version.Equals(version_str)) {
                            version_start = new DateTime(s.Start.Value.DateTime.Year, s.Start.Value.DateTime.Month, s.Start.Value.DateTime.Day, 0, 0, 0);
                        }
                    }*/
                }

            }
            
            List<ResultData> res = new List<ResultData>();
            Dictionary<String, DateTime> last_update = new Dictionary<String, DateTime>();
            foreach (var i in issues) {
                var issues_changes = issues_service.GetChangeHistoryForIssue(i.Id);
                foreach (var c in issues_changes.GetAwaiter().GetResult()) {
                    String state_name = "";
                    foreach (var f in c.Fields) {
                        if (f.Name.Equals("Спринт")) {
                            foreach (var cf in f.To.AsCollection()) {
                                var r = JsonConvert.DeserializeObject<List<String>>(cf);
                                if (r.Capacity == 0)
                                    continue;
                                Console.WriteLine("     r : " + r.First() + " cmp :" + (r.First() == version_str));
                                if (r.First().Equals(version_str) && !last_update.ContainsKey(i.Id)) {
                                    if (version_start < c.ForField("updated").To.AsDateTime())
                                    last_update.Add(i.Id, c.ForField("updated").To.AsDateTime());
                                }
                            }
                        }
                        if (f.Name.Equals("State") || f.Name.Equals("Статус")) {
                            state_name = f.Name;
                            break;
                        }
                    }
                    if (state_name.Length == 0)
                        continue;
                    string state = JsonConvert.DeserializeObject<List<String>>(c.ForField(state_name).From.AsString()).First();
                    var datetime = c.ForField("updated").To.AsDateTime();
                    if (!last_update.ContainsKey(i.Id)) {
                        //last_update.Add(i.Id, new DateTime(1970, 1, 1, 0, 0, 0));
                        last_update.Add(i.Id, new DateTime(version_start.Year, version_start.Month, version_start.Day, 0, 0, 0));
                    }
                    if (datetime < last_update[i.Id])
                        continue;
                    res.Add(new ResultData {
                        Id = i.Id,
                        Status = state,
                        /*Start = "new DateTime(" + last_update[i.Id].Year.ToString() + "," + last_update[i.Id].Month.ToString() + "," + last_update[i.Id].Day.ToString() + "," +last_update[i.Id].Hour.ToString() + "," + last_update[i.Id].Minute.ToString() + "," + last_update[i.Id].Second.ToString() + ")",
                        End = "new DateTime(" + datetime.Year.ToString() + "," + datetime.Month.ToString() + "," + datetime.Day.ToString() + "," + datetime.Hour.ToString() + "," + datetime.Minute.ToString() + "," + datetime.Second.ToString() + ")" */
                        /*Start = last_update[i.Id].ToString("dd.MM.yy hh:mm"),
                        End = datetime.ToString("dd.MM.yy hh:mm")*/
                        Start = ((Int32)(last_update[i.Id].Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString(),
                        End = ((Int32)(datetime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString(),
                    });
                    last_update[i.Id] = datetime;
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
