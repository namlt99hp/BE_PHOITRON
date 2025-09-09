using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.DataEntities
{
    public class CongThucPhoi_Quang
    {
        [Key]
        public int ID { get; set; }
        public int ID_CongThucPhoi { get; set; }
        public int ID_Quang { get; set; }
        public decimal TiLePhoi { get; set; }
    }
}
