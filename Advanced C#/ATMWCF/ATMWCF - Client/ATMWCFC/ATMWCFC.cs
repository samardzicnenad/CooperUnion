/**********************************************************************
 * Created by : Nenad Samardzic
 * Date       : 07/20/2013
 * Description: The class represents client ATM simulation.
 * Idea       : ATM Client side application runs in two phases:
 *              - greeting phase
 *                  - log in for an existing customer with the provided <user name>
 *                  - create new customer with the provided <user name>
 *              - transaction phase
 *                  - make deposit or withdrawal
 *                  - check the user's balance
 *              It also manages login/logout with the server and perform some helper functions.
 * Parameters : -
 **********************************************************************/
using System;
using ATMWCFC.ATMWCFRef;

namespace nsATMWCFC
{
    class ATMWCFC
    {
        private ATMWCFSvcClient ATMWFCRef = new ATMWCFSvcClient();
        private string sUser, sPassword;
        private Boolean bLoggedIn;

        //class constructor
        public ATMWCFC()
        {
            this.sUser = "";
            this.sPassword = "";
            this.bLoggedIn = false;
        }

        //Response to selected action
        private void GreetingPhase()
        {
            switch (Greeting())
            {
                case 0: //exit
                    Exit();
                    break;
                case 1: //create an account
                    CreateAccount();
                    WaitTime("Log in with your new credentials!");
                    ReLogIn();
                    break;
                case 2: //log in to an account
                    ReLogIn();
                    break;
                default:
                    break;
            }
        }

        //Initial selection of action - returns 0, 1 or 2
        private int Greeting()
        {
            Boolean bFirsPass = true;
            char cChoice = '\0';
            GScreen("Welcome to our ATM. Would you like to:");
            do
            {
                if (!bFirsPass)
                {
                    if (!(cChoice.Equals('\r') || cChoice.Equals('\n') || cChoice.Equals('\0')))
                    {
                        GScreen("Invalid selection!");
                    }
                }
                cChoice = Console.ReadKey(true).KeyChar;
                bFirsPass = false;
            } while (cChoice != '0' && cChoice != '1' && cChoice != '2');
            return (int)Char.GetNumericValue(cChoice);
        }

        //Greeting dialog
        private void GScreen(string sMessage)
        {
            Console.Clear();
            Console.WriteLine(sMessage);
            Console.WriteLine();
            Console.WriteLine("<0. Exit>");
            Console.WriteLine("1. Create new account");
            Console.WriteLine("2. Log in to your account");
            Console.WriteLine();
            Console.WriteLine("Please, enter your choice (1/2)");
            Console.WriteLine();
            Console.Write(">");
        }

        //Responses to selected transactions
        private void TransactionsPhase()
        {
            Console.Clear();
            switch (Transactions())
            {
                case 0:
                    Exit();
                    break;
                case 1:
                    if (DepWith("deposit").Equals("1"))
                        WaitTime("You have successfully deposited your cash!");
                    break;
                case 2:
                    if (DepWith("withdraw").Equals("1"))
                        WaitTime("You have successfully withdrew your cash!");
                    else
                        WaitTime("You didn't have enough money on your account to withdraw!");
                    break;
                case 3:
                    WaitTime(Balance());
                    System.Threading.Thread.Sleep(7500);
                    break;
                default:
                    break;
            }
        }

        //Choose transaction; returns 0, 1, 2 or 3
        private int Transactions()
        {
            Boolean bFirsPass = true;
            char cChoice = '\0';
            TScreen("Please, choose a transaction you want to perform:");
            do
            {
                if (!bFirsPass)
                {
                    if (!(cChoice.Equals("\r") || cChoice.Equals("\n") || cChoice.Equals('\0')))
                    {
                        TScreen("Invalid selection!");
                    }
                }
                cChoice = Console.ReadKey(true).KeyChar;
                bFirsPass = false;
            } while (cChoice != '0' && cChoice != '1' && cChoice != '2' && cChoice != '3');
            return (int)Char.GetNumericValue(cChoice);
        }

        //Transaction dialog
        private void TScreen(string sMessage)
        {
            Console.Clear();
            Console.WriteLine(sMessage);
            Console.WriteLine();
            Console.WriteLine("<0. Exit>");
            Console.WriteLine("1. Make a deposit");
            Console.WriteLine("2. Make a withdrawal");
            Console.WriteLine("3. Check your balance");
            Console.WriteLine();
            Console.WriteLine("Please, enter your choice (1/2/3)");
            Console.WriteLine();
            Console.Write(">");
        }

