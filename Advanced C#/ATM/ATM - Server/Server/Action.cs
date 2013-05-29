using System;
using System.IO;

namespace Server
{
    public class Action
    {
        //creates a new action record
        public string Create(string sType, string sAmmount) {
            try
            {
                string sActID = Guid.NewGuid().ToString();
                using (StreamWriter sw = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ACTION.csv"))
                {
                    sw.WriteLine(sActID + "," + sType.ToUpper()[0] + "," + sAmmount + "\n");
                    sw.Close();
                }
                return sActID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
