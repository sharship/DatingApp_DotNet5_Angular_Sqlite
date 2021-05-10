using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key; // for JWT key
        private readonly UserManager<AppUser> _userManager;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        public async Task<string> CreateTokenAsync(AppUser user)
        {

            #region Build token descriptor: based on claims and credentials
            var claims = new List<Claim>
            {
                // payload incudes:
                // nameId:
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                // userName:
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),

            };

            // get current user's roles, and add it to token claims
            var roles = await _userManager.GetRolesAsync(user);

            claims.AddRange(
                roles.Select(role => new Claim(ClaimTypes.Role, role))
            );


            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };
            #endregion

            #region Create a token: based on token handler and decriptor
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);
            #endregion


            return tokenHandler.WriteToken(token);
        }
    }
}