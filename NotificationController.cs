using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;

namespace Jellyfin.Plugin.NewEpisodeNotifier
{
    [ApiController]
    [Route("NewEpisodeNotifier")]
    public class NotificationController : ControllerBase
    {
        private readonly IUserManager _userManager;

        public NotificationController(IUserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("ClientScript.js")]
        public ActionResult GetClientScript()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Jellyfin.Plugin.NewEpisodeNotifier.ClientScript.js";
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return NotFound();
            return File(stream, "application/javascript");
        }

        [HttpGet("Check")]
        public ActionResult<object> CheckNotifications([FromQuery] string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return BadRequest();
            var user = _userManager.GetUserById(userGuid);
            if (user == null) return NotFound();

            return Ok(new { 
                hasNewContent = Plugin.Instance.HasNewContent(),
                count = Plugin.Instance.SeriesWithNewEpisodes.Count
            });
        }
    }
}