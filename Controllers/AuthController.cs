using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RecipeApp.db_data;
using RecipeApp.DTO;

// using RecipeApp.DTO;
using RecipeApp.Models;

namespace RecipesApp.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {
    public IConfiguration Configuration { get; set; }
    public RecipeapiContext Context { get; set; }

    public AuthController(IConfiguration configuration, RecipeapiContext context) {
      Configuration = configuration;
      Context = context;
    }
    //  public AuthController(IConfiguration configuration) {
    //   Configuration = configuration;
      
    // }
   // [HttpGet]
    // public string get(){
    //   var section = Configuration.GetSection("JWT");
    //   var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(section.GetValue<string>("Token")));
    //   var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    //   var issuer = section.GetValue<string>("Issuer");
    //   var audience = section.GetValue<string>("Audience");
    //   var jwtValidity = DateTime.Now.AddHours(section.GetValue<int>("ExpirationHours"));
    //   var token = new JwtSecurityToken(
    //     issuer,
    //     audience,
    //     new List<Claim>() {
    //       new Claim("id", user.Id.ToString()),
    //       new Claim("username", user.Username),
    //     },
    //     expires: jwtValidity,
    //     signingCredentials: creds
    //   );
    // return new JwtSecurityTokenHandler().WriteToken(token);  
    // }
    string CreateToken(User user)
    {
      var section = Configuration.GetSection("JWT");
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(section.GetValue<string>("Token")));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
      var issuer = section.GetValue<string>("Issuer");
      var audience = section.GetValue<string>("Audience");
      var jwtValidity = DateTime.Now.AddHours(section.GetValue<int>("ExpirationHours"));


      var token = new JwtSecurityToken(
        issuer,
        audience,
        new List<Claim>() {
          new Claim("id", user.Id.ToString()),
          new Claim("username", user.Username),
        },
        expires: jwtValidity,
        signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("/Login")]
    public async Task<ActionResult<string>> Login(LoginUserDTO User) {
      var foundUser = await Context
        .Users
        .Where(u => u.Username == User.username)
        .FirstOrDefaultAsync();
      if (foundUser == null) {
        return NotFound(new { message = "Not Found" });
      }

      var valid = BCrypt.Net.BCrypt.Verify(User.password, foundUser.Password);

      if (valid) {
        return Ok(new { token = CreateToken(foundUser) });
      }

      return BadRequest(new { message = "Bad Password" });
    }
  
  }
}