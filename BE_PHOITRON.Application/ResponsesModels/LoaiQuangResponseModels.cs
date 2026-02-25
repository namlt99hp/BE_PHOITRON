using System;

namespace BE_PHOITRON.Application.ResponsesModels
{
    public record LoaiQuangResponse(
        int ID,
        string MaLoaiQuang,
        string TenLoaiQuang,
        string? MoTa,
        bool IsActive,
        DateTimeOffset NgayTao,
        int? NguoiTao,
        DateTimeOffset? NgaySua,
        int? NguoiSua
    );

    public record LoQuangResponse(
        int ID,
        string MaLoQuang,
        string? MoTa,
        bool IsActive,
        DateTimeOffset NgayTao,
        int? NguoiTao,
        DateTimeOffset? NgaySua,
        int? NguoiSua
    );
}

