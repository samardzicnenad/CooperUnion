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
 * 				  This is a client side application.
 */
import java.io.*;
import java.net.*;
import java.awt.*;
import java.awt.event.*;
import javax.swing.*;

public class Client extends JFrame {
	private static final long serialVersionUID = 6903805369846951238L;
	private JTextField myJTF = new JTextField(); // Text field for receiving command
	private JTextArea myJTA = new JTextArea(); // Text area to display contents
	private PrintWriter toServer;
	private BufferedReader fromServer;
	private Socket socket;

	//Panel myJP to hold the label and text field
	private void CreatePanel() {
		JPanel myJP = new JPanel();
		myJP.setLayout(new BorderLayout());
		myJP.add(new JLabel("Enter a command; 'END' for the end of the session: "), BorderLayout.WEST);
		myJP.add(myJTF, BorderLayout.CENTER);
		myJTF.setHorizontalAlignment(JTextField.RIGHT);

		setLayout(new BorderLayout());
		add(myJP, BorderLayout.NORTH);
		add(new JScrollPane(myJTA), BorderLayout.CENTER);

		myJTF.addActionListener(new ButtonListener()); // Register listener

		setTitle("Client");
		setSize(500, 300);
		setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		setVisible(true); // It is necessary to show the frame here!
	}
	
	private void StartClient() {
		try {
			socket = new Socket("localhost", 8000); //socket to connect to the server
			//send data to the server
			toServer = new PrintWriter(socket.getOutputStream(), true);
			//receive data from the server
			fromServer = new BufferedReader(new InputStreamReader(socket.getInputStream()));
		}
		catch (IOException ex) {
			myJTA.append(ex.toString() + '\n');
		}
	}

	private class ButtonListener implements ActionListener {
		public void actionPerformed(ActionEvent e) {
			String sEcho="";
			try {
				// Get the command from the text field
				myJTA.append("Command is " + myJTF.getText().trim() + "\n");
				toServer.println(myJTF.getText().trim());
				myJTF.setText("");

				// Get echo from the server char by char
				sEcho = fromServer.readLine();
				myJTA.append("Echo received from server: " + sEcho + '\n');
				sEcho="";
			}
			catch (IOException ex) {
				System.err.println(ex);
			}
		}
	}
	//main method
	public static void main(String[] args) {
		Client myClient = new Client();
		myClient.CreatePanel();
		myClient.StartClient();
	}
}