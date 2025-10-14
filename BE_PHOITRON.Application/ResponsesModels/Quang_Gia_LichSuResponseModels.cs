namespace BE_PHOITRON.Application.ResponsesModels
{
    public record Quang_Gia_LichSuResponse(
        int ID,
        int ID_Quang,
        decimal Don_Gia_USD_1Tan,
        decimal Don_Gia_VND_1Tan,
        decimal Ty_Gia_USD_VND,
        string Tien_Te,
        DateTimeOffset Hieu_Luc_Tu,
        DateTimeOffset? Hieu_Luc_Den,
        string? Ghi_Chu,
        // Navigation properties
        string? Quang_Ma,
        string? Quang_Ten
    );
}
