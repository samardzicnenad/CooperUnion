/* Course		: Advanced Java Programming
 * Author		: Nenad Samardzic
 * Date			: 03/10/2013
 * Description	: Implement a java server/client chat program.
 * 				  The server can be very simple and echo the client’s commands back at it. 
 * 				  A session is terminated when the client enters the word “END”. 
 * 				  At that point, your server writes a transcript of the clients 
 * 				  chat dialog to an XML file entitled “transcript-current system time”.xml.
 * 				  Also, the complete transcript is recorded into mySQL database:
 * 					Database name: advjava
 * 					Table name: xmlparse
 * 					Fields (all string): ProjectName, UserName, ClientIP, Port, Transcript
 * 				  This is a server side application.
 */
import java.io.*;
import java.net.*;
import java.awt.*;
import javax.swing.*;
import javax.xml.parsers.*;
import javax.xml.transform.*;
import javax.xml.transform.dom.*;
import javax.xml.transform.stream.*;
import org.w3c.dom.*;
import java.sql.*;

public class Server extends JFrame {
	private static final long serialVersionUID = 2436210673342913313L;
	static private JTextArea myJTA = new JTextArea(); // Text area for displaying contents

	private BufferedReader inputFromClient;
	private PrintWriter outputToClient;

	private String sPort = "8000", sIP, sFileName, sCommand, sDBText;

	private TransformerFactory transformerFactory;
	private Transformer transformer;
	private DOMSource source;
	private StreamResult result;

	private DocumentBuilderFactory docFactory;
	private DocumentBuilder docBuilder;
	private Document doc;
	private Element rootElement, nodeIP, nodePort, nodeTrans, nodeClient;
	private Comment nodeComment;

