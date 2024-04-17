using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CityInfo.API.Controllers;

[ApiController]
[Route("/api/authentication")]
public class AuthenticationController : ControllerBase
{
  private readonly IConfiguration _configuration;

  public class AuthenticationRequestBody
  {
    public string? UserName { get; set; }
    public string? Password { get; set; }
  }

  public class CityInfoUser(int userId, string userName, string firstName, string lastName, string city)
  {
    public int UserId { get; set; } = userId;
    public string UserName { get; set; } = userName;
    public string FirstName { get; set; } = firstName;
    public string LastName { get; set; } = lastName;
    public string City { get; set; } = city;
  }

  public AuthenticationController(IConfiguration configuration)
  {
    _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
  }

  [HttpPost("authenticate")]
  public ActionResult<string> Authenticate(
    AuthenticationRequestBody authenticationRequestBody)
  {
    // Step 1: validate the username/password
    CityInfoUser? user = ValidateUserCredentials(
      authenticationRequestBody.UserName,
      authenticationRequestBody.Password);

    if (user == null)
    {
      return Unauthorized();
    }

    // Step 2: create a token
    var securityKey = new SymmetricSecurityKey(
      Convert.FromBase64String(_configuration["Authentication:SecretForKey"]));
    var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    // List of claims
    List<Claim> claimsForToken = [
      new Claim("sub", user.UserId.ToString()),
      new Claim("given_name", user.FirstName.ToString()),
      new Claim("family_name", user.LastName.ToString()),
      new Claim("city", user.City.ToString()),
    ];

    JwtSecurityToken jwtSecurityToken = new(
      _configuration["Authentication:Issuer"],
      _configuration["Authentication:Audience"],
      claimsForToken,
      DateTime.UtcNow,
      DateTime.UtcNow.AddHours(1),
      signingCredentials);

    string tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

    return Ok(tokenToReturn);
  }

  private CityInfoUser ValidateUserCredentials(string? userName, string? password)
  {
    // we don;t have a user DB or table. If you have, check the passed-through
    // username/password agains what's stored in the database.
    //
    // For demo purposes, we assume the credentials are valid

    // return a new CityInfoUser (values would normally come from your user DB/table)

    return new CityInfoUser(
      1,
      userName ?? "",
      "Kevin",
      "Dockx",
      "Antwerp");
  }
}
