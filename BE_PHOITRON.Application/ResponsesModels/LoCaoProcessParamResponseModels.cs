namespace BE_PHOITRON.Application.ResponsesModels
{
    public record ProcessParamConfiguredResponse(
        int Id,
        string Code,
        string Ten,
        string DonVi,
        int? ID_Quang_LienKet,
        int? Scope,
        int ThuTu,
        bool? IsCalculated,
        string? CalcFormula,
        decimal? GiaTri,
        int? ThuTuParam
    );
}


