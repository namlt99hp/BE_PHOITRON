using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Domain.Entities;
using System.Linq.Expressions;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface IAuthenRepository
    {
        Task<LoginResponse?> LoginAsync(LoginDto loginDto, CancellationToken ct = default);
    }
}
