/**********************************************************************
 * Created by : Nenad Samardzic
 * Date       : 07/20/2013
 * Description: The class represents ATM simulation using .NET 4.0 WCF and RDBMS.
 * Idea       : ATM Server side application serves client's ATM requests.
 *              Database for the application is mySQL.
 *              This ATM model performs following activities:
 *              - check if the customer with the selected <user name> already exists in the system
 *              - creates new customer with the provided <user name> and an account data
 *              - performs deposit and withdrawal activities
 *              - checks for the customer's balance
 * Parameters : -
 **********************************************************************/
using System;

namespace ATMWCF
{
    public class ATMWCFSvc : IATMWCFSvc
    {
        private Customer myCustomer = new Customer();
        private Transaction myTransaction = new Transaction();

        //processes incoming message
        public string Process(string sMessage)
        {
            try
            {
                string[] els = sMessage.Split((','));
                switch (els[0])
                {
                    case "SEARCH":
                        return myCustomer.Search(els[1], els[2]);
                    case "CREATE":
                        return myCustomer.Create(els[1], els[2]);
                    case "DEPOSIT":
                    case "WITHDRAW":
                        return myTransaction.DepWith(els[0], els[1], els[2]);
                    case "BALANCE":
                        return myTransaction.Balance(els[1], els[2]);
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
    }
}