	//Create text area to show the communication
	private void CreateArea() {
		setLayout(new BorderLayout());
		add(new JScrollPane(myJTA), BorderLayout.CENTER);

		setTitle("Server");
		setSize(500, 300);
		setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		setVisible(true);
	}
	//method which manages the logic
	private void RunServer() {
		boolean bFirstPass = true;

		try {
			StartServer();
			while (true) {
				sCommand = inputFromClient.readLine(); // Receive command from the client
				if (sCommand.equals("END")) {
					if (!bFirstPass) { //first pass - don't generate xml file
						SaveXML();
						bFirstPass = true;
					}
					sCommand = "Good bye!";
					bFirstPass = true;
				}
				else { //in the first pass generate start xml content and add the first command
					if(bFirstPass) {
						PopulateXML();
						bFirstPass = false;
					}
					else { // add new command
						AddCommand();
					}
					myJTA.append("Command received from client: " + sCommand + '\n');
				}
				SendEcho(); //Send echo to the client
			}
		}
		catch(IOException ex) {
			System.err.println(ex);
		}
	}
	//create a socket, listen for a connection and prepare reader and writer
	private void StartServer() {
		ServerSocket serverSocket;
		Socket socket;
		
		try {
			serverSocket = new ServerSocket(Integer.parseInt(sPort)); // Create a server socket
			myJTA.append("Server started at " + new java.util.Date() + '\n');
			socket = serverSocket.accept(); // Listen for a connection request

			inputFromClient = new BufferedReader(new InputStreamReader(socket.getInputStream()));
			outputToClient = new PrintWriter(socket.getOutputStream(), true);
		}
		catch (IOException ex) {
			myJTA.append(ex.toString() + '\n');
		}
	}
	//save XML file with client-server communication
	private void SaveXML() {
		try { // write the content into xml file
			nodeTrans = doc.createElement("transcript");
			nodeTrans.appendChild(doc.createTextNode(sCommand));
			nodeClient.appendChild(nodeTrans);							
			transformerFactory = TransformerFactory.newInstance();
			transformer = transformerFactory.newTransformer();
			source = new DOMSource(doc);
			sFileName = "transcript-" + System.currentTimeMillis() + ".xml";
			result = new StreamResult(new File(System.getProperty("user.home") + File.separator + sFileName));
			transformer.transform(source, result);
			System.out.println("XML file saved!");
			DBSave(sIP, sDBText); //Insert session into mySQL database
		} catch (TransformerException tfex) {
			tfex.printStackTrace();
		}
	}
	//build XML file structure during the first pass
	private void PopulateXML() {
		try {
			docFactory = DocumentBuilderFactory.newInstance();
			docBuilder = docFactory.newDocumentBuilder();
			// root elements
			doc = docBuilder.newDocument();
			
			nodeComment = doc.createComment("Advanced Java Assignment");
			doc.appendChild(nodeComment);
			nodeComment = doc.createComment("Nenad Samardzic");
			doc.appendChild(nodeComment);
			
			rootElement = doc.createElement("Bnai_Zion_and_Cooper_Union");
			doc.appendChild(rootElement);

			nodeClient = doc.createElement("client");
			rootElement.appendChild(nodeClient);

			nodeIP = doc.createElement("ip");
			sIP = "" + InetAddress.getLocalHost().getHostAddress();
			nodeIP.appendChild(doc.createTextNode(sIP));
			nodeClient.appendChild(nodeIP);

			nodePort = doc.createElement("port");
			nodePort.appendChild(doc.createTextNode(sPort));
			nodeClient.appendChild(nodePort);

			nodeTrans = doc.createElement("transcript");
			nodeTrans.appendChild(doc.createTextNode(sCommand));
			sDBText = sDBText + sCommand;
			nodeClient.appendChild(nodeTrans);								
		} catch (ParserConfigurationException pcex) {
			pcex.printStackTrace();
		} catch (UnknownHostException uhex) {
			uhex.printStackTrace();
		}
	}
	//add a command per turn
	private void AddCommand() {
		nodeTrans = doc.createElement("transcript");
		nodeTrans.appendChild(doc.createTextNode(sCommand));
		sDBText = sDBText + "\n" + sCommand;
		nodeClient.appendChild(nodeTrans);
	}
	//respond to client
	private void SendEcho() {
		outputToClient.println(sCommand);
		outputToClient.flush();
		sCommand="";
	}
	//save the communication to the DB
	private void DBSave(String sIP, String sDBText) {
		Connection myCon = null;
		try {
			Class.forName("com.mysql.jdbc.Driver");
			//username and password need to be provided to connection
			//this wasn't my main focus, so I chose the following representation
			myCon = DriverManager.getConnection("jdbc:mysql://localhost/advjava", "XXXXX", "XXXXX");
			System.out.println ("Database connection established");
			String sSQL = "INSERT INTO xmlparse (ProjectName, UserName, ClientIP, Port, Transcript) VALUES" + "(?, ?, ?, ?, ?)";
			PreparedStatement statement = myCon.prepareStatement(sSQL);
			statement.setString(1, "Advanced Java Assignment");
			statement.setString(2, System.getProperty("user.name"));
			statement.setString(3, sIP);
			statement.setString(4, sPort);
			statement.setString(5, sDBText);
			statement.executeUpdate();
			//statement = myCon.prepareStatement("SELECT ProjectName, UserName, ClientIP, Port, Transcript from advjava.xmlparse");
			//ResultSet resultSet = statement.executeQuery();
			//while (resultSet.next()) {
			//	System.out.println(resultSet.getString("Transcript"));
			//}
		}
		catch (Exception ex) {
			JOptionPane.showMessageDialog(null,"Error - " + ex.getMessage());
		} finally {
			System.out.println("Closing DB connection.");
			if (myCon != null) {
				try {
					myCon.close();
				}
				catch (SQLException ignore) {}
			}
		}
	}
	//main method
	public static void main(String[] args) {
		Server myServer = new Server();
		myServer.CreateArea();
		myServer.RunServer();
	}
}