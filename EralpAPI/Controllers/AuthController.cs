using DataAccessLayer.Context;
using EntityLayer.Entities;
using EralpAPI.JWT;
using EralpAPI.Models.Auth;
using EralpAPI.Statics;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        public AppDbContext Context { get; set; }
        public JwtSettings JwtSettings { get; set; }

        public AuthController(AppDbContext context, JwtSettings jwtSettings)
        {
            this.Context = context;
            this.JwtSettings = jwtSettings;
        }

        [HttpPost]
        public IActionResult GetProducts(int currentPage, int pageSize, string productName)
        {
            // Pagination

            var queryable = Context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(productName))
            {
                queryable = queryable.Where(x => x.Name.Contains(productName));
            }

            var response = queryable
                .OrderByDescending(x => x.Id)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            return Ok();
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(string userName, string password)
        {
            try
            {
                //if (!string.IsNullOrWhiteSpace(userName))
                //{
                //    // adina gore filtre
                //}
                ////user bilgilerini getirme\\
                //Context.Products
                //    .OrderByDescending(x => x.Id)
                //    .Skip(5)
                //    .Take(5);

                var getUserDetails = Context.Set<User>().FirstOrDefault(x => x.UserName.ToLower() == userName);

                if (getUserDetails == null)
                {
                    return NotFound();
                }

                if (getUserDetails.Password != password)
                {
                    return StatusCode(500, "invalid password");
                }

                SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes(JwtSettings.Secret));

                JwtSecurityToken jwtSecurityToken = new(
                    claims: GenerateClaims(getUserDetails),
                    issuer: JwtSettings.Issuer,
                    audience: JwtSettings.Audience,
                    expires: DateTime.Now.AddMinutes(JwtSettings.TokenLifeTime.TotalMinutes),
                    signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256)
                );

                JwtSecurityTokenHandler tokenHandler = new();

                var tokenAsStr = tokenHandler.WriteToken(jwtSecurityToken);

                UserResponseDto dto = new UserResponseDto()
                {
                    UserId = getUserDetails.Id,
                    UserName = getUserDetails.UserName,
                    UserToken = tokenAsStr
                };

                return Ok(dto);
            }
            catch (System.Exception)
            {
                return StatusCode(500, "internal server error!");
            }
           
        }

        [HttpGet]
        [Route("GetMyDetails")]
        [Authorize]
        public IActionResult GetMyDetails(long id)
        {
            try
            {
                return Ok();
            }
            catch (System.Exception)
            {
                return StatusCode(500, "internal server error!");
            }
        }

        private static Claim[] GenerateClaims(User user)
        {
            return new[]
            {
                new Claim(UserClaimsType.Id, user.Id.ToString()),
                new Claim(UserClaimsType.UserName, user.UserName),
            };
        }
    }
}
