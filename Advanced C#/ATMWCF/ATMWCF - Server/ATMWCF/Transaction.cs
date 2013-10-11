/**********************************************************************
 * Created by : Nenad Samardzic
 * Date       : 07/20/2013
 * Description: The class maintains ATM transaction details (transaction ID, time stamp, account, action)
 * Idea       : This class performs following activities:
 *              - for the user's account creates a transaction data (withdrawal - if possible)
 *              - returns a balance (and a turnover) for the customer's account
 * Parameters : -
 **********************************************************************/
using System;
using System.IO;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace ATMWCF
{
    public class Transaction
    {
        static readonly string dbServer = ConfigurationManager.AppSettings["dbserver"];
        static readonly string dbName = ConfigurationManager.AppSettings["database"];
        static readonly string dbUser = ConfigurationManager.AppSettings["username"];
        static readonly string dbPassword = ConfigurationManager.AppSettings["password"];

        //manages a deposit or withdrawal
        public string DepWith(string sType, string sUser, string sAmmount)
        {
            Customer myCustomer = new Customer();
            try
            {
                string sAccount = myCustomer.GetAccount(sUser);
                if (sType.Equals("WITHDRAW"))
                    if (GetBalance(sAccount) < double.Parse(sAmmount)) return "0";
                return Create(sType, sAmmount, sAccount);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "-1";
            }
        }

        //creates a new transaction record
        private string Create(string sType, string sAmmount, string sAccount)
        {
            MySqlConnection myConn = null;
            try
            {
                string sTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                using (myConn = new MySqlConnection(
                    string.Format("Server={0};Database={1};Uid={2};Pwd={3};", dbServer, dbName, dbUser, dbPassword)))
                {
                    using (MySqlCommand myCmd = myConn.CreateCommand())
                    {
                        myConn.Open();
                        myCmd.Parameters.AddWithValue("@sTimeStamp", sTimeStamp);
                        myCmd.Parameters.AddWithValue("@sAccount", sAccount);
                        if (sType.Equals("WITHDRAW"))
                        {
                            myCmd.Parameters.AddWithValue("@dDebit", double.Parse(sAmmount));
                            myCmd.Parameters.AddWithValue("@dCredit", 0);
                        }
                        else
                        {
                            myCmd.Parameters.AddWithValue("@dDebit", 0);
                            myCmd.Parameters.AddWithValue("@dCredit", double.Parse(sAmmount));
                        }
                        myCmd.CommandText = @"INSERT INTO transaction(tStamp, account, debit, credit) VALUES(@sTimeStamp, @sAccount, @dDebit, @dCredit)";
                        myCmd.ExecuteNonQuery();
                        myConn.Close();
                    }
                }
                return "1";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (myConn != null)
                {
                    try
                    {
                        myConn.Close();
                    }
                    catch (Exception ex) { /*ignore*/ }
                }
            }
        }

        //returns customer's balance (and customer account's turnover)
        public string Balance(string sUser, string sMode)
        {
            Customer myCustomer = new Customer();

            try
            {
                string sAccount = myCustomer.GetAccount(sUser);

                if (sMode.Equals("N"))
                    return "Current balance on your cash account is: " + GetBalance(sAccount).ToString();

                return "Current balance on your cash account is: " + GetBalance(sAccount).ToString() + "\n" + GetTurnover(sAccount);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "-1";
            }
        }

        //returns a balance for the account
        private double GetBalance(string sAccount)
        {
            MySqlConnection myConn = null;
            MySqlDataReader myReader = null;
            double dBalance;

            try
            {
                using (myConn = new MySqlConnection(
                    string.Format("Server={0};Database={1};Uid={2};Pwd={3};", dbServer, dbName, dbUser, dbPassword)))
                {
                    using (MySqlCommand myCmd = myConn.CreateCommand())
                    {
                        myConn.Open();
                        myCmd.Parameters.AddWithValue("@sAccount", sAccount);
                        myCmd.CommandText = @"SELECT SUM(credit)-SUM(debit) FROM transaction WHERE account=@sAccount";
                        myReader = myCmd.ExecuteReader();
                        myReader.Read();

                        if (Double.TryParse(myReader[0].ToString(), out dBalance)) return dBalance;
                        else return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (myReader != null)
                {
                    try
                    {
                        myReader.Close();
                    }
                    catch (Exception ex) { /*ignore*/ }
                }
                if (myConn != null)
                {
                    try
                    {
                        myConn.Close();
                    }
                    catch (Exception ex) { /*ignore*/ }
                }
            }
        }

        //returns an account's turnover
        private string GetTurnover(string sAccount)
        {
            MySqlConnection myConn = null;
            MySqlDataReader myReader = null;

            string sOutput = "", sTransType = "";
            double dAmmount = 0;

            try
            {
                using (myConn = new MySqlConnection(
                    string.Format("Server={0};Database={1};Uid={2};Pwd={3};", dbServer, dbName, dbUser, dbPassword)))
                {
                    using (MySqlCommand myCmd = myConn.CreateCommand())
                    {
                        myConn.Open();
                        myCmd.Parameters.AddWithValue("@sAccount", sAccount);
                        myCmd.CommandText = @"SELECT * FROM transaction WHERE account=@sAccount";
                        myReader = myCmd.ExecuteReader();
                        while (myReader.Read())
                        {
                            if (double.Parse(myReader["debit"].ToString()) != 0)
                            {
                                sTransType = "W";
                                dAmmount = double.Parse(myReader["debit"].ToString());
                            }
                            else
                            {
                                sTransType = "D";
                                dAmmount = double.Parse(myReader["credit"].ToString());
                            }
                            sOutput = sOutput + "\nTransaction date: " + myReader["tStamp"].ToString().Substring(4, 2) + "/" +
                                myReader["tStamp"].ToString().Substring(6, 2) + "/" +
                                myReader["tStamp"].ToString().Substring(0, 4) + "; Transaction type: " + sTransType + "; Ammount: " + dAmmount;
                        }
                        return sOutput;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (myReader != null)
                {
                    try
                    {
                        myReader.Close();
                    }
                    catch (Exception ex) { /*ignore*/ }
                }
                if (myConn != null)
                {
                    try
                    {
                        myConn.Close();
                    }
                    catch (Exception ex) { /*ignore*/ }
                }
            }
        }
    }
}
