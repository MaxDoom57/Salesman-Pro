using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.Models
{
    [Table("AccAdr")]
    public class AccAdrModel
    {
        public int AccKy { get; set; }
        public int AdrKy { get; set; }
    }
}
