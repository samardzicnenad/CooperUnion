using System;
using System.IO;

namespace Server
{
    public class User
    {
        //Searches for the user name in cases of new user check and log in
        public string Search(string sUser, string sPass)
        {
            try
            {
                if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\USERPASS.csv"))
                {
                    if (sPass.Equals(String.Empty))
                        return "1";
                    else
                        return "0";
                }
                string contents = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\USERPASS.csv");
                string[] rows = contents.Replace("\r\n", "").Split(('\n'));
                foreach (string row in rows)
                {
                    if (row.Equals("")) continue;
                    string[] els = row.Split((','));
                    if (sPass.Equals(String.Empty))
                    {
                        if (els[0].Equals(sUser)) return "0";
                    }
                    else
                    {
                        if (els[0].Equals(sUser) && !els[1].Equals(sPass)) return "0";
                        if (els[0].Equals(sUser) && els[1].Equals(sPass)) return "1";
                    }
                }
                if (sPass.Equals(String.Empty))
                    return "1";
                else return "0";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Creates a new user
        public string Create(string sUser, string sPass)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\USERPASS.csv"))
                {
                    sw.WriteLine(sUser + "," + sPass + "\n");
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
