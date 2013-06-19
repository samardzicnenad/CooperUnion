/**********************************************************************
 * Created by : Nenad Samardzic
 * Date       : 05/16/2013
 * Description: The class represents ATM simulation using socket communication.
 * Idea       : ATM Server side application waits for and processes client's ATM requests.
 *              Main thread is up and running all of the time and each client request is delegated to a new thread.
 *              A new thread processes the request and sends back the response.
 *              "Database" for the application is set of flat files. All of the "connected actions" are performed as one transaction - 
 *              complete success or rollback.
 *              This ATM model performs following activities:
 *              - check if the user with the selected <user name> already exists in the system
 *              - creates new user with the selected <user name>
 *              - for the new user creates a customer and an account data
 *              - performs deposit and withdrawal activities
 *              - checks for the user's balance
 *              Application keeps error log - using log4net.
 * Parameters : -
 **********************************************************************/
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Transactions;

using log4net;

namespace Server
{
    public class AsynchronousSocketListener
    {
        private IPHostEntry ipHostInfo;
        private IPAddress ipAddress;
        private IPEndPoint localEndPoint;
        private Socket listener;

        private static readonly ILog log = LogManager.GetLogger(typeof(AsynchronousSocketListener));

        private Transaction myTransaction = new Transaction();
        private Customer myCustomer = new Customer();
        private Account myAccount = new Account();
        private Action myAction = new Action();
        private User myUser = new User();

        // Thread signal.
        private ManualResetEvent allDone = new ManualResetEvent(false);

        //Constructor assigns global variables and creates listener socket
        public AsynchronousSocketListener()
        {
            this.ipHostInfo = Dns.Resolve(Dns.GetHostName()); // Establish the local endpoint for the socket.
            this.ipAddress = ipHostInfo.AddressList[0]; //
            this.localEndPoint = new IPEndPoint(ipAddress, 8080); //
            this.listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Create a TCP/IP socket.
        }


        /******Communication handling section******/
        // Sets listener socket for incoming connections - uses AcceptCallback() - triggered by BeginConnect from the client
        private void StartListening()
        {
            try
            {
                listener.Bind(localEndPoint); // Bind the socket to the local endpoint and listen for incoming connections.
                listener.Listen(100);
                
                while (true)
                {
                    allDone.Reset(); // Set the event to nonsignaled state.
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener); // Start async socket - listen for connections.
                    allDone.WaitOne(); // Wait until a connection is made before continuing.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
            }
            finally
            {
                log.Debug("Server stops!");
                EndListening();
            }
        }

        //Ends listening
        private void EndListening()
        {
            if (listener.Connected)
            {
                listener.Shutdown(SocketShutdown.Both);
                listener.Close(); //kill the listener thread
            }
        }

