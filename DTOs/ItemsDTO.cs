using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.DTOs
{
    public class ItemsDTO
    {
        //Add Items
        public class AddItemDTO
        {
            public required int companyKey { get; set; }
            public required string itemCode { get; set; }
            public required short itemTypeKey { get; set; }
            public required string itemType { get; set; }
            public string? partNo { get; set; }
            public required string itemName { get; set; }
            public string? description { get; set; }
            public required short locationKey { get; set; }
            public short? itmCat1Ky { get; set; }
            public short? itmCat2Ky { get; set; }
            public short? itmCat3Ky { get; set; }
            public short? itmCat4Ky { get; set; }
            public required short unitKey { get; set; }
            public double? discountPrecentage { get; set; }
            public required decimal costPrice { get; set; }
            public required decimal salesPrice { get; set; }
            public double? discountAmount { get; set; }
            public double? quantity { get; set; }
            public required int enteredUser { get; set; }
            public bool? fInAct { get; set; }
        }

        public class UpdateItemDTO
        {
            public  int companyKey { get; set; }
            public  string? itemCode { get; set; }
            public  short itemTypeKey { get; set; }
            public  string? itemType { get; set; }
            public string? partNo { get; set; }
            public  string? itemName { get; set; }
            public string? description { get; set; }
            public  short locationKey { get; set; }
            public short? itmCat1Ky { get; set; }
            public short? itmCat2Ky { get; set; }
            public short? itmCat3Ky { get; set; }
            public short? itmCat4Ky { get; set; }
            public  short unitKey { get; set; }
            public double? discountPrecentage { get; set; }
            public  decimal costPrice { get; set; }
            public  decimal salesPrice { get; set; }
            public double? discountAmount { get; set; }
            public double? quantity { get; set; }
            public required int enteredUser { get; set; }
        }

        public class VaidateItemDTO
        {
            public required string itemCode { get; set; }
        }
    }
}
