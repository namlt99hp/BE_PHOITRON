namespace BE_PHOITRON.Application.DTOs;

public record ThongKeFunctionDto(
    int ID,
    string Code,
    string Ten,
    string? MoTa,
    string DonVi,
    string? HighlightClass,
    bool IsAutoCalculated,
    bool IsActive
);

public record ThongKeFunctionUpsertDto(
    string Code,
    string Ten,
    string? MoTa,
    string DonVi,
    string? HighlightClass,
    bool IsAutoCalculated,
    bool IsActive = true,
    string? Nguoi_Tao = null
);

public record ThongKeFunctionUpsertWithIdDto(
    int? ID,
    string Code,
    string Ten,
    string? MoTa,
    string DonVi,
    string? HighlightClass,
    bool IsAutoCalculated,
    bool IsActive = true,
    string? Nguoi_Tao = null
);



public record PA_ThongKe_ResultDto(
    int ID_PhuongAn,
    int ID_ThongKe_Function,
    decimal GiaTri,
    DateTime Ngay_Tinh,
    string? Nguoi_Tinh,
    int? ThuTu
);

public record PlanResultsUpsertItemDto(
    int ID_ThongKe_Function,
    decimal? GiaTri,
    int? ThuTu
);

public record UpsertPlanResultsRequestDto(
    int PlanId,
    List<PlanResultsUpsertItemDto> Items
);

public record ThongKeResultDto(
    int ID_ThongKe_Function,
    string FunctionCode,
    string Ten,
    string? MoTa,
    string DonVi,
    decimal GiaTri,
    string? HighlightClass,
    int? ThuTu,
    bool IsAutoCalculated
);

public record CalculationContextDto(
    List<MixRowDataDto> MixData,
    List<GangCompositionDto> GangData,
    List<XaCompositionDto> XaData,
    List<ProcessParamDto>? ProcessParams
);

public record MixRowDataDto(
    string TenQuang,
    int LoaiQuang,
    decimal Ratio,
    decimal KlVaoLo,
    decimal TyLeHoiQuang,
    decimal KlNhan,
    decimal KlVaoLoResult,
    decimal KlNhanResult,
    Dictionary<int, decimal> Chems
);

public record GangCompositionDto(
    string Element,
    decimal Mass,
    decimal Percentage,
    bool? IsCalculated,
    string? CalcFormula,
    int? TphhId
);

public record XaCompositionDto(
    string Element,
    decimal Mass,
    decimal Percentage,
    bool? IsCalculated,
    string? CalcFormula,
    int? TphhId
);

public record ProcessParamDto(
    int ID,
    string Code,
    decimal Value,
    int? ID_Quang_LienKet,
    int? Scope,
    string? CalcFormula
);
