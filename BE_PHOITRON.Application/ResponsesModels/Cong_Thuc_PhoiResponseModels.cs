namespace BE_PHOITRON.Application.ResponsesModels
{
    public record Cong_Thuc_PhoiResponse(
        int ID,
        int ID_Quang_DauRa,
        string Ma_Cong_Thuc,
        string? Ten_Cong_Thuc,
        decimal He_So_Thu_Hoi,
        decimal Chi_Phi_Cong_Doạn_1Tan,
        int Phien_Ban,
        byte Trang_Thai,
        DateTimeOffset Hieu_Luc_Tu,
        DateTimeOffset? Hieu_Luc_Den,
        string? Ghi_Chu,
        DateTimeOffset Ngay_Tao,
        int? Nguoi_Tao,
        DateTimeOffset? Ngay_Sua,
        int? Nguoi_Sua,
        // Navigation properties
        string? Quang_DauRa_Ma,
        string? Quang_DauRa_Ten,
        // Calculated properties
        decimal? Tong_Ti_Le_Phan_Tram,
        int? So_Luong_Quang_DauVao
    );
}
