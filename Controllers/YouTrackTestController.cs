using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ytpmd.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using YouTrackSharp;
using YouTrackSharp.Projects;
using YouTrackSharp.Issues;
using YouTrackSharp.AgileBoards;
using Newtonsoft.Json;
using System.IO;

namespace ytpmd.Controllers
{ 
	[Route("api/[controller]")]
	public class YouTrackTestController : Controller
    {
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IConfiguration _configuration;

		public YouTrackTestController(UserManager<ApplicationUser> userManager, IConfiguration configuration) {
			_userManager = userManager;
			_configuration = configuration;
		}

		private string GetUnixTimeString(DateTime dt)
		{
			return new DateTimeOffset(dt).ToUnixTimeSeconds().ToString();
		}


		[HttpGet("[action]"), Authorize]
        public IEnumerable<YouTrackSharp.Projects.Project> Projects()
        {
			var user = _userManager.GetUserAsync(HttpContext.User).GetAwaiter().GetResult();

            var connection = new UsernamePasswordConnection(_configuration["YouTrackURL"], user.YTLogin, user.YTPassword);

			var projectsService = connection.CreateProjectsService();
			var projectsForCurrentUser = projectsService.GetAccessibleProjects().GetAwaiter().GetResult();

			return projectsForCurrentUser;
        }

        [HttpGet("[action]")]
        //public IEnumerable<WeatherForecast> WeatherForecasts()
        public async Task<ResultData> Dashboard()
        {
            const string version_str = "Sprint 4";
            var connection = new UsernamePasswordConnection("http://youtrack.ispsystem.net:8080", "s.arlyapov", "0J9c5V0c9C7j3V8w");

            // Поиск параметров спринта (время начала)
            DateTime version_start = new DateTime();
            DateTime version_end = new DateTime();
            var agilesettings = new AgileSettings();
            var boards_service = connection.CreateAgileBoardService();
            var boards = await boards_service.GetAgileBoards();

			var _sprints = new List<Task<Sprint>>();
            foreach (var b in boards) {
                if (!b.Name.Equals("Разработка проекта BILL-admin"))
                    continue;
                //Console.WriteLine("b : " + b.Name);
                foreach (var s in b.Sprints) {
					_sprints.Add(boards_service.GetSprint(b.Id, s.Id));
                }
            }

			var sprints = await Task.WhenAll(_sprints);

			foreach (var s in sprints) {
				if (s.Version.Equals(version_str)) {
					version_start = new DateTime(s.Start.Value.DateTime.Year, s.Start.Value.DateTime.Month, s.Start.Value.DateTime.Day, 0, 0, 0);
					version_end = new DateTime(s.Finish.Value.DateTime.Year, s.Finish.Value.DateTime.Month, s.Finish.Value.DateTime.Day, 0, 0, 0);
				}
			}

            var issues_service = connection.CreateIssuesService();
            var issues = await issues_service.GetIssuesInProject("ba", "#{" + version_str + "} ", 0, 100);
          
			var _issues_changes = new List<Task<(string, IEnumerable<Change>)>>();

			foreach (var i in issues) {
				_issues_changes.Add(Task<(string, IEnumerable<Change>)>
					.Run(async () => {return (i.Id, await issues_service.GetChangeHistoryForIssue(i.Id));}));
			}

			var issues_changes = (await Task.WhenAll(_issues_changes)).ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            
            var res_list = new List<ResultList>();
            var last_update = new Dictionary<String, DateTime>();
            foreach (var i in issues) {
                Console.WriteLine("t : " + i.Id);
                ResultList last_state = null;
                foreach (var c in issues_changes[i.Id] ) {
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
                        Start = GetUnixTimeString(datetime)
                    };

                    if (datetime < last_update[i.Id]) {
                        continue;
                    } else if (Convert.ToInt32(last_state.Start) < Convert.ToInt32(GetUnixTimeString(last_update[i.Id]))) {
                        Console.WriteLine("     last state : " + last_state.Status);
                        last_state.Start = GetUnixTimeString(last_update[i.Id]);
                        last_state.End = GetUnixTimeString(datetime);
                        res_list.Add(last_state);
                        last_state = null;
                    }

                    res_list.Add( new ResultList {
                        Id = i.Id,
                        Status = state,
                        Start = ((Int32)(last_update[i.Id].Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString(),
                        End = GetUnixTimeString(datetime),
                    });
                    last_update[i.Id] = datetime;
                }
                if (last_state != null) {
                    last_state.Start = GetUnixTimeString(last_update[i.Id]);
                    last_state.End = GetUnixTimeString(DateTime.Now);
                    res_list.Add(last_state);
                    Console.WriteLine("     last state : " + last_state.Status);
                }
            }
            
            return new ResultData {
                ListData = res_list,
                Sprint = version_str,
                SprintStart = version_start.ToString("dd.MM.yy"),
                SprintEnd = version_end.ToString("dd.MM.yy"),
                Project = "Разработка проекта BILL-admin "
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
