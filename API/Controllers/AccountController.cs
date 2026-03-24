using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers;

public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDto)
    {
            // check if the email exists already
            if(await EmailExists(registerDto.Email)) return BadRequest("Email taken");

            // create instance of hasher
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            // add and save user to db
            context.Users.Add(user);
            await context.SaveChangesAsync();

            
        return user.ToDto(tokenService);

    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDTO loginDto)
    {
        var user = await context.Users.SingleOrDefaultAsync(user => user.Email == loginDto.Email);
        
        if (user == null) return Unauthorized("Invalid email address");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash =  hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (var i =0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password!");
        }
        
        return user.ToDto(tokenService);
    }

    private async Task<bool> EmailExists(string email)
    {
        return await context.Users.AnyAsync(user => user.Email.ToLower() == email.ToLower());
    }
}
