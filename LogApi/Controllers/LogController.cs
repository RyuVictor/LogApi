using Microsoft.AspNetCore.Mvc;
using LogApi.Models;
using LogApi.DataAccess;

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
            return Ok(exceptions);
        }

        // DELETE api/log/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteException(int id)
        {
            _dataHandler.DeleteException(id);
            return Ok();
        }
    }
}
