using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.DTOs
{
    public class OrdersDTO
    {
        public class AddOrderDTO
        {
            public required short orderTypekey { get; set; }
            public int? orderNumber { get; set; }
             public required bool fInAct { get; set; }
            public required byte fApr { get; set; }
            public int? companyKey { get; set; }
            public required string OrderType { get; set; }
            public short? paymentTerm { get; set; }
            public required DateTime orderDate { get; set; }
            public string? description { get; set; }
            public required int addressKey { get; set; }
            public required int accountKey { get; set; }
            public required int RepKey { get; set; }
            public short? orderCategory1 { get; set; }
            public float? discountPrecentage { get; set; }
            public required decimal salesPrice { get; set; }
            public required int enteredUserKey { get; set; }
        }

        public class UpdateOrderDTO
        {
            public short? orderTypekey { get; set; }
            public int? orderNumber { get; set; }
            public byte? fApr { get; set; }
            public int? companyKey { get; set; }
            public string? OrderType { get; set; }
            public short? paymentTerm { get; set; }
            public DateTime? orderDate { get; set; }
            public string? description { get; set; }
            public int? addressKey { get; set; }
            public int? accountKey { get; set; }
            public int? RepKey { get; set; }
            public short? orderCategory1 { get; set; }
            public float? discountPrecentage { get; set; }
            public decimal? salesPrice { get; set; }
            public int enteredUserKey { get; set; }
        }

        public class AddOrderLinesDTO
        {
            public required int orderKey { get; set; }
            public required double lineNo { get; set; }
            public byte? fApr { get; set; }
            public required int itemKey { get; set; }
            public string? itemCode { get; set; }
            public string? description { get; set; }
            public double? estimateQuantity { get; set; }
            public required double orderQuantity { get; set; }
            public decimal? costPrice { get; set; }
            public decimal? salesPrice { get; set; }
            public float? discountPrecentage { get; set; }
            public decimal? discountAmount { get; set; }
            public required int addressKey { get; set; }
            public int enteredUserKey { get; set; }
            public required DateTime enterDateTime { get; set; }
        }

        public class UpdateOrderLinesDTO
        {
            public required int orderKey { get; set; }
            public required double lineNo { get; set; }
            public byte? fApr { get; set; }
            public required int itemKey { get; set; }
            public string? itemCode { get; set; }
            public string? description { get; set; }
            public double? estimateQuantity { get; set; }
            public double? orderQuantity { get; set; }
            public decimal? costPrice { get; set; }
            public decimal? salesPrice { get; set; }
            public float? discountPrecentage { get; set; }
            public decimal? discountAmount { get; set; }
            public int? addressKey { get; set; }
            public int enteredUserKey { get; set; }
            public required DateTime enterDateTime { get; set; }
        }

        public class AddOrderWithLinesDTO
        {
            public required AddOrderDTO newOrder { get; set; }
            public required List<AddOrderLinesDTO> orderLines { get; set; }
        }
    }
}
