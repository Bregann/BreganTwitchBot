using BreganTwitchBot.Domain.DTOs.Auth.Requests;
using BreganTwitchBot.Domain.DTOs.Auth.Responses;

namespace BreganTwitchBot.Domain.Interfaces.Api
{
    public interface IAuthService
    {
        Task RegisterUser(RegisterUserRequest request);
        Task<LoginUserResponse> LoginUser(LoginUserRequest request);
        Task<LoginUserResponse> RefreshToken(string refreshToken);
    }
}
