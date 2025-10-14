using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record Quang_TP_PhanTichCreateDto(
        [Required] int ID_Quang,
        [Required] int ID_TPHH,
        [Required] [Range(0, 100)] decimal Gia_Tri_PhanTram,
        [Required] DateTimeOffset Hieu_Luc_Tu,
        DateTimeOffset? Hieu_Luc_Den = null,
        string? Nguon_Du_Lieu = null,
        string? Ghi_Chu = null,
        int? ThuTuTPHH = null,
        decimal? KhoiLuong = null,
        // Formula calculation fields
        string? CalcFormula = null,
        bool? IsCalculated = false
    );

    public record Quang_TP_PhanTichUpdateDto(
        [Required] int ID,
        [Required] int ID_Quang,
        [Required] int ID_TPHH,
        [Required] [Range(0, 100)] decimal Gia_Tri_PhanTram,
        [Required] DateTimeOffset Hieu_Luc_Tu,
        DateTimeOffset? Hieu_Luc_Den = null,
        string? Nguon_Du_Lieu = null,
        string? Ghi_Chu = null,
        int? ThuTuTPHH = null,
        decimal? KhoiLuong = null,
        // Formula calculation fields
        string? CalcFormula = null,
        bool? IsCalculated = false
    );

    public record Quang_TP_PhanTichUpsertDto(
        int? ID,
        Quang_TP_PhanTichCreateDto Quang_TP_PhanTich
    );
}
