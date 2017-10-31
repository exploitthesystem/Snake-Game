/***********************************************************************
* Project :    PS7 - Snake Client, PS8 - Snake Server
* File    :    NetworkController.cs
* Name    :    Marko Ljubicic, Preston Balfour
* Date    :    12/08/2016
*
* Description:   This namespace contains networking code that opens the sockets
*                between the client and the server and provides helper functions for 
*                sending and receiving data. It contains a delegate for storing
*                callback functions, a state object for containing connection state,
*                and a static Network class that with helper functions for establishing
*                client-server communication.
* 
************************************************************************/

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Snake
{
    /// Delegate for callback functions stored in the state object.
    public delegate void CallbackDelegate(SocketState state);

    /// <summary>
    /// This class holds all the necessary state to handle a client connection.
    /// It is a simple collection of fields.
    /// </summary>
    public class SocketState
    {
        // Socket that represents the endpoint for the connection.
        private Socket sock;

        // Variable that stores the callback function passed as a delegate.
        private CallbackDelegate callbackDel;

        // This is the buffer where we will receive data from the remote device in bytes.
        private byte[] bytebuffer = new byte[1024];

        // Contains complete (parsed) messages received from the server.
        private string[] databuffer;

        // The number of complete messages in the data buffer.
        private int datalength;

        // This is a larger (growable) buffer, in case a single receive does not contain the full data.
        private StringBuilder sb = new StringBuilder();

        /// <summary>
        /// Constructs a state object for client-server communication.
        /// </summary>
        /// <param name="socket"></param>
        public SocketState(Socket socket)
        {
            sock = socket;
        }

        /// <summary>
        /// The player ID associated with the connection.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The socket stored in the state object.
        /// </summary>
        public Socket Socket { get { return sock; } }

        /// <summary>
        /// The callback function stored in the state object.
        /// </summary>
        public CallbackDelegate Callback { get { return callbackDel; } set { callbackDel = value; } }

        /// <summary>
        /// The buffer stored in the state object.
        /// </summary>
        public byte[] Buffer { get { return bytebuffer; } set { bytebuffer = value; } }

        /// <summary>
        /// A larger buffer for storing messages.
        /// </summary>
        public StringBuilder Data { get { return sb; } set { sb = value; } }

        /// <summary>
        /// A complete message received from the remote device.
        /// </summary>
        public string[] Messages { get { return databuffer; } set { databuffer = value; } }

        /// <summary>
        /// Get or set the number of messages in the data buffer.
        /// </summary>
        public int DataLength { get { return datalength; } set { datalength = value; } }
    }

    /// <summary>
    /// A state class used by the server networking functions to store the TcpListener
    /// and callback delegates.
    /// </summary>
    public class HostRequest
    {
        // The TcpListener for receiving client connections.
        TcpListener listener;
        // The delegate callback upon establishing connection.
        CallbackDelegate callMe;

        /// <summary>
        /// Accepts a TcpListener and delegated callback function.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="callMe"></param>
        public HostRequest(TcpListener listener, Delegate callMe)
        {
            this.listener = listener;
            this.callMe = (CallbackDelegate)callMe;
        }

        /// <summary>
        /// Get the TcpListener stored in the state object.
        /// </summary>
        public TcpListener Listener { get { return listener; } }

        /// <summary>
        /// Get the callback delegate stored in the state object.
        /// </summary>
        public CallbackDelegate CallMe { get { return callMe; } } 
    }

    /// <summary>
    /// A static class that provides networking functions for communication between a client and server.
    /// </summary>
    public static class Network
    {
        /// <summary>
        /// The port used to connect to the remote server.
        /// </summary>
        public const int DEFAULT_PORT = 11000;

        /// <summary>
        /// hostname - the name of the server to connect to
        ///
        /// callbackFunction - a function inside the client to be called when a connection is made
        ///
        /// This function should attempt to connect to the server via a provided hostname.
        /// It should save the callback function (in a socket state object) for use when data arrives.
        /// It will need to open a socket and then use the BeginConnect method. Note this method takes the "state"
        /// object and "regurgitates" it back to you when a connection is made, thus allowing "communication" between 
        /// this function and the ConnectedToServer function.
        /// </summary>
        /// <param name="callbackFunction"></param>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static Socket ConnectToServer(Delegate callbackFunction, string hostname)
        {
            try
            {
                Debug.WriteLine("connecting  to " + hostname);

                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo;
                IPAddress ipAddress = IPAddress.None;

                // Determine if the server address is a URL or an IP.
                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostname);
                    bool foundIPV4 = false;
                    foreach (IPAddress addr in ipHostInfo.AddressList)
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }
                    // Didn't find any IPV4 addresses
                    if (!foundIPV4)
                    {
                        Debug.WriteLine("Invalid address: " + hostname);
                        return null;
                    }
                }
                catch (Exception)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    Debug.WriteLine("using IP");
                    ipAddress = IPAddress.Parse(hostname);
                }

                // Create TCP/IP socket for connecting to remote server.
                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Instantiate new SocketState object, which holds state for handling connection.
                SocketState state = new SocketState(socket);

                // Store callback function in state object.
                state.Callback = (CallbackDelegate)callbackFunction;

                // Begin connection to remote host.
                socket.BeginConnect(ipAddress, DEFAULT_PORT, ConnectedToServer, state);

                // Return the socket.
                return state.Socket;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                throw;
            }
        }

        /// <summary>
        /// This function is referenced by the BeginConnect method above and is "called" by the OS when the socket 
        /// connects to the server. The "state_in_an_ar_object" object contains a field "AsyncState" which contains the 
        /// "state" object saved away in the above function.
        /// 
        /// Once a connection is established the "saved away" callbackFunction needs to called. This function is
        /// saved in the socket state, and was originally passed in to ConnectToServer.
        /// </summary>
        /// <param name="state_in_an_ar_object"></param>
        public static void ConnectedToServer(IAsyncResult state_in_an_ar_object)
        {//
            // Assign state object stored in the argument to new state object instance.
            SocketState state = (SocketState)state_in_an_ar_object.AsyncState;
            // Terminate the stored socket's request to connect to remote host.
            state.Socket.EndConnect(state_in_an_ar_object);
            // Call the callback function saved in the socket state.
            state.Callback(state);
            // Begin receiving data from the remote host.
            GetData(state);
         }

        /// <summary>
        /// The ReceiveCallback method is called by the OS when new data arrives. 
        /// This method checks to see how much data has arrived. If 0, the connection has been closed 
        /// (presumably by the server). On greater than zero data, this method calls the callback function 
        /// provided above.
        /// </summary>
        /// <param name="state_in_an_ar_object"></param>
        public static void ReceiveCallback(IAsyncResult state_in_an_ar_object)
        {
            try
            {
                // Assign state object stored in the argument to new state object instance.
                SocketState state = (SocketState)state_in_an_ar_object.AsyncState;
                // Read the number of bytes received from the remote device.
                int bytes = state.Socket.EndReceive(state_in_an_ar_object);

                // If data has been received, get the data.
                if (bytes > 0)
                {
                    // Read data from the remote device and store data received so far.
                    string incomingData = Encoding.UTF8.GetString(state.Buffer, 0, bytes);
                    state.Data.Append(incomingData);

                    // Process the data received so far.
                    ProcessData(state);

                    // Call the function stored in the state object.
                    state.Callback(state);
                } 
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to connect to remote host. Error occured: " + e);
            }
        }

        /// <summary>
        /// This is a helper function that the client View code will call whenever it wants more data. 
        /// </summary>
        /// <param name="state"></param>
        public static void GetData(SocketState state)
        {
            // Begin receiving data from the remote host.
            state.Socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
        }

        /// <summary>
        /// This function (along with its helper 'SendCallback') will allow a program to send data over a socket.
        /// This function converts data into bytes and then sends them using socket.BeginSend.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void Send(Socket socket, String data)
        {
            try
            {
                // Create state object.
                SocketState state = new SocketState(socket);

                // Convert data to byte array and store in state buffer.
                state.Buffer = Encoding.UTF8.GetBytes(data);
                // Send data to remote host. The SendCallback method below is passed as an argument.
                state.Socket.BeginSend(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), state);
            }
            catch(Exception e)
            {
                Debug.WriteLine("Unable to send to remote host. Error occurred: " + e);
            }
        }

        /// <summary>
        /// This function "assists" the Send function. If all the data has been sent, then life is good 
        /// and nothing needs to be done.
        /// </summary>
        public static void SendCallback(IAsyncResult state_in_an_ar_object)
        {
            try
            {
                // Assign state object stored in the argument to new state object instance.
                SocketState state = (SocketState)state_in_an_ar_object.AsyncState;

                // Conclude the send operation.
                int bytesSent = state.Socket.EndSend(state_in_an_ar_object);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to send to remote host. Error occured: " + e);
            }
        }

        /// <summary>
        /// This is the heart of the server code. It startse a TCP listener for new connections and passes the listener, 
        /// along with the callMe function, to BeginAcceptSocket as the state parameter. Upon a connection request coming 
        /// in the OS invokes the AcceptNewClient as the callback method.
        /// </summary>
        /// <param name="callMe"></param>
        public static void ServerAwaitingClientLoop(Delegate callMe)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, DEFAULT_PORT);

            HostRequest clientRequest = new HostRequest(listener, callMe);

            listener.Start();

            listener.BeginAcceptSocket(AcceptNewClient, clientRequest);
        }

        /// <summary>
        /// This is the callback that BeginAcceptSocket uses. It performs the following operations:
        /// 
        /// 1. Extracts the state containing the TcpListener and the callMe delegate from "ar"
        /// 2. Creates a new socket with by using listener.EndAcceptSocket
        /// 3. Saves the socket in a new SocketState
        /// 4. Calls the callMe method and pass it the new SocketState
        /// 5. Awaits a new connection request (continue the event loop) with BeginAcceptSocket.
        /// </summary>
        /// <param name="ar"></param>
        public static void AcceptNewClient(IAsyncResult ar)
        {
            // Extracts the state.
            HostRequest request = (HostRequest)ar.AsyncState;

            // Creates a new socket.
            Socket s = request.Listener.EndAcceptSocket(ar);

            // Saves the socket in a SocketState.
            SocketState state = new SocketState(s);

            // Calls the callMe method.
            request.CallMe(state);

            // Awaits a new connection request.
            request.Listener.BeginAcceptSocket(AcceptNewClient, request);
        }

        /// <summary>
        /// This private helper method processes data on behalf of ReceiveCallback.
        /// It is used to extract partial data while more data is incoming.
        /// </summary>
        /// <param name="ss"></param>
        private static void ProcessData(SocketState ss)
        {
            string totalData = ss.Data.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            string[] data = new string[parts.Length];

            // Loop until we have processed all messages.
            // We may have received more than one.
            int length = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                // Ignore empty strings added by the regex splitter
                if (parts[i].Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (parts[i][parts[i].Length - 1] != '\n')
                    break;

                // Remove the terminator character and store the complete message in the data buffer.
                data[i] = parts[i].Replace("\n", "");
                
                // Then remove it from the SocketState's growable buffer
                ss.Data.Remove(0, parts[i].Length);
                
                length++;
            }

            // Store the array of parsed data into the state object.
            ss.Messages = data;
            ss.DataLength = length;
        }
    }
}
