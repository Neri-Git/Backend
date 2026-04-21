namespace Infrastructure.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AppCore.Dto;
using AppCore.Dto.Auth;
using AppCore.Interfaces;
using AppCore.Models;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class AuthService : IAuthService
{
    private readonly UserManager<CrmUser> _userManager;
    private readonly ContactsDbContext _context;
    private readonly JwtSettings _jwtOptions;

    public AuthService(
        UserManager<CrmUser> userManager,
        ContactsDbContext context,
        JwtSettings jwtOptions)
    {
        _userManager = userManager;
        _context = context;
        _jwtOptions = jwtOptions;
    }

    // ── Logowanie ─────────────────────────────────────────

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // 1. Szukamy użytkownika po emailu
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new Exception("Nieprawidłowy email lub hasło.");

        // 2. Sprawdzamy hasło
        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            // Zwiększamy licznik nieudanych prób logowania
            await _userManager.AccessFailedAsync(user);
            throw new Exception("Nieprawidłowy email lub hasło.");
        }

        // 3. Konto musi być aktywne
        if (user.Status != SystemUserStatus.Active)
            throw new Exception("Konto jest nieaktywne.");

        // 4. Konto nie może być zablokowane
        if (await _userManager.IsLockedOutAsync(user))
            throw new Exception("Konto jest zablokowane.");

        // 5. Po poprawnym logowaniu zerujemy licznik błędnych prób
        await _userManager.ResetAccessFailedCountAsync(user);

        // 6. Tu można zapisać np. czas ostatniego logowania
        await _userManager.UpdateAsync(user);

        // 7. Generujemy access token + refresh token + dane użytkownika
        return await GenerateAuthResponseAsync(user);
    }

    // ── Odświeżenie tokenu ────────────────────────────────

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        // 1. Odczytujemy claims z wygasłego access tokenu
        var principal = GetPrincipalFromExpiredToken(dto.AccessToken);

        // 2. Pobieramy ID użytkownika z tokenu
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("Nieprawidłowy token.");

        // 3. Sprawdzamy czy użytkownik istnieje
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("Użytkownik nie istnieje.");

        // 4. Szukamy refresh tokenu w bazie
        var refreshToken = await _context.RefresTokens
            .FirstOrDefaultAsync(t =>
                t.Token == dto.RefreshToken &&
                t.UserId == userId)
            ?? throw new Exception("Nieprawidłowy refresh token.");

        // 5. Token musi być aktywny
        if (!refreshToken.IsActive)
            throw new Exception("Refresh token wygasł lub został odwołany.");

        // 6. Generujemy nową parę tokenów
        var newResponse = await GenerateAuthResponseAsync(user);

        // 7. Stary refresh token zostaje unieważniony i wskazuje nowy token
        refreshToken.Revoke(newResponse.RefreshToken);

        await _context.SaveChangesAsync();

        return newResponse;
    }

    // ── Odwołanie tokenu (wylogowanie) ────────────────────

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await _context.RefresTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken)
            ?? throw new Exception($"Refresh token nie istnieje: {refreshToken}");

        if (!token.IsActive)
            throw new Exception("Token jest już nieaktywny.");

        token.Revoke();
        await _context.SaveChangesAsync();
    }

    // ── Metody pomocnicze ─────────────────────────────────

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(CrmUser user)
    {
        // Pobieramy role użytkownika
        var roles = await _userManager.GetRolesAsync(user);

        // Tworzymy access token
        var accessToken = GenerateAccessToken(user, roles);

        // Tworzymy refresh token i zapisujemy go w bazie
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        // Zwracamy komplet danych do klienta
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes),
            User = new UserDto
            {
                Id = Guid.Parse(user.Id),
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName
            }
        };
    }

    // ── Generowanie access tokenu ─────────────────────────

    private string GenerateAccessToken(CrmUser user, IList<string> roles)
    {
        // Claims to dane zakodowane w JWT
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new("department", user.Department ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        // Dodajemy role jako claims
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        // Podpis tokenu symetrycznym kluczem
        var credentials = new SigningCredentials(
            _jwtOptions.GetSymmetricKey(),
            SecurityAlgorithms.HmacSha256);

        // Tworzymy token JWT
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── Generowanie refresh tokenu ────────────────────────

    private async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        // Unieważniamy wszystkie aktywne refresh tokeny tego użytkownika
        var activeTokens = await _context.RefresTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in activeTokens)
            token.Revoke();

        // Tworzymy nowy refresh token
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        };

        await _context.RefresTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    // ── Odczyt claims z wygasłego access tokenu ────────────

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // pozwalamy odczytać wygasły token
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = _jwtOptions.GetSymmetricKey()
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(accessToken, parameters, out var securityToken);

        // Sprawdzamy, czy podpis był wykonany właściwym algorytmem
        if (securityToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Nieprawidłowy token.");
        }

        return principal;
    }
}