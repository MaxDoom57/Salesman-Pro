using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//This model for GET only, In here used viewTables
namespace Bridge_App.Models
{
    //Items
    [Table("vewItmMas")]
    public class vewItmMas
    {
        [Key]
        public int ItmKy { get; set; }

        [Column("CKy")] public required int companyKey { get; set; }
        [Column("ItmCd")] public required string itemCode { get; set; }
        [Column("ItmTypKy")] public required short itemTypeKey { get; set; }
        [Column("ItmTyp")] public required string itemType { get; set; }
        [Column("PartNo")] public string? partNo { get; set; }
        [Column("ItmNm")] public required string itemName { get; set; }
        [Column("ItmCat1Ky")] public short? itmCat1Ky { get; set; }
        [Column("ItmCat2Ky")] public short? itmCat2Ky { get; set; }
        [Column("ItmCat3Ky")] public short? itmCat3Ky { get; set; }
        [Column("ItmCat4Ky")] public short? itmCat4Ky { get; set; }
        [Column("UnitKy")] public required short unitKey { get; set; }
        [Column("Unit")] public required string unit { get; set; }
        [Column("DisPer")] public float? discountPrecentage { get; set; }
        [Column("CosPri")] public required decimal costPrice { get; set; }
        [Column("SlsPri")] public required decimal salesPrice { get; set; }
        [Column("DisAmt")] public float? discountAmount { get; set; }
    }
}
