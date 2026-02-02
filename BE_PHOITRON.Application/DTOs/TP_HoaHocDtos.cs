using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record TP_HoaHocCreateDto(
        [Required] string Ma_TPHH,
        string? Ten_TPHH,
        string Don_Vi = "%",
        int? Thu_Tu = null,
        string? Ghi_Chu = null,
        int? Nguoi_Tao = null
    );

    public record TP_HoaHocUpdateDto(
        [Required] int ID,
        [Required] string Ma_TPHH,
        string? Ten_TPHH,
        string Don_Vi = "%",
        int? Thu_Tu = null,
        string? Ghi_Chu = null,
        int? Nguoi_Tao = null
    );

    public record TP_HoaHocUpsertDto(
        int? ID,
        TP_HoaHocCreateDto TP_HoaHoc
    );
}
