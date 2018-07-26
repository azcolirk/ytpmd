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
            const string board_str = "Разработка проекта BILL-admin";

            YouTrackCredential credential;
            using (StreamReader r = new StreamReader("credential.json"))
            {
                credential = JsonConvert.DeserializeObject<YouTrackCredential>(r.ReadToEnd());
            }
            var connection = new UsernamePasswordConnection("http://youtrack.ispsystem.net:8080", credential.username, credential.password);

            // Поиск параметров спринта (время начала)
            var agilesettings = new AgileSettings();
            var boards_service = connection.CreateAgileBoardService();
            var boards = await boards_service.GetAgileBoards();

            AgileSettings board = new AgileSettings();
            foreach (var b in boards) {
                if (b.Name.Equals(board_str)) {
                    board = b;
                    break;
                }
            }

            if (board.Id.Length == 0)
                throw new SystemException("board not found");

            var _sprints = new List<Task<Sprint>>();
            foreach (var s in board.Sprints) {
				_sprints.Add(boards_service.GetSprint(board.Id, s.Id));
            }

			var sprints = await Task.WhenAll(_sprints);

            var sprint = new Sprint();
			foreach (var s in sprints) {
				if (s.Version.Equals(version_str)) {
                    sprint = s;
				}
			}

            if (sprint.Id.Length == 0)
                throw new SystemException("sprint not found");

			var version_start = new DateTime(sprint.Start.Value.DateTime.Year, sprint.Start.Value.DateTime.Month, sprint.Start.Value.DateTime.Day, 0, 0, 0);
			var version_end = new DateTime(sprint.Finish.Value.DateTime.Year, sprint.Finish.Value.DateTime.Month, sprint.Finish.Value.DateTime.Day, 23, 59, 59);

            var issues_service = connection.CreateIssuesService();
            var ba_issues = await issues_service.GetIssuesInProject("ba", "#{" + version_str + "} и Подсистема: -Тестирование и -Docs и Подзадача: -ba-842 и Тип: -{Пользовательская история} )", 0, 250);
            var bc_issues = await issues_service.GetIssuesInProject("bc", "#{" + version_str + "} и Подсистема: Backend и -Docs )", 0, 250);
            var issues = ba_issues.Concat(bc_issues);

			var _issues_changes = new List<Task<(string, IEnumerable<Change>)>>();

			foreach (var i in issues) {
				_issues_changes.Add(Task<(string, IEnumerable<Change>)>
					.Run(async () => {
                        return (i.Id, await issues_service.GetChangeHistoryForIssue(i.Id));
                    }));
			}

			var issues_changes = (await Task.WhenAll(_issues_changes)).ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            
            var res_list = new List<ResultList>();
            var last_update = new Dictionary<String, DateTime>();
            foreach (var i in issues) {
                Console.WriteLine("t : " + i.Id);
                ResultList last_state = null;
                bool in_sprint = false; // Спринт
                var _fields = i.Fields;
                foreach(var f in _fields) {
                    if (f.Name.Equals("Спринт")) {
                        foreach(var v in f.AsCollection()) {
                            if (v == version_str) {
                                in_sprint = true;
                                break;
                            }
                        }
                    }
                }

                foreach (var c in issues_changes[i.Id] ) {
                    String state_name = "";
                    foreach (var f in c.Fields) {
                        if (f.Name.Equals("Спринт")) {
                            foreach (var cf in f.To.AsCollection()) {
                                var r = JsonConvert.DeserializeObject<List<String>>(cf);
                                if (r.Capacity == 0)
                                    continue;
                                in_sprint = r.First() == version_str;
                                Console.WriteLine("     r : " + r.First() + " cmp :" + (r.First() == version_str));
                                if (r.First().Equals(version_str) && !last_update.ContainsKey(i.Id)) {
                                    Console.WriteLine(c.ForField("updated").To.AsDateTime());
                                    if (version_start < c.ForField("updated").To.AsDateTime())
                                        last_update.Add(i.Id, c.ForField("updated").To.AsDateTime());
                                    else
                                        last_update.Add(i.Id, version_start);
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
                        last_update.Add(i.Id, version_start);
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
                        Start = GetUnixTimeString(last_update[i.Id]),
                        End = GetUnixTimeString(datetime),
                    });
                    last_update[i.Id] = datetime;
                }
                if (last_state != null) {
                    last_state.Start = GetUnixTimeString(last_update[i.Id]);
                    last_state.End = GetUnixTimeString(DateTime.Now);
                    res_list.Add(last_state);
                    Console.WriteLine("     last state : " + last_state.Status);
                } else if (in_sprint) {
                    var fields = i.Fields;
                    string status = "";
                    foreach(var f in fields) {
                        if (f.Name.Equals("State") || f.Name.Equals("Статус")) {
                            Console.WriteLine("     s : " + f.Value.ToString());
                            status = f.AsString();
                            break;
                        }
                    }
                    if (status != "") {
                        last_state = new ResultList {
                            Id = i.Id,
                            Status = status,
                            Start = GetUnixTimeString(version_start),
                            End = GetUnixTimeString(DateTime.Now)
                        };
                        res_list.Add(last_state);
                    }
                }
            }
            
            return new ResultData {
                ListData = res_list,
                Sprint = version_str,
                SprintStart = version_start.ToString("dd.MM.yy"),
                SprintEnd = version_end.ToString("dd.MM.yy"),
                Project = board.Name
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
