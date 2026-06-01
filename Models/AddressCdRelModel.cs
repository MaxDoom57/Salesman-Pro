using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Bridge_App.Models
{
    [Table("AddressCdRel")]
    public class AddressCdRelModel
    {
        [Key]
        public int AddressCdRelKy { get; set; }
        [Column("AddressCdRelTypKy")] public required short AddressCdRelTypKy { get; set; }
        [Column("AdrKy")] public required int addressKey { get; set; }
        [Column("CdKy")] public required short cdKey { get; set; }
        [Column("Lino")] public required short lineNo { get; set; }
        [Column("fApr")] public required bool fApr { get; set; }
        [Column("EntUsrKy")] public int enterdUserKey { get; set; }

    }
}
