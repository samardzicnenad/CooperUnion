using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ATMMVC.Models;
using System.Web.Routing;

namespace ATMMVC.Controllers
{
    [HandleError]
    public class TransactionController : Controller
    {
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (MembershipService == null) { MembershipService = new CustomerMembershipService(); }
            base.Initialize(requestContext);
        }

        // **************************************
        // URL: /Transaction/Deposit
        // **************************************
        public ActionResult Deposit()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Deposit(TransactionModel model)
        {
            if (ModelState.IsValid && model.credit > 0)
            {
                string dtStamp = DateTime.Now.ToString();
                var dbATM = new ATMEntities();
                var transaction = new transaction
                {
                    tStamp = dtStamp,
                    credit = model.credit,
                    account = model.account,
                    debit = 0
                };
                dbATM.transactions.AddObject(transaction);
                dbATM.SaveChanges();
                TempData["successD"] = "Y";
            }
            else
            {
                ModelState.AddModelError("", "The deposit value provided is incorrect.");
            }
            return View(model);
        }

        // **************************************
        // URL: /Transaction/Withdraw
        // **************************************
        public ActionResult Withdraw()
        {
            return View();
        }

        private double getAccountBalance(string account)
        {
            var balance = 0.0;
            using (ATMEntities db = new ATMEntities())
            {
                var trans = from t in db.transactions
                            where t.account == account
                            select t;
                foreach (transaction tran in trans)
                {
                    balance += (tran.credit - tran.debit);
                }
            }
            return balance;
        }

        [HttpPost]
        public ActionResult Withdraw(TransactionModel model)
        {
            if (ModelState.IsValid && model.debit > 0)
            {
                string dtStamp = DateTime.Now.ToString();
                var balance = getAccountBalance(model.account);
                if (balance <= model.debit)
                {
                    TempData["successW"] = "N";
                }
                else
                {
                    var dbATM = new ATMEntities();
                    var transaction = new transaction
                    {
                        tStamp = dtStamp,
                        debit = model.debit,
                        account = model.account,
                        credit = 0
                    };
                    dbATM.transactions.AddObject(transaction);
                    dbATM.SaveChanges();
                    TempData["successW"] = "Y";
                }
                return View(model);
            }
            else
            {
                ModelState.AddModelError("", "The withdrawal value provided is incorrect.");
            }
            return View(model);
        }

        // **************************************
        // URL: /Transaction/Balance
        // **************************************
        public ActionResult Balance(string userName = "")
        {
            if (userName != "")
            {
                var accountNumber = "";
                var oldUser = MembershipService.GetUser(userName, false);
                var userID = oldUser.ProviderUserKey.ToString();
                List<TransactionModel> trans = new List<TransactionModel> { };
                using (ATMEntities db = new ATMEntities())
                {
                    var customer = from c in db.customers
                                   where c.idCustomer == userID
                                   select c;
                    accountNumber = customer.Single().account;

                    var dbtrans = from t in db.transactions
                                  where t.account == accountNumber
                                  select t;
                    var balance = getAccountBalance(accountNumber);

                    foreach (transaction dbtran in dbtrans)
                    {
                        TransactionModel tempInstance = new TransactionModel();
                        tempInstance.account = dbtran.account;
                        tempInstance.debit = Convert.ToSingle(dbtran.debit);
                        tempInstance.credit = Convert.ToSingle(dbtran.credit);
                        tempInstance.tStamp = dbtran.tStamp;
                        trans.Add(tempInstance);
                    }

                    Session.Add("accountNumber", accountNumber);
                    TempData["balance"] = balance;

                    return View(trans);
                }
            }
            else
            {
                return View();
            }
        }
    }
}
