using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record QuangCreateDto(
        [Required] [StringLength(50)] string Ma_Quang,
        [Required] [StringLength(200)] string Ten_Quang,
        [Required] [Range(0, 3)] int Loai_Quang, // 0=Mua, 1=Tron, 2=Gang, 3=Khac
        bool Dang_Hoat_Dong = true,
        string? Ghi_Chu = null
    );

    public record QuangUpdateDto(
        [Required] int ID,
        [Required] [StringLength(50)] string Ma_Quang,
        [Required] [StringLength(200)] string Ten_Quang,
        [Required] [Range(0, 3)] int Loai_Quang,
        bool Dang_Hoat_Dong = true,
        string? Ghi_Chu = null
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
        [Required] decimal Gia_USD_1Tan,
        [Required] decimal Ty_Gia_USD_VND,
        [Required] decimal Gia_VND_1Tan,
        DateTimeOffset Ngay_Chon_TyGia
    );


    // Generic upsert for ores with composition (supports Xỉ with optional ID_Quang_Gang)
    public record QuangUpsertWithThanhPhanDto(
        int? ID,
        [Required] [StringLength(50)] string Ma_Quang,
        [Required] [StringLength(200)] string Ten_Quang,
        [Required] [Range(0, 4)] int Loai_Quang,
        bool Dang_Hoat_Dong = true,
        string? Ghi_Chu = null,
        [Required] IReadOnlyList<QuangThanhPhanHoaHocDto> ThanhPhanHoaHoc = null!,
        QuangGiaDto? Gia = null,
        int? ID_Quang_Gang = null
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
        int? ID_Quang_Gang = null // For Xỉ: link to Gang
    );
}
