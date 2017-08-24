using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MasterServer { 
    class Server{
        protected static Server serv;
        protected TcpListener server;
        protected bool exit = false;

        static void Main(string[] args) {
            serv = new Server();
        }

        protected Server(int iport = 12995, string address = "127.0.0.1") {
            Int32 port = iport;
            IPAddress addr = IPAddress.Parse(address);
            server = new TcpListener(addr, port);
            server.Start();
            while (!exit) {
                ServerLoop().Wait();
            }
        }

        protected async Task ServerLoop() {
            TcpClient client = await server.AcceptTcpClientAsync();
            Thread thread = new Thread(async () => await Connection(client));
            thread.Start();
        }

        private static async Task Connection(TcpClient client) {
            Console.WriteLine("Connection accepted");
            try {
                using (NetworkStream stream = client.GetStream()) {
                    byte[] buffer = new byte[1024];
                    int len = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string incomingMessage = Encoding.UTF8.GetString(buffer, 0, len);
                    Console.WriteLine("Incoming message: {0}", incomingMessage);


                    Console.WriteLine("Sending message.");
                    byte[] message = Encoding.UTF8.GetBytes("Thank you!");
                    await stream.WriteAsync(message, 0, message.Length);
                    Console.WriteLine("Closing connection.");
                }
            } catch (Exception ex) {
                Console.WriteLine("Exception message" + ex.Message);
            }
        }
    }
}