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
			{
				return BadRequest(ModelState);
			}
            
			if (tokenRequest.GrantType == "password")
			{
				var appUser = await _marketingService.Auth(new AuthRequest { UserName = tokenRequest.UserName, Password = tokenRequest.Password });

				if (!appUser.Authenticated)
				{
					return Unauthorized();
				}
				return Ok(await GetToken(tokenRequest, appUser));
			}

			if (tokenRequest.GrantType == "refresh_token")
			{
				return await RefreshToken(tokenRequest);
			}

            return Unauthorized();
        }
		
		private async Task<IActionResult> RefreshToken(TokenRequestModel tokenRequest)
		{
			var tokensForUser = await _marketingService.TokensByClientForUser(tokenRequest.ClientId, tokenRequest.UserId);

			if (tokensForUser == null || !tokensForUser.Any(t => string.Equals(t.Value, tokenRequest.RefreshToken,
				                                               StringComparison.Ordinal)))
			{
				return Unauthorized();
			}

			await _marketingService.DeleteUserTokens(int.Parse(tokenRequest.UserId));
			var rtNew = CreateRefreshToken(tokenRequest.ClientId, tokenRequest.UserId);
			await _marketingService.AddToken(rtNew.ClientId, int.Parse(rtNew.UserId), rtNew.Value);

			var token = CreateAccessToken(tokenRequest, null, rtNew.Value);

			return Ok(token);
		}

        private async Task<TokenResponseModel> GetToken(TokenRequestModel tokenRequest, ApplicationUser appUser)
        {
            try
            {
				//           var now = DateTime.UtcNow;
				//           var claims = new Claim[]
				//           {
				//              new Claim(JwtRegisteredClaimNames.Sub, appUser.Id.ToString()),
				//              new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
				//              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
				//           };

				//           var expirationInMinutes = _configuration.GetValue<int>("Auth:Jwt:TokenExpirationInMinutes");
				//           var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Auth:Jwt:Key")));

				//           JwtSecurityToken jwtSecurityToken =
				//               new JwtSecurityToken(issuer: _configuration.GetValue<string>("Auth:Key:Issuer"),
				//               audience: _configuration.GetValue<string>("Auth:Key:Audience"),
				//               claims: claims,
				//               notBefore: now,
				//               expires: now.Add(TimeSpan.FromMinutes(expirationInMinutes)),
				//               signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

				//           var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

				//           return new TokenResponseModel
				//           {
				//               Token = token,
				//               Expiration = expirationInMinutes,
				//               Success = true,
				//Id = appUser.Id.ToString(),
				//Name = appUser.Name,
				//Email = appUser.Email
				//           };
				var refreshToken = CreateRefreshToken(tokenRequest.ClientId, appUser.Id.ToString());

	            await _marketingService.AddToken(refreshToken.ClientId, int.Parse(refreshToken.UserId),
		            refreshToken.Value);

				return CreateAccessToken(tokenRequest, appUser, refreshToken.Value);
            }
            catch(Exception ex)
            {
                return new TokenResponseModel { Success = false, Message = ex.Message };
            }
        }

		private Token CreateRefreshToken(string clientId, string userId)
		{
			return new Token
			{
				CreatedDate = DateTime.UtcNow,
				ClientId = clientId,
				UserId = userId,
				Value = Guid.NewGuid().ToString("N")
			};
		}

		private TokenResponseModel CreateAccessToken(TokenRequestModel tokenRequest, ApplicationUser appUser, string refreshToken)
		{
			var now = DateTime.UtcNow;
			var expirationInMinutes = _configuration.GetValue<int>("Auth:Jwt:TokenExpirationInMinutes");
			var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Auth:Jwt:Key")));
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Sub, appUser.Id.ToString())
			};

			var jwtToken = new JwtSecurityToken(issuer: _configuration.GetValue<string>("Auth:Jwt:Issuer"),
				audience: _configuration.GetValue<string>("Auth:Jwt:Audience"),
				claims: claims,
				notBefore: now,
				expires: now.Add(TimeSpan.FromMinutes(expirationInMinutes)),
				signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

			var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

			return new TokenResponseModel
			{
				Token = token,
				RefreshToken = refreshToken,
				Expiration = expirationInMinutes,
				Id = tokenRequest.UserId,
				Name = appUser.Name,
				Success = true,
				Email = appUser.Email
			};
		}
    }
}
