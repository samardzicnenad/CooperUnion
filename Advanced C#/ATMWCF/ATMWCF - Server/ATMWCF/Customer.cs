/**********************************************************************
 * Created by : Nenad Samardzic
 * Date       : 07/20/2013
 * Description: The class maintains ATM customer details (login name and password, first name, last name, account number, creation date)
 * Idea       : This class performs following activities:
 *              - check if the customer with the selected <name> already exists in the system
 *              - creates a new customer with the provided details
 *              - returns customer's account number
 * Parameters : -
 **********************************************************************/
using System;
using System.IO;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace ATMWCF
{
    public class Customer
    {
        static readonly string dbServer = ConfigurationManager.AppSettings["dbserver"];
        static readonly string dbName = ConfigurationManager.AppSettings["database"];
        static readonly string dbUser = ConfigurationManager.AppSettings["username"];
        static readonly string dbPassword = ConfigurationManager.AppSettings["password"];

        //Searches for the user name (and checks the password)
        public string Search(string sUser, string sPass)
        {
            MySqlConnection myConn = null;
            MySqlDataReader myReader = null;
            try
            {
                using (myConn = new MySqlConnection(
                    string.Format("Server={0};Database={1};Uid={2};Pwd={3};", dbServer, dbName, dbUser, dbPassword)))
                {
                    using (MySqlCommand myCmd = myConn.CreateCommand())
                    {
                        myConn.Open();
                        myCmd.Parameters.AddWithValue("@sUser", sUser);
                        myCmd.CommandText = @"SELECT * FROM customer WHERE user=@sUser";
                        myReader = myCmd.ExecuteReader();
                        if (!myReader.HasRows) //No such customer - if I want new - OK, else - negative
                        {
                            if (sPass.Equals(String.Empty))
                                return "1";
                            else
                                return "0";
                        }
                        if (sPass.Equals(String.Empty)) return "0"; //Found that customer, but didn't want him
                        else
                        {
                            myReader.Read();
                            string dbP = myReader.GetString(myReader.GetOrdinal("Pass")); //Compare passwords - 1==match, 0==no match
                            if (dbP.Equals(sPass)) return "1";
                            return "0";
                        }
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

        //creates a new customer record
        public string Create(string sUser, string sParamList)
        {
            MySqlConnection myConn = null;
            string[] parameters = sParamList.Split((';'));
            try
            {
                using (myConn = new MySqlConnection(
                    string.Format("Server={0};Database={1};Uid={2};Pwd={3};", dbServer, dbName, dbUser, dbPassword)))
                {
                    using (MySqlCommand myCmd = myConn.CreateCommand())
                    {
                        myConn.Open();
                        myCmd.Parameters.AddWithValue("@sUser", sUser);
                        myCmd.Parameters.AddWithValue("@sPass", parameters[0]);
                        myCmd.Parameters.AddWithValue("@sFirstName", parameters[1]);
                        myCmd.Parameters.AddWithValue("@sLastName", parameters[2]);
                        myCmd.Parameters.AddWithValue("@sAccount", parameters[3]);
                        myCmd.Parameters.AddWithValue("@sCDate", DateTime.Now.ToString("MM/d/yyyy"));
                        myCmd.CommandText = @"INSERT INTO customer(user, pass, firstName, lastName, account, cDate) VALUES(@sUser, @sPass, @sFirstName, @sLastName, @sAccount, @sCDate)";
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

        //gets customer's account number
        public string GetAccount(string sUser)
        {
            MySqlConnection myConn = null;
            MySqlDataReader myReader = null;
            try
            {
                using (myConn = new MySqlConnection(
                    string.Format("Server={0};Database={1};Uid={2};Pwd={3};", dbServer, dbName, dbUser, dbPassword)))
                {
                    using (MySqlCommand myCmd = myConn.CreateCommand())
                    {
                        myConn.Open();
                        myCmd.Parameters.AddWithValue("@sUser", sUser);
                        myCmd.CommandText = @"SELECT * FROM customer WHERE user=@sUser";
                        myReader = myCmd.ExecuteReader();
                        myReader.Read();
                        return myReader.GetString(myReader.GetOrdinal("account"));
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
