using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record CTP_RangBuoc_TPHHCreateDto(
        [Required] int ID_Cong_Thuc_Phoi,
        [Required] int ID_TPHH,
        decimal? Min_PhanTram = null,
        decimal? Max_PhanTram = null,
        bool Rang_Buoc_Cung = true,
        byte? Uu_Tien = null,
        string? Ghi_Chu = null
    );

    public record CTP_RangBuoc_TPHHUpdateDto(
        [Required] int ID,
        [Required] int ID_Cong_Thuc_Phoi,
        [Required] int ID_TPHH,
        decimal? Min_PhanTram = null,
        decimal? Max_PhanTram = null,
        bool Rang_Buoc_Cung = true,
        byte? Uu_Tien = null,
        string? Ghi_Chu = null
    );

    public record CTP_RangBuoc_TPHHUpsertDto(
        int? ID,
        CTP_RangBuoc_TPHHCreateDto CTP_RangBuoc_TPHH
    );
}
