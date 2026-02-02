using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class AuthenService : IAuthenService
    {
        private readonly IAuthenRepository _authenRepo;
        private readonly IUnitOfWork _uow;

        public AuthenService(IAuthenRepository authenRepo, IUnitOfWork uow)
        {
            _authenRepo = authenRepo;
            _uow = uow;
        }

        public async Task<LoginResponse> LoginAsync(LoginDto loginDto, CancellationToken ct = default)
        {
            return await _authenRepo.LoginAsync(loginDto, ct);
        }
    }
}
