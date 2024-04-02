using Microsoft.AspNetCore.Mvc;
using LogApi.Models;
using LogApi.DataAccess;
using System.Net;

namespace LogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly IDataHandler _dataHandler;

        public LogController(DataHandlerFactory dataHandlerFactory)
        {
            _dataHandler = dataHandlerFactory.CreateDataHandler();
        }

        // POST api/log
        [HttpPost]
        public IActionResult AddException(MyException exception)
        {
            _dataHandler.AddException(exception);
            return Ok();
        }

        // GET api/log
        [HttpGet]
        public IActionResult GetAllExceptions()
        {
            var exceptions = _dataHandler.GetAllExceptions();
            //GetClientIP();
            return Ok(exceptions);
        }

        // DELETE api/log/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteException(int id)
        {
            _dataHandler.DeleteException(id);
            return Ok();
        }
        [HttpPost("filterexceptionbyproperty")]
        public IActionResult FilterExceptionsByProperty([FromBody] List<MyException> exceptions, [FromQuery] string propertyName, [FromQuery] string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(propertyValue))
            {
                return BadRequest("Both propertyName and propertyValue must be provided.");
            }
            if (exceptions == null)
            {
                exceptions = new List<MyException>();
            }
            var group = _dataHandler.FilterExceptionsByProperty(exceptions, propertyName, propertyValue);
            return Ok(group);
        }
        [HttpGet("groupbyproperty")]
        public IActionResult GroupByProperty([FromQuery] string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return BadRequest("Property name must be provided.");
            }

            var group = _dataHandler.GroupExceptionsByProperty(propertyName);
            return Ok(group);
        }
        [HttpGet("getrecentexception")]
        public IActionResult GetRecentExceptions()
        {
            var exceptions = _dataHandler.GetRecentExceptions();
            return Ok(exceptions);
        }
        //[HttpGet("GetIp")]
        //public IActionResult GetClientIP()
        //{
        //    var ipAddress = HttpContext.Connection.RemoteIpAddress;
        //    if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
        //    {
        //        ipAddress = IPAddress.Parse(HttpContext.Request.Headers["X-Forwarded-For"].First());
        //    }
        //    Console.WriteLine($"Client IP Address: {ipAddress}");
        //    return Ok($"Client IP Address: {ipAddress}"); 
        //}
    }
}
