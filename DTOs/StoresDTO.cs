using System.ComponentModel.DataAnnotations.Schema;

namespace Bridge_App.DTOs
{
    public class StoresDTO
    {
        public class AddStoresDTO
        {
            public short? companyKey { get; set; }
            public required string addressCode { get; set; }
            public  bool? fInAct { get; set; }
            public  bool? fApr { get; set; }
            public  string? addressName { get; set; }
            public  string? address { get; set; }
            public  string? town { get; set; }
            public  string? city { get; set; }
            public  string? country { get; set; }
            public required string GPS { get; set; }
            public  string? zipCode { get; set; }
            public  string? phoneNumber1 { get; set; }
            public  string? phoneNumber2 { get; set; }
            public  string? eMail { get; set; }

            public string? accountType { get; set; }
            public short? accountTypeKey { get; set; }
            public double? creditLimit { get; set; }
            public string? bankAccountNumber { get; set; }
            public string? bankAccountName { get; set; }
            public int? enteredUserKey { get; set; }
        }
        public class UpdateStoresDTO
        {
            public  short? companyKey { get; set; }
            public required string addressCode { get; set; }
            public string? addressName { get; set; }
            public string? address { get; set; }
            public string? town { get; set; }
            public string? city { get; set; }
            public string? country { get; set; }
            public string? GPS { get; set; }
            public string? zipCode { get; set; }
            public string? phoneNumber1 { get; set; }
            public string? phoneNumber2 { get; set; }
            public string? eMail { get; set; }

            public string? accountType { get; set; }
            public short? accountTypeKey { get; set; }
            public double? creditLimit { get; set; }
            public string? bankAccountNumber { get; set; }
            public string? bankAccountName { get; set; }
            public int? enteredUserKey { get; set; }

        }
        public class DeleteStoresDTO
        {
            public required bool fInAct { get; set; }
        }
    }
}
