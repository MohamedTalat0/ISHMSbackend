using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.DTOs.Auth;
using Core.Interfaces;
using Core.Settings;
using ISHMS.Core.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ISHMS.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly ILdapAuthenticationService _ldapService;
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthService(
            IAuthRepository authRepository,
            IOptions<JwtSettings> jwtSettings,
            ILdapAuthenticationService ldapService,
            UserManager<ApplicationUser> userManager)
        {
            _authRepository = authRepository;
            _jwtSettings = jwtSettings.Value;
            _ldapService = ldapService;
            _userManager = userManager;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var result = await _authRepository.RegisterAsync(dto);

            if (!result.IsAuthenticated)
            {
                return result;
            }

            var token = GenerateJwtToken(
                result.Id!,
                result.Email!,
                result.FullName!,
                result.Roles);

            result.Token = token;
            result.TokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.DurationInDays);

            return result;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var isAuthenticated =
                _ldapService.Authenticate(
                    dto.Email,
                    dto.Password);

            if (!isAuthenticated)
            {
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Message = "Invalid username or password"
                };
            }

            var adUser =
                _ldapService.GetUserInfo(dto.Email);

            if (adUser == null)
            {
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Message = "User not found in Active Directory"
                };
            }

            var dbUser =
                await _userManager.FindByNameAsync(
                    adUser.Username);

            if (dbUser == null)
            {
                dbUser = new ApplicationUser
                {
                    UserName = adUser.Username,
                    FullName = adUser.FullName ?? adUser.Username,
                    Email = adUser.Email ??
                            $"{adUser.Username}@ishms.local"
                };

                var createResult =
                    await _userManager.CreateAsync(dbUser);

                if (!createResult.Succeeded)
                {
                    return new AuthResponseDto
                    {
                        IsAuthenticated = false,
                        Message = string.Join(
                            ", ",
                            createResult.Errors.Select(e => e.Description))
                    };
                }
            }

            var token = GenerateJwtToken(
                dbUser.Id,
                dbUser.Email!,
                dbUser.FullName,
                adUser.Roles);

            return new AuthResponseDto
            {
                Id = dbUser.Id,
                IsAuthenticated = true,
                FullName = dbUser.FullName,
                Email = dbUser.Email,
                Roles = adUser.Roles,
                Token = token,
                TokenExpiration =
                    DateTime.UtcNow.AddDays(
                        _jwtSettings.DurationInDays)
            };
        }
        private string GenerateJwtToken(
            string userId,
            string email,
            string fullName,
            IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var symmetricKey = new SymmetricSecurityKey(keyBytes);

            var signingCredentials = new SigningCredentials(
                symmetricKey,
                SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwtSettings.DurationInDays),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}