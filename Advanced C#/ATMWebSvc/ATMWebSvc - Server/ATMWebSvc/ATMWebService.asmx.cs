/**********************************************************************
 * Created by : Nenad Samardzic
 * Date       : 05/30/2013
 * Description: The class represents ATM simulation using .NET 3.5 web service.
 * Idea       : ATM Server side application serves client's ATM requests.
 *              "Database" for the application is set of flat files. All of the "connected actions" are performed as one transaction - 
 *              complete success or rollback.
 *              This ATM model performs following activities:
 *              - check if the user with the selected <user name> already exists in the system
 *              - creates new user with the selected <user name>
 *              - for the new user creates a customer and an account data
 *              - performs deposit and withdrawal activities
 *              - checks for the user's balance
 * Parameters : -
 **********************************************************************/
using System;
using System.Web.Services;
using System.Transactions;
using System.IO;

namespace nsATMWebSvc
{
    /// Summary description for Service1
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ATMSvc : System.Web.Services.WebService
    {
        private User myUser = new User();
        private Action myAction = new Action();
        private Account myAccount = new Account();
        private Customer myCustomer = new Customer();
        private Transaction myTransaction = new Transaction();

        [WebMethod]
        //Process incoming message and return result
        public string Process(string sMessage)
        {
            try
            {
                string[] els = sMessage.Split((','));
                switch (els[0])
                {
                    case "SEARCH":
                        return myUser.Search(els[1], els[2]);
                    case "CREATE":
                        return myUser.Create(els[1], els[2]);
                    case "CREATECUSTACC":
                        return tsCustAcc(els[1], els[2], els[3]);
                    case "DEPOSIT":
                    case "WITHDRAW":
                        return DepWith(els[0], els[1], els[2]);
                    case "BALANCE":
                        return Balance(els[1], els[2]);
                    default:
                        return "1";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "-1";
            }
        }

        //Transaction scope for creating customer and his/her account
        private string tsCustAcc(string sUser, string sFullName, string sAccountNo)
        {
            try
            {
                using (TransactionScope TranScope = new TransactionScope())
                {
                    myCustomer.Create(sUser, sFullName, sAccountNo);
                    string sRes = myAccount.Create(sAccountNo);
                    TranScope.Complete();
                    return sRes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "-1";
            }
        }

        //Makes a deposit or withdrawal
        private string DepWith(string sType, string sUser, string sAmmount)
        {
            try
            {
                using (TransactionScope TranScope = new TransactionScope())
                {
                    string sAccount = myCustomer.GetAccount(sUser);
                    string sOutput = myAccount.CalculateBalance(sType, sAccount, sAmmount);
                    if (sOutput == "0") return "0";
                    string sActID = myAction.Create(sType, sAmmount);
                    myTransaction.Create(sType, sAmmount, sAccount, sActID);
                    return myAccount.Update(sOutput);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "-1";
            }
        }

        //returns customer's balance
        private string Balance(string sUser, string sType)
        {
            string contents, sOutput = "", sTransType = "", sAmmount = "", sTimeStamp = "";
            string[] rows, actions;

            try
            {
                string sAccountB = myCustomer.GetAccount(sUser);
                Tuple<string, string> myTuple = myAccount.GetBalance(sAccountB);

                if (sType.Equals("N"))
                    return "Current balance on your cash account is: " + myTuple.Item1;
                string sTrans = myTuple.Item2;

                contents = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TRANSACTION.csv");
                rows = contents.Replace("\r\n", "").Split(('\n'));
                contents = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ACTION.csv");
                actions = contents.Replace("\r\n", "").Split(('\n'));

                string[] trans = sTrans.Split((';'));
                foreach (string tran in trans)
                {
                    if (tran.Equals("")) continue;
                    foreach (string row in rows)
                    {
                        if (row.Equals("")) continue;
                        string[] els = row.Split((','));
                        if (els[0].Equals(tran))
                        {
                            sTimeStamp = els[1];
                            string sActID = els[3];
                            foreach (string action in actions)
                            {
                                if (action.Equals("")) continue;
                                string[] acts = action.Split((','));
                                if (acts[0].Equals(sActID))
                                {
                                    if (acts[1].Equals("D"))
                                        sTransType = "DEPOSIT   ";
                                    else
                                        sTransType = "WITHDRAWAL";
                                    sAmmount = acts[2];
                                }
                            }
                        }
                    }
                    sOutput = sOutput + "\nTransaction date: " + sTimeStamp.Substring(4, 2) + "/" + sTimeStamp.Substring(6, 2) + "/" + sTimeStamp.Substring(0, 4)
                        + "; Transaction type: " + sTransType + "; Ammount: " + sAmmount;
                }
                return sOutput;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "-1";
            }
        }
    }
}