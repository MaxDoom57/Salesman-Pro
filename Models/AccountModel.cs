using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.Models
{
    public class AccountModel
    {
        [Table("AccMas")]
        public class Accounts
        {
            [Key]
            public int AccKy { get; set; }

            [Column("CKy")] public required short companyKey { get; set; }
            [Column("AccCd")] public required string accountCode { get; set; }
            [Column("fInAct")] public required bool fInAct { get; set; }
            [Column("fDefault")] public required bool fDefault { get; set; }
            [Column("fApr")] public required bool fApr { get; set; }
            [Column("AccNm")] public string? accountName { get; set; }
            [Column("AccTyp")] public string? accountType { get; set; }
            [Column("AccTypKy")] public short? accountTypeKey { get; set; }
            [Column("CrLmt")] public double? creditLimit { get; set; }
            [Column("BnkAccNo")] public string? bankAccountNumber { get; set; }
            [Column("BnkAccNm")] public string? bankAccountName { get; set; }
            [Column("EntUsrKy")] public int EnteredUserKey { get; set; }
        }
    }
}
