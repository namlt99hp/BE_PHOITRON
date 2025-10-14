using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record CTP_ChiTiet_QuangCreateDto(
        [Required] int ID_Cong_Thuc_Phoi,
        [Required] int ID_Quang_DauVao,
        [Required] [Range(0, 100)] decimal Ti_Le_Phan_Tram,
        decimal? He_So_Hao_Hut_DauVao = null,
        int? Thu_Tu = null,
        string? Ghi_Chu = null
    );

    public record CTP_ChiTiet_QuangUpdateDto(
        [Required] int ID,
        [Required] int ID_Cong_Thuc_Phoi,
        [Required] int ID_Quang_DauVao,
        [Required] [Range(0, 100)] decimal Ti_Le_Phan_Tram,
        decimal? He_So_Hao_Hut_DauVao = null,
        int? Thu_Tu = null,
        string? Ghi_Chu = null
    );

    public record CTP_ChiTiet_QuangUpsertDto(
        int? ID,
        CTP_ChiTiet_QuangCreateDto CTP_ChiTiet_Quang
    );
}
