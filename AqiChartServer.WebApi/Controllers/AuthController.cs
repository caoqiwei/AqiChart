using AqiChart.Model.Dto;
using AqiChartServer.DB.Enties;
using AqiChartServer.DB.Interface;
using AqiChartServer.WebApi.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AqiChartServer.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUserBiz _userBiz;
        public AuthController(IConfiguration config, IUserBiz userBiz)
        {
            _config = config;
            _userBiz = userBiz;
        }

        [HttpPost("login")]
        public UserDto Login(UserLoginDto userDto)
        {
            string pw = HashPassword(userDto.Password);
            var user = _userBiz.GetUsers(userDto.UserName);
            if (user == null|| !VerifyPassword(userDto.Password, user.PasswordHash)) throw new MyException("用户名或密码错误！");
            
            
            if (user.Status == UserStatus.online.ToString()) throw new MyException("该用户已登录，不能重复登录！");
            var token = GenerateJwtToken(user);
            return new UserDto { Token = token, AvatarUrl = user.AvatarUrl, UserName = user.UserName, NickName = user.NickName, Email = user.Email,Id = user.UserId };
        }

        [Authorize]
        [HttpPost("UserInfo")]
        public object UserInfo()
        {
            ChatUsers user = _userBiz.GetUserInfo(HttpContext.User.Identity.Name);

            return new { AvatarUrl = user.AvatarUrl, UserName = user.UserName, NickName = user.NickName, Email = user.Email };
        }

        private string GenerateJwtToken(ChatUsers user)
        {
            var tokenKey = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var key = new SymmetricSecurityKey(tokenKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: new[] { new Claim(ClaimTypes.NameIdentifier, user.UserId), new Claim(ClaimTypes.Name, user.UserId) },
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 生成盐值
        private byte[] GenerateSalt(int size = 32)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[size];
            rng.GetBytes(salt);
            return salt;
        }

        // 使用PBKDF2哈希密码
        private string HashPassword(string password, int iterations = 100000)
        {
            byte[] salt = GenerateSalt();

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256);

            byte[] hash = pbkdf2.GetBytes(32); // 32字节 = 256位

            // 组合盐值和哈希值存储
            return $"{iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        // 验证密码
        private bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] originalHash = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256);

            byte[] computedHash = pbkdf2.GetBytes(32);

            // 使用恒定时间比较防止时序攻击
            return CryptographicOperations.FixedTimeEquals(computedHash, originalHash);
        }

    }
}
