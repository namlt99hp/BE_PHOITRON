using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.ResponsesModels
{
    public record TPHHResponse(
        int ID,
        string Ma_TPHH,
        string? Ten_TPHH,
        string? GhiChu,
        DateTime? NgayTao,
        int? ID_NguoiTao,
        DateTime? NgaySua,
        int? ID_NguoiSua,
        bool IsDeletd
    );

    public record TPHHOfQuangReponse(
        int ID,
        string Ma_TPHH,
        string? Ten_TPHH,
        decimal? PhanTram
    );
    public record TPHHOfQuangItem(
        int ID,
        decimal? PhanTram
    );

    public record TPHHItemResponse(
        int ID,
        string Ma_TPHH,
        string Ten_TPHH,
        bool IsDeletd
    );
}
