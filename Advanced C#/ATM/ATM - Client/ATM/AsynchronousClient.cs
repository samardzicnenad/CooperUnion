/**********************************************************************
 * Created by : Nenad Samardzic
 * Date       : 05/16/2013
 * Description: The class represents client ATM communication handler - using sockets.
 * Idea       : ATM Client side application receives client's ATM requests, builds "the appropriate sentence" and sends request to the server.
 *              This class sends different client's ATM requests using one universal function and receives the response.
 *              Signals:
 *              -1: error
 *               0: negative outcome
 *               1: positive outcome
 *              Application keeps error log - using log4net.
 * Parameters : -
 **********************************************************************/
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

using log4net;

public class AsynchronousClient
{
    private const int port = 8080; // The port number for the remote device.
    private IPHostEntry ipHostInfo;
    private IPAddress ipAddress;
    private IPEndPoint remoteEP;

    private static readonly ILog log = LogManager.GetLogger(typeof(AsynchronousClient));

    private string response; //The response from the remote device.
    private Exception myException; //Catch exception from a thread

    // ManualResetEvent instances signal completion.
    private ManualResetEvent connectDone = new ManualResetEvent(false);
    private ManualResetEvent sendDone = new ManualResetEvent(false);
    private ManualResetEvent receiveDone = new ManualResetEvent(false);

    //Async client constructor
    public AsynchronousClient()
    {
        log4net.Config.XmlConfigurator.Configure();

        this.ipHostInfo = Dns.Resolve(Dns.GetHostName()); // Establish the remote endpoint for the socket.
        this.ipAddress = ipHostInfo.AddressList[0];
        this.remoteEP = new IPEndPoint(ipAddress, port);

        this.response = String.Empty;
        this.myException = null;
    }

    //Universal function
    public string fUniversal(string sAction, string sUser = "", string sArgsList = "")
    {
        try
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Create TCP/IP socket.
            connectDone.Reset();
            client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client); // Connect to the remote endpoint. 
            connectDone.WaitOne();
            if (!ReferenceEquals(myException, null)) //No connection present
                throw (myException);
            sendDone.Reset();
            Send(client, sAction + "," + sUser + "," + sArgsList); // Send data to the remote device.
            sendDone.WaitOne();
            receiveDone.Reset();
            Receive(client); // Receive the response from the remote device.
            receiveDone.WaitOne();
            client.Shutdown(SocketShutdown.Both); //End connection
            client.Close();
            return response;
        }
        catch (Exception ex)
        {
            log.Debug(ex.ToString()); 
            throw ex;
        }
    }

    //Connection handler
    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState; // Retrieve the socket from the state object.
            client.EndConnect(ar); //Socket connected to client.RemoteEndPoint
            connectDone.Set(); // Signal that the connection has been made.
        }
        catch (Exception ex)
        {
            //Console.WriteLine(e.ToString());
            log.Debug(ex.ToString()); 
            myException = ex;
            connectDone.Set();
        }
    }

    //Sends the message and uses SendCallback()
    private void Send(Socket client, String data)
    {
        try
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data); // Convert the string data to byte data using ASCII encoding.
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client); // Begin sending
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            log.Debug(ex.ToString()); 
        }
    }

    //Sending process handler
    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState; // Retrieve the socket from the state object.
            int bytesSent = client.EndSend(ar); // Complete sending the data to the remote device.
            sendDone.Set(); // Signal that all bytes have been sent.
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            log.Debug(ex.ToString()); 
        }
    }

    //Receives the message and uses ReceiveCallback
    private void Receive(Socket client)
    {
        try
        {
            StateObject state = new StateObject(); // Create the state object.
            state.workSocket = client;
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            log.Debug(ex.ToString()); 
        }
    }

    //Receiving process handler
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            String content = String.Empty;
            StateObject state = (StateObject)ar.AsyncState; // Retrieve state object and client socket from async state object.
            Socket client = state.workSocket;
            int bytesRead = client.EndReceive(ar); // Read data from the remote device.
            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead)); //Store the data received. Might be more
            response = state.sb.ToString();
            receiveDone.Set(); // Signal that all bytes have been received.
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            log.Debug(ex.ToString()); 
        }
    }
}