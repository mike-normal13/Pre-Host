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
    //  TODO: the phone is not responding correctly to the port assign message.
    //  TODO: moving forward,
    //          for our intents and purposes,
    //              it probably won't be neccessary to set a limit on the number of phones which can connect to the host...
    //  TODO:   Once we get a concrete plan for a class hierarchy we need to make a new solution....
    //  TODO: this code is built upon some code I found on MSDN.
    //          it is slowly shaping itself into a control module for the host...

    class Host
    {
        // port number reserved for connection requests by phones
        private const int _handshakePort = 9999;
        private const int _tcpPort = 10000;
        // number of phones currently connected to the host.
        private static int _numberOfConnections = 0;
        private static int _startingPortNumber = 10001;

        private static Slot[] _slotArray = null;

        private static TcpClient _streamSocket = null;

        private static string _phoneAddress = "";
        private static string _phoneType = "";

        // TODO: we are going to need arrays of UdpClients for sending and responding to phones after they have connected to the host.
        //              all responders will listen on port _numberOfConnections + _startingPortNumber

        static int Main(string[] args)
        {
            _slotArray = new Slot[12];
            bool done = false;
            
            UdpClient handShakeListener = new UdpClient(_handshakePort);
            _streamSocket = new TcpClient();
            
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, _handshakePort);
            string received_data;
            byte[] receive_byte_array;
            try
            {
                //  TODO: this while loop is strictly in place to receive connection requests from phones.
                while (!done)
                {
                    receive_byte_array = handShakeListener.Receive(ref groupEP);
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    Console.WriteLine("{0}", received_data);

                    // if we received the proper connection request message from a phone...
                    if (received_data.StartsWith("Hello Audio-Mobile Address:"))
                    {
                        // parse message to array
                        string[] messageArray = received_data.Split(' ');

                        _phoneAddress = messageArray[3];
                        _phoneType = messageArray[5];

                        // connect tcp socket to phone.
                        // this call will also result,
                        //  via a callback,
                        //      in the host sending an assigned port number to the phone.
                        _streamSocket.BeginConnect(_phoneAddress, _handshakePort, new AsyncCallback(initialHostToPhoneConnectionCallback), null);
                        
                        // TODO: we are probably going to need to set up the sockets corressponding to the new connection on a separate thread
                        // add a new slot to the slot array
                        //_slotArray[_numberOfConnections] = new Slot("melody", _phoneAddress,  assignedPort, _numberOfConnections);

                        // TODO: we may want to keep a member thread array,
                        //          it might be easier to keep track of whether threads are alive or not....
                        // launch new thread based upon new Slot
                        //Thread thread = new Thread(new ThreadStart(_slotArray[_numberOfConnections].receiveSignal));
                        //thread.Start();
                    }
                }
            }
            catch (Exception e){    Console.WriteLine(e.ToString());    }
            handShakeListener.Close();
            return 0;
        }

        /// <summary>
        /// callback for when the host makes an initial TCP connection to a phone
        /// </summary>
        /// <param name="ar"></param>
        public static void initialHostToPhoneConnectionCallback(IAsyncResult ar)
        {
            int assignedPort = _startingPortNumber + _numberOfConnections;

            // assemble connection response
            string response = "Hello there, use port: " + assignedPort.ToString();
            byte[] responseArray = Encoding.ASCII.GetBytes(response);

            NetworkStream stream =  _streamSocket.GetStream();

            int y = responseArray.GetLength(0);

            //  TODO: we were able to trigger an index out of range exception when we fiddled around with firing connection requests from
            //                  the phone
            stream.BeginWrite(responseArray, 0, responseArray.GetLength(0), new AsyncCallback(portAssignWriteToPhoneCallback
                ), null);
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
    }
}
