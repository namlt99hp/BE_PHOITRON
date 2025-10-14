namespace BE_PHOITRON.Application.ResponsesModels
{
    // Complex Response Models for Business Logic
    public record PhuongAnTinhToanResponse(
        int ID_Phuong_An,
        string Ten_Phuong_An,
        int ID_Quang_Dich,
        string Quang_Dich_Ma,
        string Quang_Dich_Ten,
        DateTimeOffset Ngay_Tinh_Toan,
        decimal Khoi_Luong_DauRa,
        decimal Tong_Chi_Phi_1Tan,
        Dictionary<string, decimal> TPHH_DauRa,
        Dictionary<string, decimal> Co_Cau_Quang_Tho,
        List<CongThucChiTietResponse> Cong_Thuc_Chi_Tiet
    );

    public record CongThucChiTietResponse(
        int ID_Cong_Thuc_Phoi,
        string Ma_Cong_Thuc,
        string Ten_Cong_Thuc,
        int ID_Quang_DauRa,
        string Quang_DauRa_Ma,
        string Quang_DauRa_Ten,
        decimal He_So_Thu_Hoi,
        decimal Chi_Phi_Cong_Doạn_1Tan,
        decimal Khoi_Luong_DauRa,
        decimal Khoi_Luong_DauVao,
        decimal Chi_Phi_Quang_DauVao,
        List<QuangDauVaoChiTietResponse> Quang_DauVao_Chi_Tiet
    );

    public record QuangDauVaoChiTietResponse(
        int ID_Quang,
        string Ma_Quang,
        string Ten_Quang,
        decimal Ti_Le_Phan_Tram,
        decimal Khoi_Luong,
        decimal Don_Gia_1Tan,
        decimal Tong_Gia_Tri,
        Dictionary<string, decimal> TPHH_Phan_Tich
    );

    // Comparison Response Models
    public record SoSanhPhuongAnResponse(
        List<PhuongAnTinhToanResponse> Phuong_An,
        Dictionary<string, object> Thong_Ke,
        PhuongAnTinhToanResponse? Phuong_An_Toi_Uu
    );

    public record ThongKePhuongAnResponse(
        decimal Chi_Phi_Thap_Nhat,
        decimal Chi_Phi_Cao_Nhat,
        decimal Chi_Phi_Trung_Binh,
        Dictionary<string, decimal> TPHH_Trung_Binh,
        Dictionary<string, int> Top_Quang_Tho_Su_Dung
    );
}
