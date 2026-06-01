using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Bridge_App.DTOs
{
    [Keyless]
    public class AccountBalanceDetailsDTO
    {
        public DateTime? TrnDt { get; set; }
        public string? AccCd { get; set; }
        public decimal? Amount { get; set; }
        public decimal? BalAmt { get; set; }
    }
}
