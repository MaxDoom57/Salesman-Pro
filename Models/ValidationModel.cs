using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.Models
{
    public class ValidationModel
    {
        [Table("Company")]
        public class Company
        {
            [Key]
            public int CKy { get; set; }
            [Column("PrjObjKy")] public required int projectKey { get; set; }
            [Column("fInAct")] public bool? fInAct { get; set; }
        }

        [Table("CdMas")]
        public class CdMas
        {
            [Key]
            public short CdKy { get; set; }

            [Column("ConCd")] public string ConCd { get; set; } = string.Empty;
            [Column("OurCd")] public string OurCd { get; set; } = string.Empty;
            [Column("Code")] public string Code { get; set; } = string.Empty;
            [Column("fInAct")] public bool? fInAct { get; set; }
        }

        [Table("UnitCnv")]
        public class UnitCnv
        {
            [Key]
            public int UnitKy { get; set; }
            [Column("fInAct")] public bool? fInAct { get; set; }
        }

        [Table("UsrMas")]
        public class Users
        {
            [Key]
            public int UserKy { get; set; }
            [Column("UsrId")] public required string userId { get; set; }
            [Column("fInAct")] public bool? fInAct { get; set; }


        }

        [Table("OrdNoLst")]
        public class OrderNumberLast
        {
            [Key]
            public int OrdNoLstKy { get; set; }
            [Column("CKy")] public required int companyKey { get; set; }
            [Column("OurCd")] public required string ourCode { get; set; }
            [Column("LstOrdNo")] public int lastOrderNo { get; set; }
            [Column("fInAct")] public bool fInAct { get; set; }
        }

        [Table("TrnNoLst")]
        public class TrnNoLst
        {
            [Key]
            public int TrnNoLstKy { get; set; }
            [Column("CKy")] public required short companyKey { get; set; }
            [Column("OurCd")] public required string ourCode { get; set; }
            [Column("LstTrnNo")] public int lastTransactionNo { get; set; }
            [Column("fInAct")] public bool fInAct { get; set; }
        }

        [Table("vewTrnNo")]
        public class vewTrnNo
        {
            [Key]
            public int TrnKy { get; set; }
            [Column("CKy")] public short companyKey { get; set; }
            [Column("TrnNo")] public int tranNo { get; set; }
            [Column("OurCd")] public string ourCode { get; set; }
        }

        [Table("vewPmtTrmToPrmMode")]
        [Keyless]
        public class vewPmtTrmToPrmMode
        {
            [Column("PmtTrmKy")] public int PmtTrmKy { get; set; }
            [Column("PmtModeKy")] public int PmtModeKy { get; set; }

        }

        [Table("vewCdMas")]
        [Keyless]
        public class vewCdMas
        {
            [Column("CKy")] public short CKy { get; set; }
            [Column("ConCd")] public string? ConCd { get; set; }
            [Column("OurCd")] public string? OurCd { get; set; }
            [Column("CdNo1")] public double CdNo1 { get; set; }
            [Column("CdNo2")] public double CdNo2 { get; set; }
            [Column("CdNo3")] public double CdNo3 { get; set; }
        }

        [Table("UsrAdrRel")]
        public class UsrAdrRel
        {
            [Key]
            [Column("UsrAdrRelKy")] public int UsrAdrRelKy { get; set; }
            [Column("UsrKy")] public int UsrKy { get; set; }
            [Column("AdrKy")] public int AdrKy { get; set; }
        }
    }
}
