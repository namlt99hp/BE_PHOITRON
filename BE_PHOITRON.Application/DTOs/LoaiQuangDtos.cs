using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record LoaiQuangCreateDto(
        [Required] [MaxLength(50)] string MaLoaiQuang,
        [Required] [MaxLength(255)] string TenLoaiQuang,
        [MaxLength(500)] string? MoTa = null,
        bool IsActive = true,
        int? NguoiTao = null
    );

    public record LoaiQuangUpdateDto(
        [Required] int ID,
        [Required] [MaxLength(50)] string MaLoaiQuang,
        [Required] [MaxLength(255)] string TenLoaiQuang,
        [MaxLength(500)] string? MoTa = null,
        bool IsActive = true,
        int? NguoiTao = null
    );

    public record LoaiQuangUpsertDto(
        int? ID,
        LoaiQuangCreateDto LoaiQuang
    );

    public record LoQuangCreateDto(
        [Required] [MaxLength(100)] string MaLoQuang,
        [MaxLength(500)] string? MoTa = null,
        bool IsActive = true,
        int? NguoiTao = null
    );

    public record LoQuangUpdateDto(
        [Required] int ID,
        [Required] [MaxLength(100)] string MaLoQuang,
        [MaxLength(500)] string? MoTa = null,
        bool IsActive = true,
        int? NguoiTao = null
    );

    public record LoQuangUpsertDto(
        int? ID,
        LoQuangCreateDto LoQuang
    );
}

