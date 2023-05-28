using DataAccessLayer.Abstract;
using DataAccessLayer.Context;
using EntityLayer.Entities;
using EralpAPI.Helper;
using EralpAPI.JWT;
using EralpAPI.Models.Auth;
using EralpAPI.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EralpAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        public AppDbContext Context { get; set; }
        public JwtSettings JwtSettings { get; set; }
        private readonly IGenericRepository<User> _repository;
        public AuthController(AppDbContext context, IGenericRepository<User> repository)
        {
            this.Context = context;
            this._repository = repository;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public IActionResult Login(string userName, string password)
        {
            try
            {
                var getUserDetails = Context.Set<User>().FirstOrDefault(x => x.UserName.ToLower() == userName);

                if (getUserDetails == null)
                {
                    return NotFound();
                }

                if (getUserDetails.Password != password)
                {
                    return StatusCode(500, "invalid password");
                }
                var tokenHandler = new JwtSecurityTokenHandler();
                
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSetting["JWT:Secret"]));
                JwtSecurityToken jwtSecurityToken = new(
                    claims: GenerateClaims(getUserDetails),
                    issuer: ConfigurationManager.AppSetting["JWT:ValidIssuer"],
                    audience: ConfigurationManager.AppSetting["JWT:ValidAudience"],
                    expires: DateTime.Now.AddMinutes(6),
                    signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
                );

                var tokenAsStr = tokenHandler.WriteToken(jwtSecurityToken);

                UserResponseDto dto = new UserResponseDto()
                {
                    UserId = getUserDetails.Id,
                    UserName = getUserDetails.UserName,
                    UserToken = tokenAsStr.ToString()
                };

                return Ok(dto);
            }
            catch (System.Exception)
            {
                return StatusCode(500, "internal server error!");
            }

        } //Authanticate 

        [AllowAnonymous]
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> AddUser([FromBody] UserDto userDto) // add user
        {
            try
            {
                User user = new User{
                    UserName = userDto.UserName,
                    Email = userDto.Email,
                    Password = userDto.Password
                };

                await _repository.Add(user);
                return Ok();
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Route("UpdateUser/{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UserDto userDto)
       {
            try
            {
                User user = new User
                {
                    UserName = userDto.UserName,
                    Email = userDto.Email,
                    Password = userDto.Password,
                    Id=id
                };

                var existingProduct = await _repository.Update(id, user);

                if (existingProduct == null)
                {
                    return NotFound();
                }
                return Ok(existingProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet]
        [Route("GetMyDetails")]
        [Authorize]
        public async Task<IActionResult> GetMyDetails(long id) //get my user detail
        {
            try
            {
                var getMyDetail = await _repository.GetById(id);
                return Ok(getMyDetail);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, "internal server error!");
            }
        }

        private static IEnumerable<Claim> GenerateClaims(User user)
        {
            var claims = new List<Claim>();

            // Diğer kullanıcı kimlik bilgilerini claims koleksiyonuna ekle
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName.ToString()));

            return claims;
            //    new[]
            //{
            //    new Claim(UserClaimsType.Id, user.Id.ToString()),
            //    new Claim(UserClaimsType.UserName, user.UserName),
            //};
        }
    }
}
