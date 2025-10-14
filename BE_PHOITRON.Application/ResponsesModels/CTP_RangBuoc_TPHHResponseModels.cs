namespace BE_PHOITRON.Application.ResponsesModels
{
    public record CTP_RangBuoc_TPHHResponse(
        int ID,
        int ID_Cong_Thuc_Phoi,
        int ID_TPHH,
        decimal? Min_PhanTram,
        decimal? Max_PhanTram,
        bool Rang_Buoc_Cung,
        byte? Uu_Tien,
        string? Ghi_Chu,
        // Navigation properties
        string? Cong_Thuc_Phoi_Ma,
        string? Cong_Thuc_Phoi_Ten,
        string? TP_HoaHoc_Ma,
        string? TP_HoaHoc_Ten
    );
}
