using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace PreHost
{
      //    TODO: maybe part of the problem is that you are using a TcpClient as a server...
    //                      if switching all your network operations to non async does not work try using a Listener instead...
    //  TODO: the phone is not responding correctly to the port assign message.
    //  TODO: moving forward,
    //          for our intents and purposes,
    //              it probably won't be neccessary to set a limit on the number of phones which can connect to the host...
    //  TODO:   Once we get a concrete plan for a class hierarchy we need to make a new solution....
    //  TODO: this code is built upon some code I found on MSDN.
    //          it is slowly shaping itself into a control module for the host...

    class Host
    {
        //we need two Udp sockets to handle the broadcast
        private static UdpClient _handShakeListener = null;
        private static UdpClient _handShakeSender = null;

        // port number reserved for connection requests by phones
        private const int _handshakeReceivePort = 9998;
        private const int _handshakeSendPort = 9999;
        private const int _playSendPort = 10000;
        // number of phones currently connected to the host.
        private static int _numberOfConnections = 0;
        private static int _startingPortNumber = 10001;

        private static Slot[] _slotArray = null;

        private static TcpClient _streamSocket = null;

        private static TcpListener _streamListener = null;

        private static string _phoneAddress = "";
        private static string _phoneType = "";


        
        // TODO: we are going to need arrays of UdpClients for sending and responding to phones after they have connected to the host.
        //              all responders will listen on port _numberOfConnections + _startingPortNumber
        static int Main(string[] args)
        {
            _slotArray = new Slot[12];
            bool done = false;

            _handShakeListener = new UdpClient(_handshakeReceivePort);
            _handShakeSender = new UdpClient(_handshakeSendPort);
            _streamSocket = new TcpClient();

            _streamListener = new TcpListener(_handshakeReceivePort);

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, _handshakeReceivePort);
            string received_data;
            byte[] receive_byte_array;
            try
            {
                //  TODO: this while loop is strictly in place to receive connection requests from phones.
                while (!done)
                {
                    receive_byte_array = _handShakeListener.Receive(ref groupEP);
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    Console.WriteLine("{0}", received_data);

                    // if we received the proper connection request message from a phone...
                    if (received_data.StartsWith("Hello Audio-Mobile Address:"))
                    {
                        // parse message to array
                        string[] messageArray = received_data.Split(' ');

                        _phoneAddress = messageArray[3];
                        _phoneType = messageArray[5];

                       int assignedPort = _startingPortNumber + _numberOfConnections;
                       string hostName = Dns.GetHostName();
                       IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
                       string hostIpAddress = "";

                        // TODO: out of the 7 ipaddresses for my think pad on the home network,
                        //                  the only one that was not ipv6 had an adress family of InterNetwork.
                        //                      we need to double check and make sure that this kind of discrimination will work on other machines,
                        //                          and on other networks....
                        foreach(IPAddress address in ipAddresses)
                        {
                            if(address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                hostIpAddress = address.ToString();
                            }
                        }

                       // assemble connection response
                       string response = "Hello there, use port: " + assignedPort.ToString() + " and address: " + hostIpAddress;
                       byte[] responseArray = Encoding.ASCII.GetBytes(response);
                       int g = responseArray.Length;

                       _handShakeSender.Connect(_phoneAddress, _handshakeSendPort);
                       _handShakeSender.Send(responseArray, responseArray.Length);

                        // TODO: we are probably going to need to set up the sockets corressponding to the new connection on a separate thread
                        // add a new slot to the slot array
                        _slotArray[_numberOfConnections] = new Slot(_phoneType, _phoneAddress,  assignedPort, _numberOfConnections);

                        // TODO: we may want to keep a member thread array,
                        //          it might be easier to keep track of whether threads are alive or not....
                        // launch new thread based upon new Slot
                        Thread thread = new Thread(new ThreadStart(_slotArray[_numberOfConnections].receivePlaySignal));
                        thread.Start();
                    }
                }
            }
            catch (Exception e){    Console.WriteLine(e.ToString());    }
            _handShakeListener.Close();
            return 0;
        }

        /// <summary>
        /// callback for when the host makes an initial TCP connection to a phone
        /// </summary>
        /// <param name="ar"></param>
        public static void initialHostToPhoneConnectionCallback(IAsyncResult ar)
         {
            _streamSocket.EndConnect(ar);

            bool i = _streamSocket.Connected;

            bool k = true;
        }

        /// <summary>
        /// Call back for when the host sends an asigned port number to a newly connected phone
        /// </summary>
        /// <param name="ar"></param>
        public static void portAssignWriteToPhoneCallback(IAsyncResult ar)
        {
            // TODO: once we have sent the message to the phone containg the phone's newly assigned port,
            //                  increment the number of connections....

            _numberOfConnections++;
        }

        public static void readAfterPhoneAcceptedConnectionCallback(IAsyncResult ar)
        {
            int y = 0;
        }
    }
}
