using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.DTOs
{
    public record QuangCreateDto(
         string MaQuang,
         string? TenQuang,
         decimal? Gia,
         string? GhiChu,
         decimal? MatKhiNung,
         int? ID_CongThucPhoi,
         bool? IsDeleted
     );

    public record QuangUpdateDto(
        string? TenQuang,
        decimal? Gia,
        string? GhiChu
    );

    public record Quang_TPHHItem(int ID_TPHH, decimal PhanTram);

    public record Quang_TPHHUpdateDto(
        int ID_Quang,
        List<Quang_TPHHItem> Items
    );

    public record UpsertQuangMuaDto(
        int? ID,                              // null => create, otherwise update
        QuangCreateDto Quang,
        IReadOnlyList<Quang_TPHHItem> ThanhPhan
    );
    
}
