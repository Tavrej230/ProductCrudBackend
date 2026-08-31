using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProductCrud.Application.Dtos;
using ProductCrud.Application.Interface;
using ProductCrud.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProductCrud.Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository,IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }
        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                return false;
            }

            var user = new User
            {
                FullName = dto.FullName,

                Email = dto.Email,
               
                MobileNumber = dto.MobileNumber,

                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _userRepository.AddAsync(user);

            return true;
        }
        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null)
            {
                return null;
            }

            var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);

            if (!passwordValid)
            {
                return null;
            }

            var token = GenerateJwtToken(user.Id, user.FullName, user.Email);

            return new LoginResponseDto
            {
                Message = "Login successful",

                FullName = user.FullName,

                Email = user.Email,

                Token = token
            };
        }
        private string GenerateJwtToken(int userId, string fullName, string email)
        {
            var claims = new[]
            {
             new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
             new Claim(ClaimTypes.Name, fullName),
             new Claim(ClaimTypes.Email, email),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
           };

            var key = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("JWT Key is not configured.");
            }

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            );

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
