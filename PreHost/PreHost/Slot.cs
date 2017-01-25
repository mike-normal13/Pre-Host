using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace PreHost
{
    //  TODO: this class needs to handle the creation of the Melody, Percussion, and Sampler contexts.
    class Slot
    {
        // melody, percussion, or sampler
        private string _type = "";
        // ip address of the phone
        private string _phoneAddress = "";
        // this port is assigned by the host depeinding on how many phones are connected.
        private int _receivingPort = 0;

        private IPEndPoint _receivingEndPoint = null;
        
        // the port the phone will be listening on,
        //  all phones will be listening on 9999 for now.
        private int _sendingPort = 10000;
        
        private UdpClient _sendingSocket = null;
        private UdpClient _receivingSocket = null;
        
        //  TODO: this may not be neccessary....
        // connection number
        private int _id = -1;

        // TODO: stop condition is unimplemented.
        private bool stop = false;

        public Slot(string type, string address, int receivingPort, int id)
        {
            _type = type;
            _phoneAddress = address;
            _receivingPort = receivingPort;
            _id = id;                       // TODO: may get scrapped.

            _sendingSocket = new UdpClient(_phoneAddress, _sendingPort);
            //  TODO: it would be tidy if the receving socket for any given slot would only receive data from the phone it was assigned to...
            _receivingSocket = new UdpClient(_phoneAddress, _receivingPort);    // SUSPect
        }

        public void setUpPlayConnection()
        {
            string startPlayMessage = "hey, host is ready for play signals on port: " + _receivingPort.ToString();
            byte[] startPlayMessageBytes = Encoding.ASCII.GetBytes(startPlayMessage);
           _sendingSocket.Connect(_phoneAddress, _sendingPort); 
            _sendingSocket.Send(startPlayMessageBytes, startPlayMessageBytes.Length);

            receivePlaySignal();
        }

        //  TODO: would having separate receive methods for the three phone contexts reduce latency????
        //  TODO: this method is unimplemented.
        //  TODO: how are we going to handle the three phone contexts here???,
        //                      we'll start with the Melody Maker since it will probably be the simplest to trigger.
        /// <summary>
        /// method to receive play/control signals from the phone
        /// </summary>
        public void receivePlaySignal()
        {
            //  TODO: not sure what effect having BeginReceive will have at this point....
            while(!stop)
            {
                //  TODO: running this call in a loop causes a stack overflow, or the like
                //_receivingSocket.BeginReceive(new AsyncCallback(receiveSignalCallback), this);
                int y = 9;

                //  TODO: receive, then parse the signal
            }
        }

        /// <summary>
        /// Thread is launced off of this method.
        /// it sends the message back to the phone stating that the host is ready to receive play/control signals from the phone.
        /// </summary>
        /// <param name="ar"></param>
        public void receiveSignalCallback(IAsyncResult ar)
        {
            byte[] addressBytes = Encoding.ASCII.GetBytes(((Slot)ar)._phoneAddress);
            IPAddress ipAddress = new IPAddress(addressBytes);
            IPEndPoint ep = new IPEndPoint(ipAddress, ((Slot)ar)._receivingPort);
            byte[] receivedBytes = ((Slot)ar)._receivingSocket.EndReceive(ar, ref ep);

            string receivedString = Encoding.ASCII.GetString(receivedBytes);

            Console.WriteLine("receiveSignalCallback received string: " + receivedString);
        }
    }
}
