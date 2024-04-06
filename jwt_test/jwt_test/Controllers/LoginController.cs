using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace jwt_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _configuration;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private User AuthenticateUser(User user)
        {
            User _user= null;
            if(user.username =="admin" && user.password =="123456") {
                _user = new User { username = "viet hoang" };
            }
            return _user;
        }
        private string GenerateToken(User users)
        {
            var secrity = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(secrity,SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"],null,
                expires:DateTime.Now.AddMinutes(1),
                signingCredentials:credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(User user)
        {
            IActionResult respone = Unauthorized();
            var _user = AuthenticateUser(user);
          try
            {
                if (_user != null)
                {
                    var token = GenerateToken(_user);
                    respone = Ok(new { token = token });

                }
            }

        catch (Exception ex)
            {

            }
            return respone;
        }
    }
}
