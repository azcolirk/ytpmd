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
using YouTrackSharp.TimeTracking;
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

        public class BoardsData {
            [JsonProperty("boards")]
            public List<BoardData> Boards;
            [JsonProperty("projects")]
            public List<string> Projects;
        }

        public class BoardData {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("opened")]
            public string Opened { get; set; }
            [JsonProperty("closed")]
            public string Closed { get; set; }
            [JsonProperty("sprint")]
            public List<SprintData> Sprints;
        }
        public class SprintData {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("task_count")]
            public string TaskCount { get; set; }
            [JsonProperty("start")]
            public string Start { get; set; }
            [JsonProperty("end")]
            public string End { get; set; }
        }

    // const boards = [
    //   {name: "Доска 1", opened: 900, closed: 256, sprint: [
    //     {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
    //     {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
    //     {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
    //     {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
    //   ]},
    //   {name: "Доска 2", opened: 500, closed: 77, sprint: [
    //     {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
    //     {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
    //     {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
    //     {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
    //   ]},
    //   {name: "Доска 3", opened: 394, closed: 123, sprint: [
    //     {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
    //     {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
    //     {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
    //     {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
    //   ]},
    //   {name: "Доска 4", opened: 375, closed: 321, sprint: [
    //     {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
    //     {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
    //     {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
    //     {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
    //   ]},
    // ]

        [HttpGet("[action]")]
        public async Task<BoardsData> Boards()
        {
            YouTrackCredential credential;
            using (StreamReader r = new StreamReader("credential.json"))
            {
                credential = JsonConvert.DeserializeObject<YouTrackCredential>(r.ReadToEnd());
            }
            var connection = new UsernamePasswordConnection("http://youtrack.ispsystem.net:8080", credential.username, credential.password);

            // Поиск параметров спринта (время начала)
            var boards_service = connection.CreateAgileBoardService();
            var boards = await boards_service.GetAgileBoards();
            Console.WriteLine(boards);
            var all_boards = new List<BoardData>();
            foreach (var b in boards) {
                if (b.Projects.Count == 0)
                    continue;
                if (b.Sprints.Count == 0)
                    continue;
                bool has_billmgr_project = false;
                foreach (var p in b.Projects) {
                    if (p.Id == "ba" || p.Id == "bc") {
                        has_billmgr_project = true;
                        break;
                    }
                }
                if (has_billmgr_project == false)
                    continue;
                var board_data = new BoardData();
                board_data.Name = b.Name;
                Console.WriteLine(JsonConvert.SerializeObject(b));
                var _sprints = new List<Task<Sprint>>();
                foreach (var s in b.Sprints) {
                    Console.WriteLine(JsonConvert.SerializeObject(s));
                    if (b.Id.Length != 0 && s.Id.Length != 0) {
                        _sprints.Add(boards_service.GetSprint(b.Id, s.Id));
                    }
                }
                if (_sprints.Count == 0)
                    continue;
                var sprints = await Task.WhenAll(_sprints);
                board_data.Sprints = new List<SprintData>();

                foreach (var s in sprints.Reverse()) {
                    if (s.Start.HasValue == false || s.Finish.HasValue == false)
                        continue;

                    Console.WriteLine(JsonConvert.SerializeObject(s));
                    var sprint = new SprintData();
                    sprint.Name = s.Version;
                    sprint.Start = s.Start.Value.ToUnixTimeSeconds().ToString();
                    sprint.End = s.Finish.Value.ToUnixTimeSeconds().ToString();
                    board_data.Sprints.Add(sprint);
                }
                if (board_data.Sprints.Count == 0)
                    continue;
                all_boards.Add(board_data);
            }

            return new BoardsData {
                Boards = all_boards,
                Projects = new List<string>()
            };
        }

        [HttpGet("[action]/{board_str}/{version_str}")]
        public async Task<ResultData> Dashboard(string board_str, string version_str)
        {
            // const string version_str = "Sprint 4";
            // const string board_str = "Разработка проекта BILL-admin";
            Console.WriteLine(board_str);
            Console.WriteLine(version_str);

            YouTrackCredential credential;
            using (StreamReader r = new StreamReader("credential.json"))
            {
                credential = JsonConvert.DeserializeObject<YouTrackCredential>(r.ReadToEnd());
            }
            var connection = new UsernamePasswordConnection("http://youtrack.ispsystem.net:8080", credential.username, credential.password);

            // Поиск параметров спринта (время начала)
            var boards_service = connection.CreateAgileBoardService();
            var boards = await boards_service.GetAgileBoards();

            AgileSettings board = null;
            foreach (var b in boards) {
                if (b.Name.Equals(board_str)) {
                    board = b;
                    break;
                }
            }

            if (board == null || board.Id.Length == 0)
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
                    break;
				}
			}

            if (sprint.Id.Length == 0)
                throw new SystemException("sprint not found");

			var version_start = new DateTime(sprint.Start.Value.DateTime.Year, sprint.Start.Value.DateTime.Month, sprint.Start.Value.DateTime.Day, 0, 0, 0);
			var version_end = new DateTime(sprint.Finish.Value.DateTime.Year, sprint.Finish.Value.DateTime.Month, sprint.Finish.Value.DateTime.Day, 23, 59, 59);

            var issues_service = connection.CreateIssuesService();
            var ba_issues = await issues_service.GetIssuesInProject("ba", "Спринт: {" + version_str + "} и Подсистема: Разработка и тег: -Docs и Подзадача: -ba-842 и Тип: -{Пользовательская история}", 0, 250);
            var bc_issues = await issues_service.GetIssuesInProject("bc", "Спринт: {" + version_str + "} и Подсистема: Backend и тег: -Docs", 0, 250);
            var issues = ba_issues.Concat(bc_issues);

			var _issues_changes = new List<Task<(string, IEnumerable<Change>)>>();

			foreach (var i in issues) {
				_issues_changes.Add(Task<(string, IEnumerable<Change>)>
					.Run(async () => {
                        return (i.Id, await issues_service.GetChangeHistoryForIssue(i.Id));
                    }));
			}

			var issues_changes = (await Task.WhenAll(_issues_changes)).ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
            
            var res_list = new List<IssueStateItem>();
            var last_update = new Dictionary<String, DateTime>();
            var sprint_issues = new List<string>();

            foreach (var i in issues) {
                Console.WriteLine("t : " + i.Id);
                IssueStateItem last_state = null;
                bool in_sprint = false; // Спринт
                var _fields = i.Fields;
                DateTime? issue_start_date = null;
                foreach(var f in _fields) {
                    if (f.Name.Equals("Спринт")) {
                        foreach(var v in f.AsCollection()) {
                            Console.WriteLine(v + " <> " + version_str);
                            if (v == version_str) {
                                in_sprint = true;
                                break;
                            }
                        }
                    }
                    if (f.Name.Equals("created")) {
                        issue_start_date = f.AsDateTime();
                        Console.WriteLine(issue_start_date.Value.ToString());
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
                                
                                Console.WriteLine("     r : " + r.First() + " cmp :" + (r.First() == version_str));
                                if (r.First().Equals(version_str) && !last_update.ContainsKey(i.Id)) {
                                    in_sprint = true;
                                    Console.WriteLine(c.ForField("updated").To.AsDateTime());
                                    if (version_start < c.ForField("updated").To.AsDateTime())
                                        last_update.Add(i.Id, c.ForField("updated").To.AsDateTime());
                                    else
                                        last_update.Add(i.Id, issue_start_date.HasValue && issue_start_date.Value > version_start ? issue_start_date.Value : version_start);
                                }
                            }
                        }
                        if (f.Name.Equals("State") || f.Name.Equals("Статус")) {
                            state_name = f.Name;
                            Console.WriteLine("     s : " + JsonConvert.DeserializeObject<List<String>>(c.ForField(state_name).From.AsString()).First());
                            break;
                        }
                    }
                    Console.WriteLine("state_name: " + state_name);
                    if (state_name.Length == 0)
                        continue;
                    string state = JsonConvert.DeserializeObject<List<String>>(c.ForField(state_name).From.AsString()).First();
                    var datetime = c.ForField("updated").To.AsDateTime();
                    if (!last_update.ContainsKey(i.Id)) {
                        last_update.Add(i.Id, issue_start_date.HasValue && issue_start_date.Value > version_start ? issue_start_date.Value : version_start);
                    }

                    last_state = new IssueStateItem {
                        Id = i.Id,
                        Summary = i.Summary,
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

                    res_list.Add( new IssueStateItem {
                        Id = i.Id,
                        Summary = i.Summary,
                        Status = state,
                        Start = GetUnixTimeString(last_update[i.Id]),
                        End = GetUnixTimeString(datetime),
                    });
                    last_update[i.Id] = datetime;
                }
                Console.WriteLine("id: " + i.Id + " 1 - " + in_sprint);
                if (last_state != null || in_sprint) {
                    if (sprint_issues.IndexOf(i.Id) == -1) {
                        sprint_issues.Add(i.Id);
                    }
                }
                if (last_state != null) {
                    Console.WriteLine("id: " + i.Id + " 2");
                    last_state.Start = GetUnixTimeString(last_update[i.Id]);
                    last_state.End = GetUnixTimeString(DateTime.Now);
                    res_list.Add(last_state);
                    Console.WriteLine("     last state : " + last_state.Status);
                } else if (in_sprint) {
                    Console.WriteLine("id: " + i.Id + " 3");
                    Console.WriteLine("last_state is null, get currenct state");
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
                        var state = new IssueStateItem {
                            Id = i.Id,
                            Summary = i.Summary,
                            Status = status,
                            Start = GetUnixTimeString(issue_start_date.HasValue && issue_start_date.Value > version_start ? issue_start_date.Value : version_start),
                            End = GetUnixTimeString(DateTime.Now)
                        };
                        res_list.Add(state);
                    }
                }
            }

            var _work_items = new List<Task<(string, IEnumerable<WorkItem>)>>();
            var time_tracking_service = connection.CreateTimeTrackingService();
            foreach(var i in sprint_issues) {
                _work_items.Add(Task<IEnumerable<WorkItem>>
                    .Run(async () => {
                        return (i, await time_tracking_service.GetWorkItemsForIssue(i));
                    })
                );
            }

            var work_items = (await Task.WhenAll(_work_items)).ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
            var work_item_list = new Dictionary<string, List<WorkItem>>();
            foreach (var work in work_items)
            {
                if (!work_item_list.ContainsKey(work.Key)) {
                    work_item_list.Add(work.Key, new List<WorkItem>());
                }
                work_item_list[work.Key].AddRange(work.Value);
            }
            Console.WriteLine(work_item_list);
            var work_list = new List<IssueWorkItem>();
            foreach(var item_works in work_item_list) {
                foreach(var item in item_works.Value) {
                    if (item.Date.HasValue && item.Date.Value >= version_start && item.Date.Value <= version_end) {
                        var work = new IssueWorkItem();
                        work.Id = item_works.Key;
                        work.Date = GetUnixTimeString(item.Date.Value);
                        work.Duration = item.Duration.TotalMinutes.ToString();
                        Console.WriteLine(work.Id);
                        Console.WriteLine(item.Date);
                        Console.WriteLine(item.Duration);
                        Console.WriteLine(item.Description);
                        Console.WriteLine(item.Author.Login);
                        Console.WriteLine(item.WorkType);
                        if (item.WorkType != null) {
                            work.WorkType = item.WorkType.Name;
                        }
                        work.Description = item.Description;
                        work.Author = item.Author.Login;
                        work_list.Add(work);
                    }
                }
            }
            
            return new ResultData {
                ListData = res_list,
                WorkData = work_list,
                Sprint = version_str,
                SprintStart = GetUnixTimeString(version_start),
                SprintEnd = GetUnixTimeString(version_end),
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
            public List<IssueStateItem> ListData;
            [JsonProperty("workdata")]
            public List<IssueWorkItem> WorkData;

        }

        public class IssueStateItem {
            public string Id { get; set; }
            public string Summary { get; set; }
            public string Status { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
        }

        public class IssueWorkItem {
            public string Id { get; set; }
            public string Date { get; set; }
            public string Duration { get; set; }
            public string WorkType { get; set; }
            public string Description { get; set; }
            public string Author { get; set; }
        }
    }
}
