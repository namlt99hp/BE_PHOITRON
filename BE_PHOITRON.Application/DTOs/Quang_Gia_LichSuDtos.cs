using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record Quang_Gia_LichSuCreateDto(
        [Required] int ID_Quang,
        [Required] [Range(0, double.MaxValue)] decimal Don_Gia_USD_1Tan,
        [Required] [Range(0, double.MaxValue)] decimal Don_Gia_VND_1Tan,
        [Required] [Range(0, double.MaxValue)] decimal Ty_Gia_USD_VND,
        [Required] DateTimeOffset Hieu_Luc_Tu,
        string Tien_Te = "USD",
        DateTimeOffset? Hieu_Luc_Den = null,
        string? Ghi_Chu = null,
        int? Created_By_User_ID = null
    );

    public record Quang_Gia_LichSuUpdateDto(
        [Required] int ID,
        [Required] int ID_Quang,
        [Required] [Range(0, double.MaxValue)] decimal Don_Gia_USD_1Tan,
        [Required] [Range(0, double.MaxValue)] decimal Don_Gia_VND_1Tan,
        [Required] [Range(0, double.MaxValue)] decimal Ty_Gia_USD_VND,
        [Required] DateTimeOffset Hieu_Luc_Tu,
        string Tien_Te = "USD",
        DateTimeOffset? Hieu_Luc_Den = null,
        string? Ghi_Chu = null,
        int? Created_By_User_ID = null
    );

    public record Quang_Gia_LichSuUpsertDto(
        int? ID,
        Quang_Gia_LichSuCreateDto Quang_Gia_LichSu
    );
}
