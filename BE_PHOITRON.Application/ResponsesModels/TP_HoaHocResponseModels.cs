namespace BE_PHOITRON.Application.ResponsesModels
{
    public record TP_HoaHocResponse(
        int ID,
        string Ma_TPHH,
        string? Ten_TPHH,
        string Don_Vi,
        int? Thu_Tu,
        string? Ghi_Chu,
        DateTimeOffset Ngay_Tao,
        bool Da_Xoa
    );
}
