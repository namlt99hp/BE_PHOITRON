namespace BE_PHOITRON.Application.ResponsesModels
{
    public record CTP_ChiTiet_QuangResponse(
        int ID,
        int ID_Cong_Thuc_Phoi,
        int ID_Quang_DauVao,
        decimal Ti_Le_Phan_Tram,
        decimal? He_So_Hao_Hut_DauVao,
        int? Thu_Tu,
        string? Ghi_Chu,
        // Navigation properties
        string? Cong_Thuc_Phoi_Ma,
        string? Cong_Thuc_Phoi_Ten,
        string? Quang_DauVao_Ma,
        string? Quang_DauVao_Ten
    );
}
