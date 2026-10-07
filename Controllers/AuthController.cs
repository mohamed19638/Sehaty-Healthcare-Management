using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(AppDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(x => x.Email == dto.Email))
            return Conflict(new { message = "البريد الإلكتروني مستخدم بالفعل." });
        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Phone = dto.Phone,
            Height = dto.Height,
            Weight = dto.Weight,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, dto.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return CreatedAtAction(
            nameof(Me),
            new { id = user.Id },
            new
            {
                user.Id,
                user.FullName,
                user.Email,
            }
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == dto.Email);
        if (
            user is null
            || new PasswordHasher<User>().VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password
            ) == PasswordVerificationResult.Failed
        )
            return Unauthorized(new { message = "البريد الإلكتروني أو كلمة المرور غير صحيحة." });
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
        };
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
        );
        var jwt = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        );
        return Ok(
            new
            {
                token = new JwtSecurityTokenHandler().WriteToken(jwt),
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                },
            }
        );
    }

    [Authorize, HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await db.Users.FindAsync(CurrentUser.Id(User));
        return user is null
            ? NotFound()
            : Ok(
                new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.DateOfBirth,
                    user.Gender,
                    user.Phone,
                    user.Height,
                    user.Weight,
                }
            );
    }
}

