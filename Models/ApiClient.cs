using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//This Model for retrive user and device verificaation data 
namespace Bridge_App.Models
{
    [Table("UsrMas")]
    public class UsrMas
    {
        [Key]
        public int UsrKy { get; set; }

        [Required]
        public string UsrId { get; set; } = string.Empty;

        [Required]
        public string UsrNm { get; set; } = string.Empty;
        
        [Required]
        public bool fInAct { get; set; } = false;

        [Required]
        public string? SKey {  get; set; } 
    }
}
