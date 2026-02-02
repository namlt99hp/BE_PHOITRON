using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record Cong_Thuc_PhoiCreateDto(
        [Required] int ID_Quang_DauRa,
        [Required] string Ma_Cong_Thuc,
        [Required] DateTimeOffset Hieu_Luc_Tu,
        string? Ten_Cong_Thuc = null,
        [Required] [Range(0.0001, double.MaxValue)] decimal He_So_Thu_Hoi = 1.0000m,
        [Required] [Range(0, double.MaxValue)] decimal Chi_Phi_Cong_Doạn_1Tan = 0,
        int Phien_Ban = 1,
        [Range(0, 2)] byte Trang_Thai = 0,
        DateTimeOffset? Hieu_Luc_Den = null,
        string? Ghi_Chu = null,
        int? Nguoi_Tao = null
    );

    public record Cong_Thuc_PhoiUpdateDto(
        [Required] int ID,
        [Required] int ID_Quang_DauRa,
        [Required] string Ma_Cong_Thuc,
        [Required] DateTimeOffset Hieu_Luc_Tu,
        string? Ten_Cong_Thuc = null,
        [Required] [Range(0.0001, double.MaxValue)] decimal He_So_Thu_Hoi = 1.0000m,
        [Required] [Range(0, double.MaxValue)] decimal Chi_Phi_Cong_Doạn_1Tan = 0,
        int Phien_Ban = 1,
        [Range(0, 2)] byte Trang_Thai = 0,
        DateTimeOffset? Hieu_Luc_Den = null,
        string? Ghi_Chu = null,
        int? Nguoi_Tao = null
    );

    public record Cong_Thuc_PhoiUpsertDto(
        int? ID,
        Cong_Thuc_PhoiCreateDto Cong_Thuc_Phoi
    );
}
