using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.DataEntities
{
    public class Quang_TPHH
    {
        [Key]
        public int ID { get; set; }
        public int ID_Quang { get; set; }
        public int ID_TPHH { get; set; }
        public decimal PhanTram { get; set; }
    }
}
