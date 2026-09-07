// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Microsoft.Extensions.Configuration;
// using Microsoft.IdentityModel.Tokens;
// using TmsApi.Infrastructure.Identity;

// namespace TmsApi.Infrastructure.Services;

// public class TokenService
// {
//     private readonly IConfiguration _config;

//     public TokenService(IConfiguration config)
//     {
//         _config = config;
//     }

//     public string GenerateJwt(TmsUser user, IList<string> roles)
//     {
//         var claims = new List<Claim>
//         {
//             new Claim(ClaimTypes.NameIdentifier, user.Id),
//             new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
//             new Claim("FirstName", user.FirstName)
//         };

//         foreach (var role in roles)
//         {
//             claims.Add(new Claim(ClaimTypes.Role, role));
//         }

//         var key = new SymmetricSecurityKey(
//             Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
//         );

//         var creds = new SigningCredentials(
//             key,
//             SecurityAlgorithms.HmacSha256
//         );

//         var token = new JwtSecurityToken(
//             issuer: _config["Jwt:Issuer"],
//             audience: _config["Jwt:Audience"],
//             claims: claims,
//             expires: DateTime.UtcNow.AddMinutes(
//                 int.Parse(_config["Jwt:ExpiryMinutes"]!)
//             ),
//             signingCredentials: creds
//         );

//         return new JwtSecurityTokenHandler().WriteToken(token);
//     }
// }


using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using TmsApi.Infrastructure.Identity;

namespace TmsApi.Infrastructure.Services;

/// <summary>
/// Service responsible for generating JWT access tokens.
/// </summary>
public class TokenService
{
    private readonly IConfiguration _config;

    /// <summary>
    /// Constructor.
    /// IConfiguration gives us access to appsettings.json.
    /// </summary>
    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates a JWT access token for the specified user.
    /// </summary>
    /// <param name="user">
    /// The authenticated TMS user.
    /// </param>
    /// <param name="roles">
    /// Roles assigned to the user.
    /// </param>
    /// <returns>
    /// A signed JWT string.
    /// </returns>
    public string GenerateJwt(
        TmsUser user,
        IList<string> roles)
    {
        // ============================================================
        // 1. READ JWT CONFIGURATION
        // ============================================================

        var jwtKey = _config["Jwt:Key"];

        var jwtIssuer = _config["Jwt:Issuer"];

        var jwtAudience = _config["Jwt:Audience"];

        var expiryMinutesText =
            _config["Jwt:ExpiryMinutes"];


        // ============================================================
        // 2. VALIDATE JWT CONFIGURATION
        // ============================================================

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException(
                "Jwt:Key is missing from appsettings.json.");
        }

        if (string.IsNullOrWhiteSpace(jwtIssuer))
        {
            throw new InvalidOperationException(
                "Jwt:Issuer is missing from appsettings.json.");
        }

        if (string.IsNullOrWhiteSpace(jwtAudience))
        {
            throw new InvalidOperationException(
                "Jwt:Audience is missing from appsettings.json.");
        }

        if (!int.TryParse(
                expiryMinutesText,
                out var expiryMinutes))
        {
            throw new InvalidOperationException(
                "Jwt:ExpiryMinutes is missing or invalid.");
        }


        // ============================================================
        // 3. VALIDATE USER
        // ============================================================

        if (user == null)
        {
            throw new ArgumentNullException(
                nameof(user));
        }

        if (string.IsNullOrWhiteSpace(user.Id))
        {
            throw new InvalidOperationException(
                "User Id is missing.");
        }


        // ============================================================
        // 4. CREATE CLAIMS
        // ============================================================

        var claims = new List<Claim>
        {
            // User's unique Identity ID
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id),

            // User's email
            new Claim(
                ClaimTypes.Email,
                user.Email ?? string.Empty),

            // User's first name
            new Claim(
                "FirstName",
                user.FirstName ?? string.Empty)
        };


        // ============================================================
        // 5. ADD USER ROLES
        // ============================================================

        foreach (var role in roles)
        {
            // Add every role as a Role claim.
            //
            // Example:
            // Admin
            // Instructor
            // Student

            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }


        // ============================================================
        // 6. CREATE SECURITY KEY
        // ============================================================

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));


        // ============================================================
        // 7. CREATE SIGNING CREDENTIALS
        // ============================================================

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);


        // ============================================================
        // 8. CREATE JWT TOKEN
        // ============================================================

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,

            audience: jwtAudience,

            claims: claims,

            expires:
                DateTime.UtcNow.AddMinutes(
                    expiryMinutes),

            signingCredentials: credentials);


        // ============================================================
        // 9. CONVERT TOKEN TO STRING
        // ============================================================

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}