namespace AppCore.Dto.Auth;

public record RefreshTokenDto(
    string AccessToken,
    string RefreshToken
);