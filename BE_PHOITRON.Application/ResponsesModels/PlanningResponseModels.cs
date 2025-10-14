namespace BE_PHOITRON.Application.ResponsesModels
{
    public record PlanValidationIssue(
        string Code,
        string Message,
        string? NodePath = null
    );

    public record PlanValidationResult(
        bool IsValid,
        IReadOnlyList<PlanValidationIssue> Issues
    );

    public record LeafComposition(
        int QuangId,
        string Ma_Quang,
        string Ten_Quang,
        decimal OutputMass_Ton
    );

    public record ComputePlanResult(
        int PlanId,
        DateTimeOffset Ngay_Tinh_Toan,
        IReadOnlyDictionary<string, decimal> TPHH_OutputPercent,
        decimal Tong_Chi_Phi_1Tan,
        IReadOnlyList<LeafComposition> LeafBreakdown
    );

    public record ComparePlansResult(
        IReadOnlyList<ComputePlanResult> Plans,
        string? RankedBy = null
    );
}


