using Microsoft.AspNetCore.Mvc;
using UserManagement.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using SharedModels;
using RegionalUserService.Interfaces;
using RealTimeBroadcast;

namespace Communication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunicationController : ControllerBase
    {
        private readonly IUserManagement _userManagement;

        private IRegionalUserService GetRegionalUserServiceProxy(string region)
        {
            return ServiceProxy.Create<IRegionalUserService>( new Uri("fabric:/SnakesGameApp/RegionaluserService"),
                new ServicePartitionKey(region));
        }

        public CommunicationController(IUserManagement userManagement)
        {
            _userManagement = userManagement;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserModel model)
        {
            var isNewUser = await _userManagement.RegisterUser(model.Email, model.Password, model.Nickname, model.Region, model.HighestScore);
            if(!isNewUser)
            {
                return Ok(new { message = "User already exists!" });
            }
            Console.WriteLine($"Creating service proxy with URI: fabric:/SnakesGameApp/RegionaluserService, Region: {model.Region}");
            var regionalUserServiceProxy = GetRegionalUserServiceProxy(model.Region);
            await regionalUserServiceProxy.AddUserToRegion(model);
            return Ok(new { message = "User registered!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginModel model)
        {
            var regionalUserServiceProxy = GetRegionalUserServiceProxy(model.Region);
            var userExists = await regionalUserServiceProxy.IsUserInRegion(model.Email);
            if (!userExists)
            {
                return Ok(new { message = "User does not exist!", user = new UserModel() });
            }
            var correctPassword = await _userManagement.IsCorrectPassword(model.Email, model.Password);
            if(!correctPassword)
            {
                return Ok(new { message = "Incorrect password!", user = new UserModel() });
            }
            await _userManagement.LoginUser(model.Email, model.Password);
            var userData = await _userManagement.GetUserData(model.Email);
            var clientModel = new ClientModel(userData.Item2.Email, userData.Item2.Nickname, userData.Item2.Region, userData.Item2.HighestScore);
            return Ok(new { message = "User exists!", user = userData.Item2});
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            await _userManagement.UpdatePassword(model);
            return Ok(new { message = "Score updated and leaderboards refreshed!" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            return Ok(new { message = "Score updated and leaderboards refreshed!" });
        }
    }
  
}
