using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MasterApi.Core.Config;

//https://weblog.west-wind.com/posts/2016/may/23/strongly-typed-configuration-settings-in-aspnet-core

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace MasterApi.Web.Controllers.v1
{
    [Route("api/{version}/settings")]
    public class ConfigurationController : Controller
    {
        private readonly AppSettings _mySettings;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationRoot _configRoot;

        public ConfigurationController(IOptions<AppSettings> settings, IConfiguration configuration, IConfigurationRoot configRoot)
        {
            _mySettings = settings.Value;
            _configuration = configuration;
            _configRoot = configRoot;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(AppSettings), 200)]
        public IActionResult Get()
        {
            return Ok(new { _mySettings });
        }

        [HttpGet("appname")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult AppName()
        {
            return Ok(new { _mySettings.ApplicationName });
        }

        [HttpGet("maxlistcount")]
        [ProducesResponseType(typeof(int), 200)]
        public IActionResult MaxListCount()
        {
            return Ok(new { _mySettings.MaxItemsPerList });
        }

        [HttpGet("appname_key")]
        [ProducesResponseType(typeof(string), 200)]
        public string AppNameKey()
        {
            return _configuration.GetValue<string>("AppSettings:ApplicationName");
        }

        [HttpGet("maxlistcount_key")]
        [ProducesResponseType(typeof(int), 200)]
        public int MaxListCountKey()
        {
            return _configuration.GetValue<int>("AppSettings:MaxItemsPerList");
        }

        [HttpGet("reloadconfig")]
        [ProducesResponseType(typeof(ActionResult), 200)]
        public ActionResult ReloadConfig()
        {
            _configRoot.Reload();

            // this should give the latest value from config
            var lastVal = _configuration.GetValue<string>("AppSettings:ApplicationName");

            return Ok(lastVal);
        }
    }

}