        // Accepts callback and defines a new thread handler for it - ReadCallback() - triggered by BeginSend from the client
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                allDone.Set(); // Signal the main thread to continue.
                Socket acbListener = (Socket)ar.AsyncState; // Get the socket that handles the client request.
                Socket acbHandler = acbListener.EndAccept(ar); //create new socket that will take over request
                StateObject state = new StateObject(); // Create the state object.
                state.workSocket = acbHandler;
                acbHandler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
            }
        }

        // Reads the message from the client, does the action and sends response
        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                string content = String.Empty;
                StateObject state = (StateObject)ar.AsyncState; // Retrieve state object and handler socket from async state object.
                Socket rcbHandler = state.workSocket;
                int bytesRead = rcbHandler.EndReceive(ar); // Read data from the client socket.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead)); // Store data received
                content = Process(state.sb.ToString());
                Send(rcbHandler, content); // send the result back to the client.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
            }
        }

        // Starts sending response - uses SendCallback()
        private void Send(Socket handler, String data)
        {
            try
            {
                byte[] byteData = Encoding.ASCII.GetBytes(data); // Convert the string data to byte data using ASCII encoding.
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler); // Begin sending
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
            }
        }

        //Handler for sending
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState; // Retrieve the socket from the state object.
                int bytesSent = handler.EndSend(ar);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close(); //kill the handler thread
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
            }
        }


        /******Program logic section******/
        //Process incoming message and return result
        private string Process(string sMessage)
        {
            try
            {
                string[] els = sMessage.Split((','));
                switch (els[0])
                {
                    case "SEARCH":
                        return myUser.Search(els[1], els[2]);
                    case "CREATE":
                        return myUser.Create(els[1], els[2]);
                    case "CREATECUSTACC":
                        return tsCustAcc(els[1], els[2], els[3]);
                    case "DEPOSIT":
                    case "WITHDRAW":
                        return DepWith(els[0], els[1], els[2]);
                    case "BALANCE":
                        return Balance(els[1], els[2]);
                    default:
                        return "1";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
                return "-1";
            }
        }

        //Transaction scope for creating customer and his/her account
        private string tsCustAcc(string sUser, string sFullName, string sAccountNo)
        {
            try
            {
                using (TransactionScope TranScope = new TransactionScope())
                {
                    myCustomer.Create(sUser, sFullName, sAccountNo);
                    string sRes = myAccount.Create(sAccountNo);
                    TranScope.Complete();
                    return sRes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString());
                return "-1";
            }
        }

        //Makes a deposit or withdrawal
        private string DepWith(string sType, string sUser, string sAmmount)
        {
            try
            {
                using (TransactionScope TranScope = new TransactionScope())
                {
                    string sAccount = myCustomer.GetAccount(sUser);
                    string sOutput = myAccount.CalculateBalance(sType, sAccount, sAmmount);
                    if (sOutput == "0") return "0";
                    string sActID = myAction.Create(sType, sAmmount);
                    myTransaction.Create(sType, sAmmount, sAccount, sActID);
                    return myAccount.Update(sOutput);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
                return "-1";
            }
        }

        //returns customer's balance
        private string Balance(string sUser, string sType)
        {
            string contents, sOutput = "", sTransType = "", sAmmount = "", sTimeStamp = "";
            string[] rows, actions;

            try
            {
                string sAccountB = myCustomer.GetAccount(sUser);
                Tuple<string, string> myTuple = myAccount.GetBalance(sAccountB);

                if (sType.Equals("N"))
                    return "Current balance on your cash account is: " + myTuple.Item1;
                string sTrans = myTuple.Item2;

                contents = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TRANSACTION.csv");
                rows = contents.Replace("\r\n", "").Split(('\n'));
                contents = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ACTION.csv");
                actions = contents.Replace("\r\n", "").Split(('\n'));

                string[] trans = sTrans.Split((';'));
                foreach (string tran in trans)
                {
                    if (tran.Equals("")) continue;
                    foreach (string row in rows)
                    {
                        if (row.Equals("")) continue;
                        string[] els = row.Split((','));
                        if (els[0].Equals(tran))
                        {
                            sTimeStamp = els[1];
                            string sActID = els[3];
                            foreach (string action in actions)
                            {
                                if (action.Equals("")) continue;
                                string[] acts = action.Split((','));
                                if (acts[0].Equals(sActID))
                                {
                                    if (acts[1].Equals("D"))
                                        sTransType = "DEPOSIT   ";
                                    else
                                        sTransType = "WITHDRAWAL";
                                    sAmmount = acts[2];
                                }
                            }
                        }
                    }
                    sOutput = sOutput + "\nTransaction date: " + sTimeStamp.Substring(4, 2) + "/" + sTimeStamp.Substring(6, 2) + "/" + sTimeStamp.Substring(0, 4)
                        + "; Transaction type: " + sTransType + "; Ammount: " + sAmmount;
                }
                return sOutput;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                log.Debug(ex.ToString()); 
                return "-1";
            }
        }

        //main method starts with StartListening()
        static void Main(String[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Debug("Server starts!");
            AsynchronousSocketListener myASL = new AsynchronousSocketListener();
            myASL.StartListening();
        }
    }
}