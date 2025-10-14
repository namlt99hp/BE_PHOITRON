using BE_PHOITRON.Application.DTOs;

namespace BE_PHOITRON.Application.ResponsesModels
{
    public record QuangResponse(
        int ID,
        string Ma_Quang,
        string Ten_Quang,
        int Loai_Quang,
        bool Dang_Hoat_Dong,
        bool Da_Xoa,
        string? Ghi_Chu,
        DateTimeOffset Ngay_Tao,
        int? Nguoi_Tao,
        DateTimeOffset? Ngay_Sua,
        int? Nguoi_Sua,
        decimal? Gia_USD_1Tan = null,
        decimal? Gia_VND_1Tan = null,
        decimal? Ty_Gia_USD_VND = null,
        DateTimeOffset? Ngay_Chon_TyGia = null,
        string? Tien_Te = null
    );

// Minimal models for batch chemistry API
public record QuangMinimal(int Id, string Ten_Quang);
public record TPHHOfQuangMinimal(int Id, decimal PhanTram, int? ThuTuTPHH = null);
public record OreChemistryBatchItem(QuangMinimal quang, IReadOnlyList<TPHHOfQuangMinimal> tP_HoaHocs);

    public record QuangDetailResponse(
        QuangResponse Quang,
        IReadOnlyList<TPHHOfQuangResponse> TP_HoaHocs,
        QuangGiaDto? GiaHienTai
    );

    public record TPHHOfQuangResponse(
        int ID,
        string Ma_TPHH,
        string? Ten_TPHH,
        decimal? PhanTram,
        int? ThuTuTPHH = null,
        string? CalcFormula = null,
        bool? IsCalculated = false
    );

    // CongThucPhoi Detail Response
    public record CongThucPhoiDetailResponse(
        int Id,
        string Ma_Cong_Thuc,
        string Ten_Cong_Thuc,
        string? Ghi_Chu,
        int ID_Phuong_An,
        int ID_Quang_Dau_Ra,
        decimal Tong_Ti_Le_Phoi,
        QuangMinimal QuangDauRa,
        IReadOnlyList<CTP_ChiTiet_QuangResponse> ChiTietQuang,
        IReadOnlyList<CTP_RangBuoc_TPHHResponse> RangBuocTPHH
    );

    // Summaries for plan → formulas listing
    public record PhuongAnWithFormulasResponse(
        int PlanId,
        string Ten_Phuong_An,
        DateTimeOffset? Ngay_Tinh_Toan,
        int? Milestone,
        IReadOnlyList<CongThucPhoiSummary> Formulas
    );

    // Optimized response with full details and Path-based sorting
    public record PhuongAnWithFormulasDetailsResponse(
        int PlanId,
        string Ten_Phuong_An,
        DateTimeOffset? Ngay_Tinh_Toan,
        IReadOnlyList<CongThucPhoiDetailMinimal> Formulas // Already sorted by Path/Level
    );

    // Response with all formulas in one list, sorted by ThuTuPhoi
    public record PhuongAnWithMilestonesResponse(
        int PlanId,
        string Ten_Phuong_An,
        DateTimeOffset? Ngay_Tinh_Toan,
        IReadOnlyList<CongThucPhoiDetailMinimal> Formulas, // Đã sắp xếp theo ThuTuPhoi, mỗi formula có milestone
        IReadOnlyList<QuangKetQuaInfo>? QuangKetQua = null // Gang và Xỉ kết quả của phương án
    );

    public record QuangKetQuaInfo(
        int ID_Quang,
        int LoaiQuang,
        string Ma_Quang,
        string Ten_Quang
    );


    public record CongThucPhoiSummary(
        int Id,
        string Ma_Cong_Thuc,
        string? Ten_Cong_Thuc,
        int ID_Quang_Dau_Ra,
        string? Ten_Quang_Dau_Ra,
        int? Milestone
    );

    // Minimal detail for dialog/editor
    public record CongThucPhoiDetailMinimal(
        CongThucInfo CongThuc,
        QuangChem QuangDauRa,
        IReadOnlyList<ChiTietQuangChem> ChiTietQuang,
        IReadOnlyList<RangBuocTPHHItem> RangBuocTPHH,
        int? Milestone
    );

    public record CongThucInfo(int Id, string Ma, string? Ten, string? GhiChu);

    public record QuangChem(
        int Id,
        string Ma_Quang,
        string Ten_Quang,
        IReadOnlyList<TPHHItem> TP_HoaHocs
    );

    public record ChiTietQuangChem(
        int ID_Quang,
        string Ten_Quang,
        decimal Ti_Le_Phan_Tram,
        IReadOnlyList<TPHHValue> TP_HoaHocs,
        int? Loai_Quang,
        decimal? Gia_USD_1Tan,
        decimal? Ty_Gia_USD_VND,
        decimal? Gia_VND_1Tan,
        // Milestone-specific fields from CTP_ChiTiet_Quang
        decimal? Khau_Hao,
        decimal? Ti_Le_KhaoHao,
        decimal? KL_VaoLo,
        decimal? Ti_Le_HoiQuang,
        decimal? KL_Nhan
    );

    public record TPHHItem(int Id, string Ma_TPHH, string? Ten_TPHH, decimal? PhanTram, int? ThuTuTPHH = null);
    public record TPHHValue(int Id, decimal? PhanTram, int? ThuTuTPHH = null);

    public record RangBuocTPHHItem(int ID_TPHH, string Ma_TPHH, string? Ten_TPHH, decimal? Min_PhanTram, decimal? Max_PhanTram);

    // Batch get formulas by output ore ids
    public record FormulaByOutputOreResponse(
        int OutputOreId,
        int CongThucPhoiId,
        string Ma_Cong_Thuc,
        string Ten_Cong_Thuc,
        DateTimeOffset Ngay_Tao,
        IReadOnlyList<FormulaItem> Items
    );

    public record FormulaItem(
        int Id,
        string Ma_Quang,
        string Ten_Quang,
        int Loai_Quang,
        decimal Gia_USD_1Tan,
        decimal Ty_Gia_USD_VND,
        decimal Gia_VND_1Tan,
        decimal Ti_Le_PhanTram
    );
}
