﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Transactions;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Services
{
    public class CashTransactionData
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int PaymentType { get; set; }
        public int TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string AccountName { get; set; }
    }

    public class CashService
    {
        public dynamic GetCurrentCashOperationData()
        {
            if (AppServices.MainDataContext.CurrentWorkPeriod == null)
                return new[] { 0m, 0m, 0m };

            var startDate = AppServices.MainDataContext.CurrentWorkPeriod.StartDate;

            var cashAmount = Dao.Sum<Payment>(x => x.Amount,
                                                 x =>
                                                 x.PaymentType == (int)PaymentType.Cash &&
                                                 x.Date > startDate);

            var creditCardAmount = Dao.Sum<Payment>(x => x.Amount,
                                                 x =>
                                                 x.PaymentType == (int)PaymentType.CreditCard &&
                                                 x.Date > startDate);

            var ticketAmount = Dao.Sum<Payment>(x => x.Amount,
                                                 x =>
                                                 x.PaymentType == (int)PaymentType.Ticket &&
                                                 x.Date > startDate);

            return new[] { cashAmount, creditCardAmount, ticketAmount };
        }

        public void AddIncome(int accountId, decimal amount, string description, PaymentType paymentType)
        {
            AddTransaction(accountId, amount, description, paymentType, TransactionType.Income);
        }

        public void AddExpense(int accountId, decimal amount, string description, PaymentType paymentType)
        {
            AddTransaction(accountId, amount, description, paymentType, TransactionType.Expense);
        }

        public void AddLiability(int accountId, decimal amount, string description)
        {
            AddTransaction(accountId, amount, description, 0, TransactionType.Liability);
        }

        public void AddReceivable(int accountId, decimal amount, string description)
        {
            AddTransaction(accountId, amount, description, 0, TransactionType.Receivable);
        }

        public IEnumerable<CashTransaction> GetTransactions(WorkPeriod workPeriod)
        {
            Debug.Assert(workPeriod != null);
            if (workPeriod.StartDate == workPeriod.EndDate)
                return Dao.Query<CashTransaction>(x => x.Date >= workPeriod.StartDate);
            return Dao.Query<CashTransaction>(x => x.Date >= workPeriod.StartDate && x.Date < workPeriod.EndDate);
        }

        public IEnumerable<CashTransactionData> GetTransactionsWithAccountData(WorkPeriod workPeriod)
        {
            var wp = new WorkPeriod { StartDate = workPeriod.StartDate, EndDate = workPeriod.EndDate };
            if (wp.StartDate == wp.EndDate) wp.EndDate = DateTime.Now;
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                var lines = from ct in workspace.Queryable<CashTransaction>()
                            join account in workspace.Queryable<Account>() on ct.AccountId equals account.Id into ctC
                            from account in ctC.DefaultIfEmpty()
                            where ct.Date >= wp.StartDate && ct.Date < wp.EndDate
                            select new { CashTransaction = ct, Account = account };

                return lines.ToList().Select(x => new CashTransactionData
                {
                    Amount = x.CashTransaction.Amount,
                    AccountName = x.Account != null ? x.Account.Name : Resources.UndefinedWithBrackets,
                    Date = x.CashTransaction.Date,
                    Name = x.CashTransaction.Name,
                    PaymentType = x.CashTransaction.PaymentType,
                    TransactionType = x.CashTransaction.TransactionType
                });
            }
        }

        private static void AddTransaction(int accountId, decimal amount, string description, PaymentType paymentType, TransactionType transactionType)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                if (transactionType == TransactionType.Income || transactionType == TransactionType.Expense)
                {
                    var c = new CashTransaction
                    {
                        Amount = amount,
                        Date = DateTime.Now,
                        Name = description,
                        PaymentType = (int)paymentType,
                        TransactionType = (int)transactionType,
                        UserId = AppServices.CurrentLoggedInUser.Id,
                        AccountId = accountId
                    };
                    workspace.Add(c);
                }
                else
                {
                    var c = new AccountTransaction
                    {
                        Amount = amount,
                        Date = DateTime.Now,
                        Name = description,
                        TransactionType = (int)transactionType,
                        UserId = AppServices.CurrentLoggedInUser.Id,
                        AccountId = accountId
                    };
                    workspace.Add(c);
                }

                workspace.CommitChanges();
            }
        }
    }


}
