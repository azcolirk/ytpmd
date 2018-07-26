using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YouTrackSharp;
using YouTrackSharp.Issues;
using YouTrackSharp.AgileBoards;
using Newtonsoft.Json;
using System.IO;

namespace ytpmd.Controllers
{
    public class YouTrackCredential
    {
        public string username;
        public string password;
    }

    [Route("")]
    public class QueryController : Controller
    {
        [Route("api/sprint/summary")]
        [HttpGet]
        //public IEnumerable<WeatherForecast> WeatherForecasts()
        public ResultData Get()
        {
            const string version_str = "Sprint 4";
            YouTrackCredential credential;
            using (StreamReader r = new StreamReader("credential.json"))
            {
                credential = JsonConvert.DeserializeObject<YouTrackCredential>(r.ReadToEnd());
            }
            var connection = new UsernamePasswordConnection("http://youtrack.ispsystem.net:8080", credential.username, credential.password);
            var issues_service = connection.CreateIssuesService();
            var ba_issues = issues_service.GetIssuesInProject("ba", "#{" + version_str + "}", 0, 250).GetAwaiter().GetResult();
            var issues = issues_service.GetIssuesInProject("bc", "#{" + version_str + "} и Подсистема: Backend", 0, 250).GetAwaiter().GetResult();

            // Поиск параметров спринта (время начала)
            DateTime version_start = new DateTime();
            DateTime version_end = new DateTime();
            var agilesettings = new AgileSettings();
            var boards_service = connection.CreateAgileBoardService();
            var boards = boards_service.GetAgileBoards().GetAwaiter().GetResult();
            foreach (var b in boards) {
                if (!b.Name.Equals("Разработка проекта BILL-admin"))
                    continue;
                //Console.WriteLine("b : " + b.Name);
                foreach (var s in b.Sprints) {
                    var ss = boards_service.GetSprint(b.Id, s.Id).GetAwaiter().GetResult();
                    /*Console.WriteLine("s : " + ss.Start.ToString());
                    Console.WriteLine("v : " + ss.Version);*/
                    if (ss.Version.Equals(version_str)) {
                        version_start = new DateTime(ss.Start.Value.DateTime.Year, ss.Start.Value.DateTime.Month, ss.Start.Value.DateTime.Day, 0, 0, 0);
                        version_end = new DateTime(ss.Finish.Value.DateTime.Year, ss.Finish.Value.DateTime.Month, ss.Finish.Value.DateTime.Day, 0, 0, 0);
                    }
                    /*foreach (var p in b.Projects) {
                        Console.WriteLine("p : " + p.Id);
                        if (p.Id.Equals("ba") && s.Version.Equals(version_str)) {
                            version_start = new DateTime(s.Start.Value.DateTime.Year, s.Start.Value.DateTime.Month, s.Start.Value.DateTime.Day, 0, 0, 0);
                        }
                    }*/
                }

            }
            
            List<ResultList> res_list = new List<ResultList>();
            Dictionary<String, DateTime> last_update = new Dictionary<String, DateTime>();
            foreach (var i in issues) {
                Console.WriteLine("t : " + i.Id);
                var issues_changes = issues_service.GetChangeHistoryForIssue(i.Id);
                ResultList last_state = null;
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
                            Console.WriteLine("     s : " + JsonConvert.DeserializeObject<List<String>>(c.ForField(state_name).From.AsString()).First());
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

                    last_state = new ResultList {
                        Id = i.Id,
                        Status = JsonConvert.DeserializeObject<List<String>>(c.ForField(state_name).To.AsString()).First(),
                        Start = ((Int32)(datetime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString()
                    };

                    if (datetime < last_update[i.Id]) {
                        continue;
                    } else if (Convert.ToInt32(last_state.Start) < (Int32)(last_update[i.Id].Subtract(new DateTime(1970, 1, 1))).TotalSeconds) {
                        Console.WriteLine("     last stte : " + last_state.Status);
                        last_state.Start = ((Int32)(last_update[i.Id].Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
                        last_state.End = ((Int32)(datetime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
                        res_list.Add(last_state);
                        last_state = null;
                    }

                    res_list.Add( new ResultList {
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
                if (last_state != null) {
                    last_state.Start = ((Int32)(last_update[i.Id].Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
                    last_state.End = ((Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
                    res_list.Add(last_state);
                    Console.WriteLine("     last stte : " + last_state.Status);
                }
            }
            
            return new ResultData {
                ListData = res_list,
                Sprint = version_str,
                SprintStart = version_start.ToString("dd.MM.yy"),
                SprintEnd = version_end.ToString("dd.MM.yy"),
                Project = "Разработка проекта BILL-admin"
                };
        }

        public class ResultData {
            [JsonProperty("sprint")]
            public string Sprint { get; set; }
            [JsonProperty("sprintstart")]
            public string SprintStart { get; set; }
            [JsonProperty("sprintend")]
            public string SprintEnd { get; set; }
            [JsonProperty("project")]
            public string Project { get; set; }
            [JsonProperty("listdata")]
            public List<ResultList> ListData;

        }

        public class ResultList {
            public string Id { get; set; }
            public string Status { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
        }
    }
}
