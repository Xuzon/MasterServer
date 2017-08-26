using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MasterServer{
    public class Client {
        public TcpClient client { get; protected set; }
        public string message { get; protected set; }
        public NetworkStream stream { get; protected set; }

        public Client(TcpClient client, string message, NetworkStream stream) {
            this.client = client;
            this.message = message;
            this.stream = stream;
        }
    }
}
