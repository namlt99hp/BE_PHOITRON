using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.DataEntities
{
    public class CongThucPhoi_TPHH
    {
        [Key]
        public int ID { get; set; }
        public int ID_CongThucPhoi { get; set; }
        public int ID_TPHH { get; set; }
        public decimal? Min_PhanTram { get; set; }
        public decimal? Max_PhanTram { get; set; }
    }
}
