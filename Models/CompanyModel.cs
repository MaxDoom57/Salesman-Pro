using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.Models
{
    public class CompanyModel
    {
        [Table("CompanyProject")]
        public class CompanyProject
        {
            [Key]
            public int CPKy { get; set; }
            [Column("CKy")] public required int companyKey { get; set; }
            [Column("PrjKy")] public required int projectKey { get; set; }
            [Column("DbServer")] public required string dbServer { get; set; }
            [Column("DbName")] public required string dbName { get; set; }
            [Column("DbUser")] public string? dbUser { get; set; }
            [Column("DbPassword")] public string? dbPassword { get; set; }
        }
    }
}
