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
                //log.Debug(ex.ToString());
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

/* ROB'S CODE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;

namespace AtmWebSvc
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : System.Web.Services.WebService
    {
        private static string credentialsFileName = @"C:\credentials.csv";
        private static string sessionsFileName = @"C:\sessions.csv";

        private static FileInfo credentialsFile;
        private static FileInfo sessionsFile;

        public Service1()
        {
            try
            {
                credentialsFile = new FileInfo(credentialsFileName);
                sessionsFile = new FileInfo(sessionsFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
           
        [WebMethod]
        public string Register(string uid, string pwd)
        {
            using (StreamWriter sw = credentialsFile.AppendText())
            {
                sw.WriteLine(uid + "," + pwd);
                sw.Close();
            }
            return("OK");
        }

        [WebMethod]
        public string Login(string uid, string pwd)
        {
            Dictionary<string, string> dic = loadCredentials();
            if (dic.ContainsKey(uid))
            {
                if (pwd.Equals(dic[uid]))
                {
                    return ((Guid.NewGuid()).ToString());
                }
            }
            return ("REJECTED");
        }

        private Dictionary<string, string> loadCredentials()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            using (StreamReader sr = new StreamReader(credentialsFile.OpenRead()))
            {
                string line = (string)null;

                while ((line = sr.ReadLine()) != (string)null)
                {
                    string[] lineVals = line.Split(',');
                    dic.Add(lineVals[0], lineVals[1]);
                }
            }
            return (dic);
        }
    }
}
*/