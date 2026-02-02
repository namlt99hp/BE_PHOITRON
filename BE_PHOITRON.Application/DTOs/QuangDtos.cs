using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record QuangCreateDto(
        [Required] [StringLength(50)] string Ma_Quang,
        [Required] [StringLength(200)] string Ten_Quang,
        [Required] int Loai_Quang,
        bool Dang_Hoat_Dong = true,
        string? Ghi_Chu = null,
        int? Nguoi_Tao = null
    );

    public record QuangUpdateDto(
        [Required] int ID,
        [Required] [StringLength(50)] string Ma_Quang,
        [Required] [StringLength(200)] string Ten_Quang,
        [Required] int Loai_Quang,
        bool Dang_Hoat_Dong = true,
        string? Ghi_Chu = null,
        int? Nguoi_Tao = null
    );

    public record QuangUpsertDto(
        int? ID,
        QuangCreateDto Quang
    );

    public record QuangThanhPhanHoaHocDto(
        [Required] int ID_TPHH,
        [Required] [Range(0, 100)] decimal Gia_Tri_PhanTram,
        int? ThuTuTPHH = null,
        decimal? KhoiLuong = null,
        string? CalcFormula = null,
        bool? IsCalculated = null
    );

    public record QuangGiaDto(
        [Range(0, double.MaxValue, ErrorMessage = "Giá USD phải lớn hơn hoặc bằng 0")] decimal Gia_USD_1Tan, // Không bắt buộc, cho phép 0 (nguyên liệu xoay vòng)
        [Range(0, double.MaxValue, ErrorMessage = "Tỷ giá phải lớn hơn hoặc bằng 0")] decimal Ty_Gia_USD_VND, // Không bắt buộc, có thể là 0
        [Range(0, double.MaxValue, ErrorMessage = "Giá VND phải lớn hơn hoặc bằng 0")] decimal Gia_VND_1Tan, // Không bắt buộc, có thể là 0
        DateTimeOffset Ngay_Chon_TyGia
    );


    // Generic upsert for ores with composition (supports Xỉ with optional ID_Quang_Gang)
    public record QuangUpsertWithThanhPhanDto(
        int? ID,
        [Required] [StringLength(50)] string Ma_Quang,
        [Required] [StringLength(200)] string Ten_Quang,
        [Required] int Loai_Quang,
        bool Dang_Hoat_Dong = true,
        string? Ghi_Chu = null,
        [Required] IReadOnlyList<QuangThanhPhanHoaHocDto> ThanhPhanHoaHoc = null!,
        QuangGiaDto? Gia = null,
        int? ID_Quang_Gang = null,
        int? Nguoi_Tao = null,
        bool SaveAsTemplate = false,
        GangTemplateConfigDto? TemplateConfig = null
    );

    public record GangTemplateConfigItemDto(
        int Id,
        int ThuTu
    );

    public record GangTemplateConfigDto(
        IReadOnlyList<GangTemplateConfigItemDto>? ProcessParams = null,
        IReadOnlyList<GangTemplateConfigItemDto>? ThongKes = null
    );

    public record GangDichConfigUpsertDto(
        [Required] QuangUpsertWithThanhPhanDto Gang,
        QuangUpsertWithThanhPhanDto? Slag,
        GangTemplateConfigDto? TemplateConfig = null
    );

    // DTO for creating/updating Gang/Xỉ result ores with plan mapping
    public record QuangKetQuaUpsertDto(
        int? ID,
        [Required] [StringLength(50)] string Ma_Quang,
        [Required] [StringLength(200)] string Ten_Quang,
        [Required] [Range(2, 4)] int Loai_Quang, // 2=Gang, 4=Xỉ
        [Required] IReadOnlyList<QuangThanhPhanHoaHocDto> ThanhPhanHoaHoc,
        [Required] int ID_PhuongAn, // Required plan ID for mapping
        bool Dang_Hoat_Dong = true,
        string? Ghi_Chu = null,
        int? ID_Quang_Gang = null, // For Xỉ: link to Gang
        int? Nguoi_Tao = null
    );

}