using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace OtpSystem.Application.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    //public string GenerateJwtToken(User user)
    //{
    //    var claims = new List<Claim>
    //    {
    //        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    //        new(ClaimTypes.Name, user.UserName.ToString()),
    //        new(ClaimTypes.Role, user.Role.Name.ToString())
    //    };

    //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
    //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //    var token = new JwtSecurityToken(
    //        issuer: _config["Jwt:Issuer"],
    //        audience: _config["Jwt:Audience"],
    //        claims: claims,
    //        expires: DateTime.UtcNow.AddDays(30),
    //        signingCredentials: creds
    //    );

    //    return new JwtSecurityTokenHandler().WriteToken(token);
    //}

    //public async Task<ClaimsPrincipal> GenerateCookieToken(User user)
    //{
    //    var claims = new List<Claim>
    //    {
    //        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    //        new(ClaimTypes.Name, user.UserName.ToString()),
    //        new(ClaimTypes.Role, user.Role.Name.ToString())
    //    };

    //    var identity = new ClaimsIdentity(claims, "Cookies");
    //    var principal = new ClaimsPrincipal(identity);

    //    return principal;
    //}
}
