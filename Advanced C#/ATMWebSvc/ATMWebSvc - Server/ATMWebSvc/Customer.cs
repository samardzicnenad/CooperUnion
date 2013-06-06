using System;
using System.IO;

namespace nsATMWebSvc
{
    public class Customer
    {
        //creates a new customer record
        public string Create(string sUser, string sFullName, string sAccountNo)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CUSTOMER.csv"))
                {
                    sw.WriteLine(sUser + "," + sFullName + "," + sAccountNo + "\n");
                    sw.Close();
                }
                return "1";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //gets selected customer's account
        public string GetAccount(string sUser)
        {
            try
            {
                string contents, sAccount = "";
                string[] rows;

                contents = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CUSTOMER.csv");
                rows = contents.Replace("\r\n", "").Split(('\n'));
                foreach (string row in rows)
                {
                    if (row.Equals("")) continue;
                    string[] els = row.Split((','));
                    if (els[0].Equals(sUser))
                        sAccount = els[2];
                }
                return sAccount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
