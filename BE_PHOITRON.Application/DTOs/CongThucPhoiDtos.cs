using BE_PHOITRON.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.DTOs
{
    public record CreateCongThucPhoiDto(
        string MaCongThuc,
        string? TenCongThuc,
        string? GhiChu,
        int ID_NguoiTao
    );
    public record UpdateCongThucPhoiDto(
        string? TenCongThuc,
        decimal? TongPhanTram,
        string? GhiChu,
        int ID_NguoiSua
    );

    public record CongThuc_TPHHItem(int ID_TPHH, decimal? Min_PhanTram, decimal? Max_PhanTram);
    public record CongThuc_QuangItem(int ID_Quang, decimal TiLePhoi);
    public record ThanhPhamDto(int ID_TPHH, decimal PhanTram);
    public record UpdateCongThucPTDto(
        int ID,

        List<CongThuc_TPHHItem>? ListTPHH,
        List<CongThuc_QuangItem>? ListQuang
    );

    public record UpsertCongThucPTDto
    {
        public int ID { get; init; } // <=0: create, >0: update

        public string? MaCongThuc { get; init; }
        public string? TenCongThuc { get; init; }
        public decimal? TongPhanTram { get; init; }
        public string? GhiChu { get; init; }
        public int? ID_NguoiTao { get; init; }
        public int? ID_NguoiSua { get; init; }
        public List<CongThuc_TPHHItem>? ListTPHH { get; init; } // null: không đụng; []: xoá sạch; có phần tử: đồng bộ
        public List<CongThuc_QuangItem>? ListQuang { get; init; }
    }

    public sealed record UpsertAndConfirmDto(
        CongThucPhoiUpsertDto CongThucPhoi,        // header công thức + inputs + (tuỳ) min/max
        QuangTron Quang,    // header quặng thành phẩm
        IReadOnlyList<TPHHResultItems> KetQuaTPHHtItems // %TPHH do FE đã tính
    );

    public sealed record CongThucPhoiUpsertDto(
        int? ID,                     // null => tạo mới; có ID => update
        int? ID_QuangNeo,            // neo (tuỳ chọn)
        string MaCongThuc,
        string TenCongThuc,
        string? GhiChu,
        decimal? TongPhanTram,
        IReadOnlyList<QuangInputDto> QuangInputs,             // quặng nguồn + tỉ lệ phối
        IReadOnlyList<RangBuocTPHH>? RangBuocTPHHs = null // (tuỳ) min/max TPHH
    );

    public sealed record QuangInputDto(int ID_Quang, decimal TiLePhoi);
    public sealed record RangBuocTPHH(int ID_TPHH, decimal? Min_PhanTram, decimal? Max_PhanTram);

    public sealed record QuangTron(
        string MaQuang,
        string TenQuang,
        decimal? Gia,
        string? GhiChu,
        decimal? MatKhiNung
    );

    public sealed record TPHHResultItems(int ID_TPHH, decimal PhanTram);

    public sealed record UpsertAndConfirmResult(int FormulaId, int OreId);
}
