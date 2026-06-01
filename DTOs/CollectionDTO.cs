using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.DTOs
{
    public class CollectionDTO
    {
        public class AddCollectionDTO
        {
            public required int companyKey { get; set; }
            public required int collectionNumber { get; set; }
            public required int collectionId { get; set; }
            public required decimal amount { get; set; }
            public required DateTime tranDateTime { get; set; }
            public required int addressKey { get; set; }
            public required string paymentMode { get; set; }
            public string? chequeNo { get; set; }
            public required int repKey { get; set; }
            public required bool fInAct { get; set; }
            public required bool fSync { get; set; }
            public required bool fPost { get; set; }
            public required int PostUserKey { get; set; }
        }
    }
}
