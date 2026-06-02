using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.Models
{
    public class ViewStoresModel
    {
        [Table("vewAdrDet")]
        public class ViewStores
        {
            [Key]
            public int AdrKy { get; set; }
            [Column("CKy")] public short? companyKey { get; set; }
            [Column("AdrCd")] public string addressCode { get; set; }
            [Column("fInAct")] public bool fInAct { get; set; }
            //[Column("AdrKy")] public int AddressKey { get; set; }
            [Column("AdrNm")] public string? addressName { get; set; }
            [Column("Address")] public string? address { get; set; }
            [Column("Town")] public string? town { get; set; }
            [Column("City")] public string? city { get; set; }
            [Column("Country")] public string? country { get; set; }
            [Column("GPSLoc")] public string? GPS { get; set; }
            [Column("ZipCd")] public string? zipCode { get; set; }
            [Column("TP1")] public string? phoneNumber1 { get; set; }
            [Column("TP2")] public string? phoneNumber2 { get; set; }
            [Column("EMail")] public string? eMail { get; set; }
            [Column("EntUsrKy")] public int? enteredUserKey { get; set; }
            [Column("AccTyp")] public string? accountType { get; set; }
            [Column("AdrTyp")] public string? AdrTyp { get; set; }
            [Column("AccKy")] public int? accountKey { get; set; }
            [Column("SlsRepKy")] public int? SlsRepKy { get; set; }
        }
    }
}
