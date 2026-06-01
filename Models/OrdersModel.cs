using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.Models
{
    public class OrdersModel
    {
        [Table("OrdMas")]
        public class Orders
        {
            [Key]
            public int OrdKy { get; set; }

            [Column("OrdTypKy")] public required short orderTypekey { get; set; }
            [Column("OrdNo")] public int? orderNumber { get; set; }
            [Column("fInAct")] public required bool fInAct { get; set; }
            [Column("fApr")] public required byte fApr { get; set; }
            [Column("CKy")] public int? companyKey { get; set; } //****
            [Column("OrdTyp")] public required string OrderType { get; set; }
            [Column("PmtTrmKy")] public short? paymentTermKey { get; set; }
            [Column("OrdDt")] public required DateTime orderDate { get; set; }
            [Column("Des")] public string? description { get; set; }
            [Column("Adrky")] public required int addressKey { get; set; }
            [Column("AccKy")] public required int accountKey { get; set; }
            [Column("RepAdrKy")] public required int RepKey { get; set; }
            [Column("OrdCat1Ky")] public short? orderCategory1 { get; set; }
            [Column("DisPer")] public float? discountPrecentage { get; set; }
            [Column("SlsPri")] public required decimal salesPrice { get; set; }
            [Column("EntUsrKy")] public int? enteredUserKey { get; set; }
        }


        [Table("OrdDet")]
        public class OrderLines
        {
            [Key]
            public int OrdDetKy { get; set; }

            [Column("Ordky")] public required int orderKey { get; set; }
            [Column("LiNo")] public required double lineNo { get; set; }
            [Column("fApr")] public required bool fApr { get; set; }
            [Column("ItmKy")] public required int itemKey { get; set; }
            [Column("ItmCd")] public string? itemCode { get; set; }
            [Column("Des")] public string? description { get; set; }
            [Column("EstQty")] public double? estimateQuantity { get; set; }
            [Column("OrdQty")] public required double orderQuantity { get; set; }
            [Column("CosPri")] public required decimal costPrice { get; set; }
            [Column("SlsPri")] public required decimal salesPrice { get; set; }
            [Column("DisPer")] public float? discountPrecentage { get; set; }
            [Column("DisAmt")] public decimal? discountAmount { get; set; }
            [Column("AdrKy")] public required int addressKey { get; set; }
            [Column("EntUsrKy")] public int? enteredUserKey { get; set; }
            [Column("EntDtm")] public DateTime? enterDateTime { get; set; }
        }
    }
}
