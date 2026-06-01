using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.Models
{
    public class CollectionModel
    {
        [Table("CollectionDt")]
        public class CollectionDt
        {
            [Key]
            public int ColDtKy { get; set; }

            [Column("CKy")] public required int companyKey { get; set; }
            [Column("ColNo")] public required int collectionNumber { get; set; }
            [Column("ColId")] public required int collectionId { get; set; }
            [Column("Amt")] public required decimal Amt { get; set; }
            [Column("TrnDt")] public required DateTime tranDateTime { get; set; }
            [Column("AdrKy")] public required int addressKey { get; set; }
            [Column("PmtMode")] public required string paymentMode { get; set; }
            [Column("ChqNo")] public required string chequeNo { get; set; }
            [Column("RepKy")] public required int repKey { get; set; }
            [Column("fInAct")] public required bool fInAct { get; set; }
            [Column("fSync")] public required bool fSync { get; set; }
            [Column("fPost")] public required bool fPost { get; set; }
            [Column("PostUsrKy")] public required int PostUserKey { get; set; }

        }
    }
}
