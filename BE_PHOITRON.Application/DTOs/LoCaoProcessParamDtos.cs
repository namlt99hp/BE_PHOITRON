namespace BE_PHOITRON.Application.DTOs
{
    public record UpsertProcessParamValuesDto(
        int PaLuaChonCongThucId,
        IReadOnlyList<UpsertProcessParamValueItem> Items
    );

    public record UpsertProcessParamValueItem(
        int IdProcessParam,
        decimal GiaTri,
        int? ThuTuParam
    );
}


