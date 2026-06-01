namespace Bridge_App.DTOs
{
    public class InvoiceDTO
    {
        // Used when adding a new invoice (header + items + accounts)
        public class AddInvoiceDTO
        {
            // --- TrnMas (Header) fields ---
            public DateTime TransactionDate { get; set; }
            public int TranNo { get; set; }
            //public int STrnKy { get; set; } = 0;
            public decimal? ReturnAmount { get; set; } = 0;
            public int RepAddressKey { get; set; } = 0;
            public string? DocNo { get; set; }
            public string? YourReference { get; set; }
            public int AddressKey { get; set; }
            public int AccountKey { get; set; }
            public required string PaymentTerm1 { get; set; }
            public string? PaymentTerm2 { get; set; }
            public decimal PmtTrm1Amount { get; set; }
            public decimal? PmtTrm2Amount { get; set; }
            public decimal Amount { get; set; }
            public short? BUKy { get; set; }
            public short LocationKey { get; set; }
            public decimal? DiscountPresentage { get; set; }
            public string? Description { get; set; }
            public DateTime EntDtm { get; set; } = DateTime.Now;

            // --- ItmTrn (Item lines) ---
            public List<ItmTrnDTO> ItemTransactions { get; set; } = new();

            //--- fields for additional functionality ---
            public required decimal totalAmount { get; set; } //total amount before reduceing discount
            public required decimal discountAmount { get; set; }
        }

        // DTO for ItmTrn table
        public class ItmTrnDTO
        {
            public short LineNo { get; set; }
            public int ItemKey { get; set; }
            public short LocationKey { get; set; }
            public decimal Quantity { get; set; }
            public decimal DeliveryQuantity { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal DiscountPresentage { get; set; }
            public decimal CostPrice { get; set; }
            public decimal SalesPrice { get; set; }
            public decimal TranPrice { get; set; }
        }
    }
}
