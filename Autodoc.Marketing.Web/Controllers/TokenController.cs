using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Autodoc.Marketing.Data.Interfaces;
using Autodoc.Marketing.Data.Models;
using Autodoc.Marketing.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Autodoc.Marketing.Web.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IMarketingService _marketingService;
        private readonly IConfiguration _configuration;

        public TokenController(IMarketingService marketingService, IConfiguration configuration)
        {
            _configuration = configuration;
            _marketingService = marketingService;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Auth([FromBody]TokenRequestModel tokenRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var authResult = await _marketingService.Auth(new AuthRequest { UserName = tokenRequest.UserName, Password = tokenRequest.Password });

            if (!authResult.Authenticated)
                return Unauthorized();
            if (tokenRequest.GrantType == "password")
            return Ok(GetToken(tokenRequest, authResult));

            return Unauthorized();
        }

        private TokenResponseModel GetToken(TokenRequestModel tokenRequest, ApplicationUser appUser)
        {
            try
            {
                var now = DateTime.UtcNow;
                var claims = new Claim[]
                {
                   new Claim(JwtRegisteredClaimNames.Sub, appUser.Id.ToString()),
                   new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
                   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var expirationInMinutes = _configuration.GetValue<int>("Auth:Jwt:TokenExpirationInMinutes");
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Auth:Jwt:Key")));

                JwtSecurityToken jwtSecurityToken =
                    new JwtSecurityToken(issuer: _configuration.GetValue<string>("Auth:Key:Issuer"),
                    audience: _configuration.GetValue<string>("Auth:Key:Audience"),
                    claims: claims,
                    notBefore: now,
                    expires: now.Add(TimeSpan.FromMinutes(expirationInMinutes)),
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

                var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

                return new TokenResponseModel
                {
                    Token = token,
                    Expiration = expirationInMinutes,
                    Success = true,
					Id = appUser.Id,
					Name = appUser.Name,
					Email = appUser.Email
                };
            }
            catch(Exception ex)
            {
                return new TokenResponseModel { Success = false, Message = ex.Message };
            }
        }
    }
}
