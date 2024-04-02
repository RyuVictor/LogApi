using LogApi.DataAccess.Exceptions;
using LogApi.DataAccess.Users;
using LogApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserHandler _userHandler;

        public UserController(UserHandlerFactory userHandlerFactory)
        {
            _userHandler = userHandlerFactory.CreateUserHandler();
        }
        [HttpPost]
        public IActionResult AddUser(LoggedInUser user)
        {
            _userHandler.AddUser(user);
            return Ok();
        }
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userHandler.GetAllUsers();
            return Ok(users);
        }
        [HttpDelete]
        public IActionResult DeleteUser(int id)
        {
            _userHandler.DeleteUser(id);
            return Ok();
        }
    }
}
