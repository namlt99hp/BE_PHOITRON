using System.ComponentModel.DataAnnotations;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.DTOs
{
    public record Phuong_An_PhoiCreateDto(
        [Required] string Ten_Phuong_An,
        [Required] int ID_Quang_Dich,
        [Required] DateTimeOffset Ngay_Tinh_Toan,
        int Phien_Ban = 1,
        [Range(0, 2)] byte Trang_Thai = 0,
        byte? Muc_Tieu = null,
        string? Ghi_Chu = null
    );

    public record Phuong_An_PhoiUpdateDto(
        [Required] int ID,
        [Required] string Ten_Phuong_An,
        [Required] int ID_Quang_Dich,
        [Required] DateTimeOffset Ngay_Tinh_Toan,
        int Phien_Ban = 1,
        [Range(0, 2)] byte Trang_Thai = 0,
        byte? Muc_Tieu = null,
        string? Ghi_Chu = null
    );

    public record Phuong_An_PhoiUpsertDto(
        int? ID,
        Phuong_An_PhoiCreateDto Phuong_An_Phoi
    );

    // === Mix Quặng (Phối) DTOs ===
    public record MixQuangRequestDto(
        CongThucPhoiDto CongThucPhoi,
        IReadOnlyList<CTP_ChiTiet_QuangDto> ChiTietQuang,
        IReadOnlyList<CTP_RangBuoc_TPHHDto> RangBuocTPHH,
        QuangThanhPhamDto QuangThanhPham,
        int? Milestone = null
    );

    public record CongThucPhoiDto(
        int? ID,
        int ID_Phuong_An,
        string? Ma_Cong_Thuc,
        string? Ten_Cong_Thuc,
        string? Ghi_Chu,
        DateTimeOffset? Ngay_Tao
    );

    public record CTP_ChiTiet_QuangDto(
        int ID_Quang,
        decimal Ti_Le_PhanTram,
        // Milestone-specific optional fields
        decimal? Khau_Hao = null,
        decimal? Ti_Le_KhaoHao = null,
        decimal? KL_VaoLo = null,
        decimal? Ti_Le_HoiQuang = null,
        decimal? KL_Nhan = null,
        IReadOnlyList<TPHHValue>? TP_HoaHocs = null // Thành phần hóa học đã chỉnh sửa
    );

    public record CTP_RangBuoc_TPHHDto(
        int ID_TPHH,
        decimal? Min_PhanTram,
        decimal? Max_PhanTram
    );

    public record QuangThanhPhamDto(
        string Ma_Quang,
        string Ten_Quang,
        int Loai_Quang,
        IReadOnlyList<QuangTPPhanTichDto> ThanhPhanHoaHoc,
        QuangGiaDto? Gia = null // Giá của quặng đầu ra
    );

    public record QuangTPPhanTichDto(
        int ID_TPHH,
        decimal Gia_Tri_PhanTram,
        int? ThuTuTPHH = null
    );


    // ==== Clone Plan / Clone Milestones DTOs ====
    public record ClonePlanRequestDto(
        int SourcePlanId,
        string NewPlanName,
        bool ResetRatiosToZero = false,
        bool CopySnapshots = false,
        bool CopyDates = false,
        bool CopyStatuses = false,
        DateTimeOffset? CloneDate = null
    );

    public record CloneMilestonesRequestDto(
        int SourcePlanId,
        int? TargetPlanId,
        IReadOnlyList<CloneMilestoneItem> CloneItems,
        bool ResetRatiosToZero = false,
        bool CopySnapshots = false,
        bool CopyDates = false,
        DateTimeOffset? CloneDate = null
    );

    public record CloneMilestoneItem(
        int? Milestone,
        IReadOnlyList<int>? FormulaIds
    );
}
