using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class PlanningService : IPlanningService
    {
        private readonly IPhuong_An_PhoiRepository _planRepo;
        private readonly IPA_LuaChon_CongThucRepository _mappingRepo;
        private readonly ICong_Thuc_PhoiRepository _recipeRepo;
        private readonly IQuangRepository _quangRepo;
        private readonly IQuang_TP_PhanTichRepository _tpRepo;
        private readonly IQuang_Gia_LichSuRepository _giaRepo;
        private readonly IUnitOfWork _uow;

        public PlanningService(
            IPhuong_An_PhoiRepository planRepo,
            IPA_LuaChon_CongThucRepository mappingRepo,
            ICong_Thuc_PhoiRepository recipeRepo,
            IQuangRepository quangRepo,
            IQuang_TP_PhanTichRepository tpRepo,
            IQuang_Gia_LichSuRepository giaRepo,
            IUnitOfWork uow)
        {
            _planRepo = planRepo;
            _mappingRepo = mappingRepo;
            _recipeRepo = recipeRepo;
            _quangRepo = quangRepo;
            _tpRepo = tpRepo;
            _giaRepo = giaRepo;
            _uow = uow;
        }

        public Task<PlanValidationResult> ValidatePlanAsync(ValidatePlanRequest request, CancellationToken ct = default)
        {
            var issues = new List<PlanValidationIssue>();
            // Placeholder: real validation (overlap periods, sum 100%, yield > 0, no cycles)
            var ok = true;
            return Task.FromResult(new PlanValidationResult(ok, issues));
        }

        public Task<ComputePlanResult> ComputePlanAsync(ComputePlanRequest request, CancellationToken ct = default)
        {
            // Placeholder minimal shape. Real logic to expand tree, compute TPHH and costs.
            var ngay = request.Ngay_Tinh_Toan ?? DateTimeOffset.Now;
            var tphh = new Dictionary<string, decimal>();
            var leaves = new List<LeafComposition>();
            return Task.FromResult(new ComputePlanResult(request.PlanId, ngay, tphh, 0m, leaves));
        }

        public async Task<ComparePlansResult> ComparePlansAsync(ComparePlansRequest request, CancellationToken ct = default)
        {
            var results = new List<ComputePlanResult>();
            foreach (var id in request.PlanIds)
            {
                var comp = await ComputePlanAsync(new ComputePlanRequest(id, request.OutputMass_Ton, request.Ngay_Tinh_Toan), ct);
                results.Add(comp);
            }
            return new ComparePlansResult(results);
        }
    }
}


