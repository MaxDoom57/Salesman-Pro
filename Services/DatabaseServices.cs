using Bridge_App.Data;
using Bridge_App.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection.Emit;
using static Bridge_App.Models.ValidationModel;

namespace Bridge_App.Services
{
    public class DatabaseServices
    {
        private readonly IDynamicConnectionService _connectionService;

        public DatabaseServices(IDynamicConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        // Helper to create dynamic DbContext
        private AppDbContext CreateDynamicContext()
        {
            var connectionString = GlobalSessionContext.Current.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Dynamic connection string is not set for this session.");

            return new AppDbContext(connectionString);
        }

        public async Task<short> GetAccountTypeKey(string accountType)
        {
            using var dynamicContext = CreateDynamicContext();

            var key = await dynamicContext.CdMas
                .Where(c => c.ConCd == "AccTyp" && c.fInAct == false && c.Code == accountType)
                .Select(c => c.CdKy)
                .FirstOrDefaultAsync(); // returns 0 if no match

            if (key == 0)
                throw new Exception($"Account type '{accountType}' not found in CdMas.");

            return key;
        }


        // Get Last order number
        public async Task<int> GetOrderNumberLast(int CKy, string ourCode)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.OrderNumberLast
                .Where(c => c.ourCode == ourCode && !c.fInAct && c.companyKey == CKy)
                .Select(c => c.lastOrderNo)
                .FirstAsync();
        }

        // Increase last order number by 1
        public async Task IncrementOrderNumberLast(int companyKey, string ourCode)
        {
            using var dynamicContext = CreateDynamicContext();
            var record = await dynamicContext.OrderNumberLast
                .FirstOrDefaultAsync(c => c.ourCode == ourCode &&
                                          c.fInAct == false &&
                                          c.companyKey == companyKey);
            if (record == null)
                throw new Exception("Order number record not found.");
            record.lastOrderNo += 1;
            await dynamicContext.SaveChangesAsync();
        }


        public async Task<int> GetUserKey(AppDbContext context, string userId)
        {
            return await context.Users
                .Where(s => s.UsrId == userId && s.fInAct == false)
                .Select(s => s.UsrKy)
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetItemCode(AppDbContext context, int itemKey)
        {
            return await context.Items
                .Where(c => c.ItmKy == itemKey && c.fInAct == false)
                .Select(c => c.itemCode)
                .FirstAsync();
        }

        public async Task<int> GetAddressKeyByAdrCode(AppDbContext context, string addressCode)
        {
            return await context.Stores
                .Where(c => c.addressCode == addressCode)
                .Select(c => c.AdrKy)
                .FirstAsync();
        }
        public async Task<short> GetAddressTypeKey(string addressType)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.CdMas
                .Where(c => c.ConCd == "AdrTyp" && c.fInAct == false && c.Code == addressType)
                .Select(c => c.CdKy)
                .FirstAsync();
        }
        public async Task<int> GetAccountKey(AppDbContext context, string accountCode)
        {
            return await context.Accounts
                .Where(c => c.accountCode == accountCode && c.fInAct == false)
                .Select(c => c.AccKy)
                .FirstAsync();
        }

        //Get last TranNo
        public async Task<int> GetTranNumberLast(int CKy, string ourCode)
        {
            using var context = CreateDynamicContext();

            var existing = await context.TrnNoLst
                .Where(c => c.ourCode == ourCode && !c.fInAct && c.companyKey == CKy)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return existing.lastTransactionNo;
            }

            int newLastTrn = 1;

            string sql = @"
                        INSERT INTO TrnNoLst
                        (fInAct, CKy, SKy, OurCd, CdKy, LstTrnNo, LstDocNo)
                        VALUES
                        (@fInAct, @CKy, @SKy, @OurCd, @CdKy, @LstTrnNo, @LstDocNo);
                    ";

