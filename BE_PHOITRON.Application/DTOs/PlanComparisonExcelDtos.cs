using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    /// <summary>
    /// Excel-like plan comparison response model
    /// </summary>
    public record PlanComparisonExcelResponse(
        List<PlanComparisonExcelRow> ThieuKetSection,
        List<PlanComparisonExcelRow> LoCaoSection,
        List<PlanComparisonExcelRow> GiaThanhGangSection,
        List<PlanComparisonExcelRow> SoSanhGiaThanhSection,
        List<PlanComparisonExcelColumn> PlanColumns
    );

    /// <summary>
    /// Excel row data
    /// </summary>
    public record PlanComparisonExcelRow(
        string RowName,
        string? Unit,
        bool IsBold,
        bool IsSectionHeader,
        bool IsSubSectionHeader,
        string? BackgroundColor,
        Dictionary<int, decimal?> PlanValues, // PlanId -> Value
        Dictionary<int, string?> PlanTextValues // PlanId -> Text Value (for formulas, etc.)
    );

    /// <summary>
    /// Plan column information
    /// </summary>
    public record PlanComparisonExcelColumn(
        int PlanId,
        string PlanName,
        string PlanCode
    );

    /// <summary>
    /// Raw data for building Excel comparison
    /// </summary>
    public record PlanComparisonRawData(
        int PlanId,
        string PlanName,
        string PlanCode,
        DateTimeOffset NgayTinhToan,
        
        // Thiêu kết data
        List<ThieuKetQuangData> ThieuKetQuang,
        List<ThieuKetKPIData> ThieuKetKPI,
        
        // Lò cao data
        List<LoCaoQuangData> LoCaoQuang,
        List<LoCaoKPIData> LoCaoKPI,
        
        // Gang kết quả
        List<GangThanhPhanData> GangThanhPhan,
        
        // Xỉ kết quả
        List<XaThanhPhanData> XaThanhPhan,
        
        // Thống kê
        List<ThongKeData> ThongKe,
        
        // Giá thành
        List<GiaThanhData> GiaThanh
    );

    /// <summary>
    /// Thiêu kết quặng data
    /// </summary>
    public record ThieuKetQuangData(
        string MaQuang,
        string TenQuang,
        decimal TiLePhanTram,
        decimal? KhauHao,
        decimal? TiLeKhauHao
    );

    /// <summary>
    /// Thiêu kết KPI data
    /// </summary>
    public record ThieuKetKPIData(
        string KpiName,
        string Unit,
        decimal Value
    );

    /// <summary>
    /// Lò cao quặng data
    /// </summary>
    public record LoCaoQuangData(
        string MaQuang,
        string TenQuang,
        decimal TiLePhanTram,
        decimal? KLVaoLo,
        decimal? TiLeHoiQuang,
        decimal? KLNhan
    );

    /// <summary>
    /// Lò cao KPI data
    /// </summary>
    public record LoCaoKPIData(
        string KpiName,
        string Unit,
        decimal Value
    );

    /// <summary>
    /// Gang thành phần data
    /// </summary>
    public record GangThanhPhanData(
        string MaTPHH,
        string TenTPHH,
        decimal TiLePhanTram,
        decimal? KhoiLuong
    );

    /// <summary>
    /// Xỉ thành phần data
    /// </summary>
    public record XaThanhPhanData(
        string MaTPHH,
        string TenTPHH,
        decimal TiLePhanTram,
        decimal? KhoiLuong
    );

    /// <summary>
    /// Thống kê data
    /// </summary>
    public record ThongKeData(
        string MaThongKe,
        string TenThongKe,
        string? DonVi,
        decimal GiaTri
    );

    /// <summary>
    /// Giá thành data
    /// </summary>
    public record GiaThanhData(
        string ComponentName,
        string Unit,
        decimal Value
    );
}

