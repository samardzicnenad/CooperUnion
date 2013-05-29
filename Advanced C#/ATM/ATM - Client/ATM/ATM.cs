using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

namespace ATM
{
    class ATM
    {
        AsynchronousClient myAC;
        private string sUser, sPassword;
        private Boolean bLoggedIn; //, bNewCycle;

        private static readonly ILog log = LogManager.GetLogger(typeof(ATM));

        //class constructor
        public ATM()
        {
            myAC = new AsynchronousClient(); //Instance of AsynchronousClient class

            this.sUser = "";
            this.sPassword = "";
            this.bLoggedIn = false;
            //this.bNewCycle = false;
        }

        /******User interface section******/
        //Responses to selected actions
        private void GreetingPhase()
        {
            switch (Greeting())
            {
                case 0: //exit
                    Exit();
                    break;
                case 1: //create an account
                    if (CreateAccount().Equals("1"))
                        CreateCustAcc(sUser);
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
                    //this.bNewCycle = true;
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


        /******Connection handling section******/
        //Check connection to server
        private void Connected()
        {
            try
            {
                if (myAC.fUniversal("CONN CHECK").Equals("1")) //Connection present
                {
                    log.Debug("Successfully connected to server!"); 
                    Console.WriteLine("Successfully connected to server!");
                    System.Threading.Thread.Sleep(1500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while connecting to server!");
                log.Debug("Error while connecting to server!"); 
                System.Threading.Thread.Sleep(1500);
                Exit();
            }
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
                return myAC.fUniversal("SEARCH", sUser, sPassword);
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
                log.Debug(ex.ToString());
            }
        }

        //Log out
        private void LogOut()
        {
            sUser = "";
            sPassword = "";
            bLoggedIn = false;
            log.Debug("Logging out."); 
        }

        //Exits the ATM
        private void Exit()
        {
            //DISCONNECT FROM THE SERVER
            if (bLoggedIn)
                LogOut();
            Console.Clear();
            Console.WriteLine("Thank you. Goodbye.");
            log.Debug("Client is shutting down."); 
            System.Threading.Thread.Sleep(3500);
            Environment.Exit(0); //!---
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
            /*Console.WriteLine("Press any key to continue");
            Console.ReadKey(true);*/
            System.Threading.Thread.Sleep(2500);
        }


        /******Program logic section******/
        //Creates an account
        private string CreateAccount()
        {
            Boolean bFirstPass = true;

            try
            {
                do
                {
                    do
                    {
                        Console.Clear();
                        if (!bFirstPass) Console.WriteLine("That user name is already taken.");
                        Console.WriteLine("Please, enter your username:");
                        sUser = Console.ReadLine();
                    } while (sUser == "");
                    bFirstPass = false;
                } while (myAC.fUniversal("SEARCH", sUser).Equals("0"));
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
                return myAC.fUniversal("CREATE", sUser, sPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
                return "-1";
            }
        }

        //Creates a customer
        private string CreateCustAcc(string userName)
        {
            string sFName, sLName, sAccount;

            try
            {
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

                return myAC.fUniversal("CREATECUSTACC", sUser, sFName + " " + sLName + "," + sAccount);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
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
                return myAC.fUniversal(sType.ToUpper(), sUser, sAmmount);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
                return "-1";
            }
        }

        //Retrieves user's account balance (and turnover)
        private string Balance()
        {
            try
            {
                /*char cChoice = '\0';
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
                } while (cChoice != 'Y' && cChoice != 'y' && cChoice != 'N' && cChoice != 'n');
                Console.Clear();
                return myAC.Balance(sUser, "ACCOUNT", "TRANSACTION", "ACTION", cChoice.ToString().ToUpper());*/
                Console.Clear();
                return myAC.fUniversal("BALANCE", sUser, "N");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
                return "-1";
            }
        }

        //Main method
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            ATM myATM = new ATM();
            
            //Check connection to server
            myATM.Connected();
            //while (true)
            //{
                myATM.GreetingPhase();
                //while (!myATM.bNewCycle)
                while (true)
                {
                    myATM.TransactionsPhase();
                }
                //myATM.bNewCycle = false;
            //}
        }
    }
}