using System.Collections.Generic;

namespace BE_PHOITRON.Application.ResponsesModels
{
    public record ProcessParamTemplateItem(
        int Id,
        string Code,
        string Ten,
        string? DonVi,
        int ThuTu
    );

    public record ThongKeTemplateItem(
        int Id,
        string Code,
        string Ten,
        string? DonVi,
        int ThuTu
    );

    public record GangTemplateConfigResponse(
        QuangResponse Gang,
        IReadOnlyList<TPHHOfQuangResponse> GangTPHHs,
        QuangResponse? Slag,
        IReadOnlyList<TPHHOfQuangResponse> SlagTPHHs,
        IReadOnlyList<ProcessParamTemplateItem> ProcessParams,
        IReadOnlyList<ThongKeTemplateItem> ThongKes
    );

    public record GangDichConfigDetailResponse(
        QuangDetailResponse Gang,
        QuangDetailResponse? Slag,
        IReadOnlyList<ProcessParamTemplateItem> ProcessParams,
        IReadOnlyList<ThongKeTemplateItem> ThongKes
    );
}

