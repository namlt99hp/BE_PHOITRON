using BE_PHOITRON.Application.ResponsesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.ResponsesModels
{
    public record CongThucPhoiResponse(
        int ID,
        string MaCongThuc,
        string TenCongThuc,
        decimal? TongPhanTram,
        string? GhiChu,
        DateTime? NgayTao,
        int? ID_NguoiTao,
        DateTime? NgaySua,
        int? ID_NguoiSua,
        bool IsDeleted,
        int? ID_QuangNeo
    );

    public record TPHHOfCongThucResponse(
        int ID_TPHH,
        string Ma_TPHH,
        string? Ten_TPHH,
        decimal? Min_PhanTram,
        decimal? Max_PhanTram
    );
    public record CongThucQuangResponse(
        int ID,
        string MaQuang,
        string? TenQuang,
        List<TPHHOfQuangReponse> TP_HoaHocs
    );
    public record CongThucPhoiDetailRespone(
        CongThucPhoiResponse CongThuc,
        List<TPHHOfCongThucResponse> TPHHs,
        List<CongThucQuangResponse> Quangs
    );
    
    public sealed record UpsertResult(int FormulaId);

    
    public sealed record ConfirmOreResult(int OreId);

    // ===== VM phục vụ FE edit / so sánh =====
    public sealed record FormulaInputVm(
        int ID_Quang, string MaQuang, string TenQuang, decimal? Gia, decimal TiLePhoi);

    public sealed record CongThucEditVm(
        int ID, int? ID_QuangNeo, string MaCongThuc, string TenCongThuc, string? GhiChu,
        List<FormulaInputVm> Inputs
    );

    public sealed record FormulaSummaryVm(
        int ID, string MaCongThuc, string TenCongThuc, int ProducedCount, DateTimeOffset? LastProducedAt);

    public sealed record NeoDashboardVm(
        int ID_QuangNeo, string? MaQuangNeo, string? TenQuangNeo, List<FormulaSummaryVm> Formulas);

}
