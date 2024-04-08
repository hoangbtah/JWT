using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Security.Cryptography;

namespace jwt_role.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user= new User();
        public readonly IConfiguration _conficguration;
        public AuthController(IConfiguration configuration)
        {
            _conficguration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordhand, out byte[] passwordsalt);
                user.Username = request.Username;
            user.Role = request.Role;
            user.PasswordHash = passwordhand;
            user.PasswordSalt=passwordsalt;
            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            if(user.Username != request.Username)
            {
                return BadRequest("ten dang nhap sai");
            }
            if(!VerifyPasswordHash(request.Password,user.PasswordHash,user.PasswordSalt))
            {
                return BadRequest("sai mat khau");
            }
            string token = CreateToken(user);
            return Ok(token);
        }
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.Username),
                 new Claim(ClaimTypes.Role,user.Role)
            };

            var key= new SymmetricSecurityKey(System.Text.Encoding.UTF8.
                GetBytes(_conficguration.GetSection("AppSettings:Token").Value));

            var creads = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
                var token = new JwtSecurityToken(
                  claims: claims,
                  expires: DateTime.Now.AddDays(1),
                  signingCredentials: creads);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;

        }
        private void CreatePasswordHash(string password,out byte[] passwordhand,out byte[] passwordsalt)
        {
            using (var hmac= new HMACSHA512())
            {
                passwordsalt = hmac.Key;
                passwordhand=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password,  byte[] passwordhand,  byte[] passwordsalt)
        {
            using (var hmac = new HMACSHA512(user.PasswordSalt))
            {
                var comptedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return comptedHash.SequenceEqual(passwordhand);
            }
        }
        [Authorize(Roles ="Admin")]
        [HttpGet("getname")]
        public IActionResult getname()
        {
            return Ok("my name is ");
        }
        [Authorize]
        [HttpGet("getpassword")]
        public IActionResult getpassword()
        {
            return Ok("my password");
        }
        
        [HttpGet("getstring")]
        public IActionResult getstring()
        {
            return Ok("my string");
        }

    }
}