        //Log in to an account
        private string LogIn()
        {
            try
            {
                do
                {
                    Console.Clear();
                    Console.WriteLine("Please, enter your username:");
                    sUser = Console.ReadLine();
                } while (sUser == "");
                do
                {
                    Console.Clear();
                    Console.WriteLine("Please, enter your password:");
                    sPassword = "";
                    ConsoleKeyInfo key;
                    do
                    {
                        key = Console.ReadKey(true);
                        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                        {
                            sPassword += key.KeyChar;
                            Console.Write("*");
                        }
                        else
                        {
                            if (key.Key == ConsoleKey.Backspace && sPassword.Length > 0)
                            {
                                sPassword = sPassword.Substring(0, (sPassword.Length - 1));
                                Console.Write("\b \b");
                            }
                        }
                    }
                    while (key.Key != ConsoleKey.Enter);
                } while (sPassword == "");
                return ATMWFCRef.Process(fUniversal("SEARCH", sUser, sPassword));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Repeated logging in case of error
        private void ReLogIn()
        {
            char cChoice = '\0';
            Boolean bFirsPass;

            try
            {
                while (LogIn().Equals("0"))
                {
                    sUser = "";
                    sPassword = "";
                    bFirsPass = true;

                    Console.Clear();
                    Console.WriteLine("Your entry didn't match our data!");
                    Console.WriteLine("Would you like to try again? (Y/N)");
                    do
                    {
                        if (!bFirsPass)
                        {
                            if (!(cChoice.Equals("\r") || cChoice.Equals("\n") || cChoice.Equals('\0')))
                            {
                                Console.Clear();
                                Console.WriteLine("Incorrect choice! Please, enter Y or N!");
                            }
                        }
                        cChoice = Console.ReadKey(true).KeyChar;
                        bFirsPass = false;
                    } while (cChoice != 'Y' && cChoice != 'y' && cChoice != 'N' && cChoice != 'n');
                    if (cChoice == 'N' || cChoice == 'n')
                        Exit();
                    bLoggedIn = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        //Log out
        private void LogOut()
        {
            sUser = "";
            sPassword = "";
            bLoggedIn = false;
        }

        //Exits the ATM
        private void Exit()
        {
            if (bLoggedIn)
                LogOut();
            Console.Clear();
            Console.WriteLine("Thank you. Goodbye.");
            System.Threading.Thread.Sleep(3500);
            Environment.Exit(0);
        }

        /******Helper methods section******/
        //Helper function - check if entry is a number
        private Boolean CheckAmmount(string s)
        {
            double dNum;
            Boolean bCheckOK = double.TryParse(s, out dNum);
            return (bCheckOK && dNum > 0);
        }

        //Helper function - give message and wait for the key pressed
        private void WaitTime(string sMessage)
        {
            Console.Clear();
            Console.WriteLine(sMessage);
            System.Threading.Thread.Sleep(2500);
        }

        /******Program logic section******/
        //Creates an account
        private string CreateAccount()
        {
            Boolean bFirstPass = true;
            string sFName, sLName, sAccount;

            try
            {
                do
                {
                    do
                    {
                        Console.Clear();
                        if (!bFirstPass) Console.WriteLine("That user name is either already taken or not allowed.");
                        Console.WriteLine("Please, enter your username:");
                        sUser = Console.ReadLine();
                    } while (sUser == "");
                    bFirstPass = false;
                } while (ATMWFCRef.Process(fUniversal("SEARCH", sUser)).Equals("0"));
                do
                {
                    Console.Clear();
                    Console.WriteLine("Please, enter your password:");
                    sPassword = "";
                    ConsoleKeyInfo key;
                    do
                    {
                        key = Console.ReadKey(true);
                        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                        {
                            sPassword += key.KeyChar;
                            Console.Write("*");
                        }
                        else
                        {
                            if (key.Key == ConsoleKey.Backspace && sPassword.Length > 0)
                            {
                                sPassword = sPassword.Substring(0, (sPassword.Length - 1));
                                Console.Write("\b \b");
                            }
                        }
                    }
                    while (key.Key != ConsoleKey.Enter);
                } while (sPassword == "");
                do
                {
                    Console.Clear();
                    Console.WriteLine("Please, enter your first name:");
                    sFName = Console.ReadLine();
                } while (sFName == "");
                do
                {
                    Console.Clear();
                    Console.WriteLine("Please, enter your last name:");
                    sLName = Console.ReadLine();
                } while (sLName == "");
                sAccount = Guid.NewGuid().ToString();
                Console.Clear();
                Console.WriteLine("Your account number is: " + sAccount);
                System.Threading.Thread.Sleep(3500);

                return ATMWFCRef.Process(fUniversal("CREATE", sUser, sPassword + ";" + sFName + ";" + sLName + ";" + sAccount));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "-1";
            }
        }

        //Makes a deposit or a withdrawal
        private string DepWith(string sType)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("Enter the amount of cash you would like to " + sType);
                string sAmmount = Console.ReadLine();
                while (!CheckAmmount(sAmmount))
                {
                    Console.Clear();
                    Console.WriteLine("Incorrect entry!");
                    Console.WriteLine("Please, enter the amount of cash you would like to " + sType);
                    sAmmount = Console.ReadLine();
                }
                return ATMWFCRef.Process(fUniversal(sType.ToUpper(), sUser, sAmmount));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "-1";
            }
        }

        //Retrieves user's account balance (and turnover)
        private string Balance()
        {
            try
            {
                char cChoice = '\0';
                Boolean bFirsPass = true;

                Console.Clear();
                Console.WriteLine("Would you like to see the account's turnover? (Y/N)");
                do
                {
                    if (!bFirsPass)
                    {
                        if (!(cChoice.ToString().Equals("\r") || cChoice.ToString().Equals("\n")))
                        {
                            Console.Clear();
                            Console.WriteLine("Incorrect choice! Please, enter Y or N!");
                        }
                    }
                    cChoice = Console.ReadKey(true).KeyChar;
                    bFirsPass = false;
                } while (Char.ToUpper(cChoice) != 'Y' && Char.ToUpper(cChoice) != 'N');
                Console.Clear();
                return ATMWFCRef.Process(fUniversal("BALANCE", sUser, cChoice.ToString().ToUpper()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "-1";
            }
        }

        //Universal function - string connector, comma separated
        private string fUniversal(string sAction, string sUser = "", string sArgsList = "")
        {
            return sAction + "," + sUser + "," + sArgsList;
        }

        //Main method
        static void Main(string[] args)
        {
            ATMWCFC myATM = new ATMWCFC();
            myATM.GreetingPhase();
            while (true)
            {
                myATM.TransactionsPhase();
            }
        }
    }
}
