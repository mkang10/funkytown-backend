using System.Threading.Tasks;
using Domain.DTO.Request;
using Domain.DTO.Response;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> AuthenticateAsync(string username, string password);
        Task<TokenResponse> RegisterAsync(RegisterReq registerDTO);

        Task<LoginResponse> AuthenticateWithGoogleAsync(string idToken);

    }
}