            await context.Database.ExecuteSqlRawAsync(
                sql,
                new SqlParameter("@fInAct", false),
                new SqlParameter("@CKy", CKy),
                new SqlParameter("@SKy", 1),
                new SqlParameter("@OurCd", ourCode),
                new SqlParameter("@CdKy", SqlDbType.SmallInt) { Value = 0 },
                new SqlParameter("@LstTrnNo", newLastTrn),
                new SqlParameter("@LstDocNo", DBNull.Value)
            );

            return newLastTrn;
        }



        // Increase last traan number by 1
        public async Task IncrementTranNumberLast(int companyKey, string ourCode)
        {
            using var dynamicContext = CreateDynamicContext();
            var record = await dynamicContext.TrnNoLst
                .FirstOrDefaultAsync(c => c.ourCode == ourCode &&
                                          c.fInAct == false &&
                                          c.companyKey == companyKey);
            if (record == null)
                throw new Exception("Order number record not found.");
            record.lastTransactionNo += 1;
            await dynamicContext.SaveChangesAsync();
        }

        public async Task<short> GetTranTypeKey(string tranTypeCode)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.CdMas
                .Where(c => c.ConCd == "TrnTyp" && c.fInAct == false && c.Code == tranTypeCode)
                .Select(c => c.CdKy)
                .FirstAsync();
        }

        public async Task<int> GetAccountKeyByAdrKy(int addressKey)
        {
            using var dynamicContext = CreateDynamicContext();

            var accountKey = await dynamicContext.ViewStores
                .Where(c => c.AdrKy == addressKey && c.fInAct == false)
                .Select(c => (int?)c.accountKey) 
                .FirstOrDefaultAsync();

            if (accountKey == null)
                throw new Exception($"No active account found for AdrKy = {addressKey}");

            return accountKey.Value;
        }


        public async Task<short?> GetPaymentTermKey(string paymentTermCode)
        {
            using var dynamicContext = CreateDynamicContext();

            var key = await dynamicContext.CdMas
                .Where(c => c.ConCd == "PmtTrm" && c.OurCd == paymentTermCode && c.fInAct == false)
                .Select(c => c.CdKy)
                .FirstOrDefaultAsync(); // returns 0 if no match

            if (key == 0)
                throw new Exception($"Payment term '{paymentTermCode}' not found in CdMas.");

            return key;
        }

        public async Task<int> GetTranKey(string ourCode, int tranNo)
        {
            using var dynamicContext = CreateDynamicContext();
            var tran = await dynamicContext.TrnMas
                .Where(t => t.ourCode == ourCode && t.tranNo == tranNo)
                .Select(t => t.TrnKy)
                .FirstOrDefaultAsync();

            if (tran == 0)
                return 0; 

            return tran;
        }


        public async Task<short> GetPaymentModeKey(int paymentTermKey)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.vewPmtTrmToPrmMode
                .Where(c => c.PmtTrmKy == paymentTermKey)
                .Select(c => (short)c.PmtModeKy)
                .FirstAsync();
        }

        public async Task<vewCdMas?> GetSaleTransactionCodes(int cKy)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.vewCdMas
                .Where(x => x.OurCd == "SALE" && x.ConCd == "TrnTyp" && x.CKy == cKy)
                .Select(x => new vewCdMas
                {
                    CdNo1 = x.CdNo1,
                    CdNo2 = x.CdNo2,
                    CdNo3 = x.CdNo3
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetDefaultSalesAccountKey(short cKy)
        {
            using var dynamicContext = CreateDynamicContext();

            return await dynamicContext.Accounts
                .Where(x => x.accountType == "SALE" && x.fDefault == true && x.companyKey == cKy)
                .Select(x => (int)x.AccKy)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetSalesRepKeyByUserKey(AppDbContext context, int userKey)
        {
            return await context.UsrAdrRel
                .Where(s => s.UsrKy == userKey)
                .Select(s => s.AdrKy)
                .FirstOrDefaultAsync();
        }
    }
}
