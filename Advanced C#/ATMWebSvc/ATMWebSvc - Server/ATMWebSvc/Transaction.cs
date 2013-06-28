/**********************************************************************
 * Created by : Nenad Samardzic
 * Date       : 05/30/2013
 * Description: The class maintains ATM transaction details (transaction ID, time stamp, account, action)
 * Idea       : This class performs following activities:
 *              - for the user's account creates a transaction data
 * Parameters : -
 **********************************************************************/
using System;
using System.IO;

namespace nsATMWebSvc
{
    public class Transaction
    {
        //creates a new transaction record
        public string Create(string sType, string sAmmount, string sAccount, string sActID)
        {
            try
            {
                string sTransID = Guid.NewGuid().ToString();
                string sTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                using (StreamWriter sw = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TRANSACTION.csv"))
                {
                    sw.WriteLine(sTransID + "," + sTimeStamp + "," + sAccount + "," + sActID + "\n");
                    sw.Close();
                }
                return "1";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
