using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace MasterServer { 
    class Server{
        protected static Server serv;
        protected TcpListener server;
        protected bool exit = false;

        static void Main(string[] args) {
            serv = new Server();
        }

        protected Server(int port = 12995) {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
            server = new TcpListener(endPoint);
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

        /// <summary>
        /// Tcp Connection for matchmaking
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private static async Task Connection(TcpClient client) {
            Console.WriteLine("Connection accepted");
            try {
                using (NetworkStream stream = client.GetStream()) {
                    //message buffer
                    byte[] buffer = new byte[1024];
                    //flag to see if removed out of the cpu
                    bool removed = false;
                    //added match
                    MatchStats match = null;
                    while (client.Connected) {
                        //read message
                        int len = await stream.ReadAsync(buffer, 0, buffer.Length);
                        //check if has to close connection or not
                        if (match != null && !match.matched) {
                            break;
                        }
                        string incomingMessage = Encoding.UTF8.GetString(buffer, 0, len);
                        //if is select match add the player
                        if (incomingMessage.StartsWith("SelectMatch")) {
                            match = Server.AddPlayerToQueue(client, incomingMessage, stream);
                        }

                        //if remove me remove me from match
                        if (incomingMessage.StartsWith("RemoveMe")) {
                            match?.RemoveClient(client);
                            removed = true;
                            break;
                        }
                        Thread.Sleep(100);
                    }
                    //client has disconnected and wasn't manually removed so remove its match
                    if (!removed) {
                        match?.RemoveClient(client);
                        lock (Matches) {
                            if (match?.clients?.Count == 0) {
                                Matches.Remove(match);
                            }
                        }
                    }
                    Console.WriteLine("Connection ended");
                    client.Dispose();
                }
            } catch (Exception ex) {
                client.Dispose();
                Console.WriteLine("Exception message" + ex.Message);
            }
        }

        public static List<MatchStats> Matches { get; protected set; } = new List<MatchStats>();
        /// <summary>
        /// Add player to match and creat a match if needed
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected static MatchStats AddPlayerToQueue(TcpClient client, string message,NetworkStream stream) {
            lock (Matches) {
                MatchStats match;
                for (int i = 0; i <= Matches.Count; i++) {
                    //if I reached end of list create a new match
                    if (i == Matches.Count) {
                        match = new MatchStats();
                        Matches.Add(match);
                    }
                    //if current match is not matched, and is not full add client
                    if (!Matches[i].matched && Matches[i].clients.Count < MatchStats.numberOfPlayersForMatch) {
                        match = Matches[i];
                        match.AddClient(client, message, stream);
                        return match;
                    }
                }
                return null;
            }
        }

        public static int GetFreePort() {
            int toRet = 0;
            Random rand = new Random();
            toRet = rand.Next();
            return toRet;
        }
    }
}