using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record ValidatePlanRequest(
        [Required] int PlanId
    );

    public record ComputePlanRequest(
        [Required] int PlanId,
        [Required] decimal OutputMass_Ton,
        DateTimeOffset? Ngay_Tinh_Toan = null
    );

    public record ComparePlansRequest(
        [Required] IReadOnlyList<int> PlanIds,
        [Required] decimal OutputMass_Ton,
        DateTimeOffset? Ngay_Tinh_Toan = null
    );
}


