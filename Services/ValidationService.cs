using Bridge_App.Data;
using Bridge_App.Models;
using Bridge_App.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Bridge_App.Services
{
    public class ValidationService
    {
        private readonly IDynamicConnectionService _connectionService;

        public ValidationService(IDynamicConnectionService connectionService)
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

        // Checks if the given companyKey exists in the Company table
        public async Task<bool> IsExistCompanyKey(int companyKey)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.Company.AnyAsync(c => c.CKy == companyKey && c.fInAct == false);
        }

        // Checks if the given itemCode exists in the Items table
        public async Task<bool> IsExistItemCode(string itemCode)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.Items.AnyAsync(l => l.itemCode == itemCode && l.fInAct == false);
        }

        // Checks if the given itemtypeKey exists in the CdMas table for the specified ourCode and ConCd "ItmTyp"
        public async Task<bool> IsExistItemType(string ourCode, short itemtypeKey)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.CdMas.AnyAsync(c =>
                c.ConCd == "ItmTyp" &&
                c.fInAct == false &&
                c.OurCd == ourCode &&
                c.CdKy == itemtypeKey
            );
        }

        // Checks if the given unitKey exists in the UnitCnv table
        public async Task<bool> IsValidUnitKey(short unitKey)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.UnitCnv.AnyAsync(c =>
                c.UnitKy == unitKey &&
                c.fInAct == false
            );
        }

        public async Task<bool> IsValidUserKey(int? enteredUserKey)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.Users.AnyAsync(u => u.UsrKy == enteredUserKey);
        }

        public async Task<bool> IsValidAccountTypeCode(string accountType)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.CdMas.AnyAsync(c =>
                c.ConCd == "AccTyp" &&
                c.fInAct == false &&
                c.Code == accountType
            );
        }

        public async Task<bool> IsValidAccountTypeKey(short accountTypeKey)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.CdMas.AnyAsync(c =>
                c.ConCd == "AccTyp" &&
                c.fInAct == false &&
                c.CdKy == accountTypeKey
            );
        }

        public async Task<bool> IsValidOrderTypeKey(string ConCd, short OrderTypeKey)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.CdMas.AnyAsync(o =>
                o.ConCd == ConCd &&
                o.CdKy == OrderTypeKey &&
                o.fInAct == false
            );
        }

        public async Task<bool> IsValidOrderType(string OrderType)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.CdMas.AnyAsync(o =>
                o.ConCd == "OrdTyp" &&
                o.Code == OrderType &&
                o.fInAct == false
            );
        }

        public async Task<bool> IsValidPaymentTermKey(short paymentTermKey)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.CdMas.AnyAsync(s =>
                s.ConCd == "PmtTrm" &&
                s.CdKy == paymentTermKey &&
                s.fInAct == false
            );
        }

        public async Task<bool> IsValidAddressKey(int addressKey)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.Stores.AnyAsync(s =>
                s.AdrKy == addressKey &&
                s.fInAct == false
            );
        }

        public async Task<bool> isAddressCodeExist(string addressCode)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.Stores.AnyAsync(s =>
                s.addressCode == addressCode &&
                s.fInAct == false
            );
        }

        public async Task<bool> IsValidPaymentMode(string paymentMode)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.CdMas.AnyAsync(s =>
                s.ConCd == "PmtMode" &&
                s.OurCd == paymentMode &&
                s.fInAct == false
            );
        }

        public async Task<bool> IsValidAccountKey(int accountKey)
        {
            using var dynamicContext = CreateDynamicContext();
            return await dynamicContext.Accounts.AnyAsync(a =>
                a.AccKy == accountKey &&
                a.fInAct == false
            );
        }
    }
}
