using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    /// <summary>
    /// DTO for plan comparison response
    /// </summary>
    public record PlanComparisonResponse(
        int PlanId,
        string PlanName,
        DateTimeOffset NgayTinhToan,
        PlanComparisonThieuKetData? ThieuKet,
        PlanComparisonLoCaoData? LoCao,
        PlanComparisonGangData Gang,
        PlanComparisonXaData Xa,
        List<PlanComparisonThongKeData> ThongKe
    );

    /// <summary>
    /// DTO for plan comparison matrix response (for UI table display)
    /// </summary>
    public record PlanComparisonMatrixResponse(
        List<PlanComparisonMatrixItem> QuangThanhPhanMatrix,
        List<PlanComparisonMatrixItem> ThamSoDacBietMatrix,
        List<PlanComparisonMatrixItem> GangThanhPhanMatrix,
        List<PlanComparisonMatrixItem> XaThanhPhanMatrix,
        List<PlanComparisonMatrixItem> ThongKeMatrix
    );

    /// <summary>
    /// Matrix item for comparison table
    /// </summary>
    public record PlanComparisonMatrixItem(
        string ItemName,
        string? ItemCode,
        string? Unit,
        Dictionary<int, decimal?> PlanValues // PlanId -> Value
    );

    /// <summary>
    /// Thiêu kết section data
    /// </summary>
    public record PlanComparisonThieuKetData(
        List<PlanComparisonQuangThanhPhan> QuangThanhPhan,
        List<PlanComparisonThamSoDacBiet> ThamSoDacBiet
    );

    /// <summary>
    /// Lò cao section data
    /// </summary>
    public record PlanComparisonLoCaoData(
        List<PlanComparisonQuangThanhPhan> QuangThanhPhan,
        List<PlanComparisonThamSoDacBiet> ThamSoDacBiet
    );

    /// <summary>
    /// Quặng thành phần trong công thức phối
    /// </summary>
    public record PlanComparisonQuangThanhPhan(
        int CongThucPhoiId,
        string MaCongThuc,
        string TenCongThuc,
        int QuangId,
        string MaQuang,
        string TenQuang,
        decimal TiLePhanTram
    );

    /// <summary>
    /// Tham số đặc biệt (khấu hao, tỷ lệ khấu hao, KL vào lò, tỷ lệ hồi quặng, KL nhận)
    /// </summary>
    public record PlanComparisonThamSoDacBiet(
        int CongThucPhoiId,
        string MaCongThuc,
        string TenCongThuc,
        int QuangId,
        string MaQuang,
        string TenQuang,
        decimal? KhauHao,
        decimal? TiLeKhauHao,
        decimal? KLVaoLo,
        decimal? TiLeHoiQuang,
        decimal? KLNhan
    );

    /// <summary>
    /// Gang kết quả data
    /// </summary>
    public record PlanComparisonGangData(
        int QuangId,
        string MaQuang,
        string TenQuang,
        List<PlanComparisonThanhPhanHoaHoc> ThanhPhanHoaHoc
    );

    /// <summary>
    /// Xỉ kết quả data
    /// </summary>
    public record PlanComparisonXaData(
        int QuangId,
        string MaQuang,
        string TenQuang,
        List<PlanComparisonThanhPhanHoaHoc> ThanhPhanHoaHoc
    );

    /// <summary>
    /// Thành phần hóa học
    /// </summary>
    public record PlanComparisonThanhPhanHoaHoc(
        int TPHHId,
        string MaTPHH,
        string TenTPHH,
        decimal TiLePhanTram,
        decimal? KhoiLuong
    );

    /// <summary>
    /// Thống kê data
    /// </summary>
    public record PlanComparisonThongKeData(
        int ThongKeFunctionId,
        string MaThongKe,
        string TenThongKe,
        string? MoTa,
        string? DonVi,
        decimal GiaTri,
        int ThuTu
    );
}
