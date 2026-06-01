using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.Models
{
    public class InvoiceModel
    {
        [Table("TrnMas")]
        public class TrnMas
        {
            [Key]
            public int TrnKy { get; set; }

            [Column("CKy")] public required short companyKey { get; set; }
            [Column("TrnDt")] public required DateTime transactionDate { get; set; }
            [Column("TrnTypKy")] public short? transactionTypeKey { get; set; }
            [Column("TrnNo")] public required int tranNo { get; set; }
            [Column("fInAct")] public bool fInAct { get; set; }
            [Column("fApr")] public byte fApr { get; set; }
            //[Column("STrnKy")] public int STrnKy { get; set; } //0
            [Column("RtnAmt")] public decimal? returnAmount { get; set; } //0
            [Column("RepAdrKy")] public int repAddressKey { get; set; } //0

            [StringLength(10, ErrorMessage = "Our Code length should be less than 10 characters")]
            [Column("OurCd")] public required string ourCode { get; set; }

            [StringLength(15, ErrorMessage = "DocNo length should be less than 15 characters")]
            [Column("DocNo")] public string? docNo { get; set; }

            [StringLength(25, ErrorMessage = "YourReference length should be less than 25 characters")]
            [Column("YurRef")] public string? yourReference { get; set; }
            [Column("Adrky")] public required int addressKey { get; set; }
            [Column("AccKy")] public int accountKey { get; set; }
            [Column("PmtTrmKy")] public short paymentTermKey { get; set; }
            [Column("PmtTrmKy2")] public short? paymentTermKey2 { get; set; }
            [Column("PmtTrm1Amt")] public decimal PmtTrm1Amount { get; set; }
            [Column("PmtTrm2Amt")] public decimal? PmtTrm2Amount { get; set; }
            [Column("Amt")] public required decimal amount { get; set; }
            [Column("BUKy")] public short? BUKy { get; set; }
            [Column("LocKy")] public required short locationKey { get; set; }
            [Column("DisPer")] public decimal? discountPresentage { get; set; }

            [StringLength(60, ErrorMessage = "Description length should be less than 60 characters")]
            [Column("Des")] public string? description { get; set; }
            [Column("EntUsrKy")] public int enteredUserKey { get; set; }
            [Column("EntDtm")] public DateTime EntDtm { get; set; }

        }

        [Table("ItmTrn")]
        public class ItmTrn
        {
            [Key]
            public int ItmTrnKy { get; set; }

            [Column("TrnKy")] public required int tranKey { get; set; }
            [Column("LiNo")] public required short lineNo { get; set; }
            [Column("CKy")] public short companyKey { get; set; }
            [Column("RefItmTrnKy")] public int RefItmTrnKy { get; set; }
            [Column("ItmKy")] public required int itemKey { get; set; }
            [Column("ItmTrnRem")] public string? ItmTrnRem { get; set; }
            [Column("LocKy")] public required short locationKey { get; set; }
            [Column("Qty")] public required decimal quantity { get; set; }
            [Column("DisAmt")] public decimal discountAmount { get; set; }
            [Column("DisPer")] public decimal discountPresentage { get; set; }
            [Column("CosPri")] public decimal costPrice { get; set; }
            [Column("SlsPri")] public decimal salesPrice { get; set; }
            [Column("TrnPri")] public decimal tranPrice { get; set; }
            [Column("EntUsrKy")] public int enteredUserKey { get; set; }
        }

        [Table("AccTrn")]
        public class AccTrn
        {
            [Key]
            public int AccTrnKy { get; set; }

            [Column("TrnKy")] public required int tranKey { get; set; }
            [Column("LiNo")] public required short lineNo { get; set; }
            [Column("AccKy")] public required int accountKey { get; set; }
            [Column("PmtModeKy")] public required short paymentModeKey { get; set; }
            [Column("Amt")] public required short amount { get; set; }
            [Column("EntUsrKy")] public int enteredUserKey { get; set; }
        }
    }
}
