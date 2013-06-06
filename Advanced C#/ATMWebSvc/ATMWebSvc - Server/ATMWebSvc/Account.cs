using System;
using System.IO;

namespace nsATMWebSvc
{
    public class Account
    {
        //creates a new account record
        public string Create(string sAccountNo)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ACCOUNT.csv"))
                {
                    sw.WriteLine(sAccountNo + ",0.00," + DateTime.Now.ToString("MM/d/yyyy") + ",-\n");
                    sw.Close();
                }
                return "1";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //updates an account record
        public string Update(string sOutput)
        {
            try
            {
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ACCOUNT.csv", String.Empty);
                using (StreamWriter sw = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ACCOUNT.csv"))
                {
                    sw.WriteLine(sOutput);
                    sw.Close();
                }
                return "1";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //calculates a new balance for the account depending on the type of transaction
        public string CalculateBalance(string sType, string sAccount, string sAmmount)
        {
            string contents, sNewBalance, sOutput = "";
            string sTransID = Guid.NewGuid().ToString();
            string[] rows;

            try
            {
                contents = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ACCOUNT.csv");
                rows = contents.Replace("\r\n", "").Split(('\n'));
                foreach (string row in rows)
                {
                    if (row.Equals("")) continue;
                    string[] els = row.Split((','));
                    if (els[0].Equals(sAccount))
                    {
                        if (sType.Equals("DEPOSIT"))
                            sNewBalance = (double.Parse(els[1]) + double.Parse(sAmmount)).ToString();
                        else
                        {
                            if (double.Parse(els[1]) < double.Parse(sAmmount))
                                return "0";
                            else
                                sNewBalance = (double.Parse(els[1]) - double.Parse(sAmmount)).ToString();
                        }
                        if (els[3].Equals("-"))
                            sOutput = sOutput + sAccount + "," + sNewBalance + "," + els[2] + "," + sTransID + "\n";
                        else
                            sOutput = sOutput + sAccount + "," + sNewBalance + "," + els[2] + "," + els[3] + ";" + sTransID + "\n";
                    }
                    else
                        sOutput = sOutput + row + "\n";
                }
                return sOutput;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //gets balance and transactions for the account
        public Tuple<string, string> GetBalance(string sAccount)
        {
            string contents, sBalance = "", sTrans = "";
            string[] rows;

            try
            {
                contents = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ACCOUNT.csv");
                rows = contents.Replace("\r\n", "").Split(('\n'));
                foreach (string row in rows)
                {
                    if (row.Equals("")) continue;
                    string[] els = row.Split((','));
                    if (els[0].Equals(sAccount))
                    {
                        sBalance = els[1];
                        sTrans = els[3];
                    }
                }
                return new Tuple<string, string>(sBalance, sTrans);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new Tuple<string, string>("-1", "-1");
            }
        }
    }
}
