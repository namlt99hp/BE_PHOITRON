namespace BE_PHOITRON.Application.ResponsesModels
{
    public record Quang_TP_PhanTichResponse(
        int ID,
        int ID_Quang,
        int ID_TPHH,
        decimal Gia_Tri_PhanTram,
        DateTimeOffset Hieu_Luc_Tu,
        DateTimeOffset? Hieu_Luc_Den,
        string? Nguon_Du_Lieu,
        string? Ghi_Chu,
        int? ThuTuTPHH,
        bool Da_Xoa,
        decimal? KhoiLuong,
        // Formula calculation fields
        string? CalcFormula,
        bool? IsCalculated,
        // Navigation properties
        string? Quang_Ma,
        string? Quang_Ten,
        string? TP_HoaHoc_Ma,
        string? TP_HoaHoc_Ten
    );
}
