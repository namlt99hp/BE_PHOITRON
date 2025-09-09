using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.ResponsesModel
{
    public record QuangResponse(
        int ID,
        string MaQuang,
        string? TenQuang,
        decimal? Gia,
        string? GhiChu,
        DateTime? NgayTao,
        int? ID_NguoiTao,
        DateTime? NgaySua,
        int? ID_NguoiSua,
        bool IsDeleted,
        decimal? MatKhiNung,
        int LoaiQuang,
        int? ID_CongThucPhoi
    );

    public record QuangDetailResponse (
        QuangResponse Quang,
        List<TPHHOfQuangReponse> TP_HoaHocs
    );

    public record QuangItemResponse(
        int ID,
        string MaQuang,
        string TenQuang,
        decimal? Gia,
        int LoaiQuang,
        bool IsDeleted
    );
}
