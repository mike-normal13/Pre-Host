﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace PreHost
{
    // TODO:    this class will be created 
    //              and run on a seperate thread whenever a successfull connection is made between the host and a phone.
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
        private int _sendingPort = 9999;
        //private UdpClient _sendingSocket = null;
        private UdpClient _receivingSocket = null;
        
        //  TODO: this may not be neccessary....
        // connection number
        private int _id = -1;

        private bool stop = false;

        public Slot(string type, string address, int receivingPort, int id)
        {
            _type = type;
            _phoneAddress = address;
            _receivingPort = receivingPort;
            _id = id;                       // TODO: may get scrapped.

            //_sendingSocket = new UdpClient(_phoneAddress, _sendingPort);
            //  TODO: it would be tidy if the receving socket for any given slot would only receive data from the phone it was assigned to...
            _receivingSocket = new UdpClient(_phoneAddress, _receivingPort);    // SUSPect
        }

        public void receiveSignal()
        {
            //  TODO: not sure what effect having BeginReceive will have at this point....
            //while(!stop)
            //{
                _receivingSocket.BeginReceive(new AsyncCallback(receiveSignalCallback), this);
            //}
        }

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