using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(AppUser user)
    {
        // get key to sign the token
        var tokenKey = config["TokenKey"] ?? throw new Exception("Cannot get token key");

        if(tokenKey.Length < 64)
        {
            throw new Exception("Your token key needs to be >= 64 characters");
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        // claims to claim who is the owner of this token
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        // create the credentials for signing the token with
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };

        // create the token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // return the token back to the user
        return tokenHandler.WriteToken(token);
    }
}
