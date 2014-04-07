using System;
using System.Linq;
using System.Web.Mvc;
using ATMMVC.Models;
using System.Web.Security;
using System.Web.Routing;

namespace ATMMVC.Controllers
{
    [HandleError]
    public class CustomerController : Controller
    {
        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new CustomerMembershipService(); }

            base.Initialize(requestContext);
        }

        // **************************************
        // URL: /Customer/ChangePassword
        // **************************************
        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewData["PasswordLength"] = MembershipService.MinPasswordLength;
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewData["PasswordLength"] = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Customer/ChangePasswordSuccess
        // **************************************

        //This is what determines whether it is going to allow access with(out) logging on.
        [Authorize]
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        // **************************************
        // URL: /Customer/LogOn
        // **************************************
        public ActionResult LogOn()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(model.UserName, model.Password))
                {
                    FormsService.SignIn(model.UserName, false);

                    var accountNumber = "";
                    var oldUser = MembershipService.GetUser(model.UserName, false);
                    var userID = oldUser.ProviderUserKey.ToString();
                    using (ATMEntities db = new ATMEntities())
                    {
                        var customer = from c in db.customers
                                    where c.idCustomer == userID
                                    select c;
                        accountNumber = customer.Single().account;
                    }
                    Session.Add("accountNumber", accountNumber);

                    if (!String.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }
            return View(model);
        }

        // **************************************
        // URL: /Customer/LogOff
        // **************************************
        public ActionResult LogOff()
        {
            FormsService.SignOut();
            Session.Add("accountNumber", null);
            return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Customer/Register
        // **************************************
        public ActionResult Register()
        {
            ViewData["PasswordLength"] = MembershipService.MinPasswordLength;
            return View();
        }
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email);
                if (createStatus == MembershipCreateStatus.Success)
                { // On success create a new customer data
                    MembershipUser newUser = MembershipService.GetUser(model.UserName, false);
                    string newUserID = newUser.ProviderUserKey.ToString();
                    string dtStamp = DateTime.Now.ToString();
                    string accountNo = Guid.NewGuid().ToString();
                    var dbATM = new ATMEntities();
                    var customer = new customer
                    {
                        idCustomer = newUserID,
                        firstName = model.FirstName,
                        lastName = model.LastName,
                        account = accountNo,
                        cDate = dtStamp
                    };
                    dbATM.customers.AddObject(customer);
                    dbATM.SaveChanges();
                    FormsService.SignIn(model.UserName, false);
                    Session.Add("accountNumber", accountNo);
                    TempData["justRegistered"] = "Y";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", CustomerValidation.ErrorCodeToString(createStatus));
                }
            }
            ViewData["PasswordLength"] = MembershipService.MinPasswordLength;
            return View(model);
        }
    }
}