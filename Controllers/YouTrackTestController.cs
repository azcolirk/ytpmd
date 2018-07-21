using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YouTrackSharp;
using ytpmd.Models;
using YouTrackSharp.Projects;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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


		[HttpGet("[action]"), Authorize]
        public IEnumerable<Project> Projects()
        {
			var user = _userManager.GetUserAsync(HttpContext.User).GetAwaiter().GetResult();

            var connection = new UsernamePasswordConnection(_configuration["YouTrackURL"], user.YTLogin, user.YTPassword);

			var projectsService = connection.CreateProjectsService();
			var projectsForCurrentUser = projectsService.GetAccessibleProjects().GetAwaiter().GetResult();

			return projectsForCurrentUser;
        }

    }

}