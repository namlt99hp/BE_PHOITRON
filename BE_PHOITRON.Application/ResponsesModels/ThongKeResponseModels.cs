namespace BE_PHOITRON.Application.ResponsesModels;

public record ThongKeFunctionResponse(
    int ID,
    string Code,
    string Ten,
    string? MoTa,
    string DonVi,
    string? HighlightClass,
    bool IsAutoCalculated,
    bool IsActive
);

public record ThongKeResultResponse(
    int ID_ThongKe_Function,
    string FunctionCode,
    string Ten,
    string DonVi,
    decimal GiaTri,
    string? HighlightClass,
    int? ThuTu,
    string? MoTa,
    bool IsAutoCalculated
);
