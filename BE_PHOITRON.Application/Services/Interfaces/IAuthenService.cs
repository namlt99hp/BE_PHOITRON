using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface IAuthenService
    {
        Task<LoginResponse> LoginAsync(LoginDto loginDto, CancellationToken ct = default);
    }
}
