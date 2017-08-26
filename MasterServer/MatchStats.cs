using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MasterServer{
    class MatchStats{
        public List<Client> clients { get; protected set; } = new List<Client>();
        public const int numberOfPlayersForMatch = 2;
        public MasterServer.Server server;
        public bool matched { get; protected set; }
        public MatchStats() {
            
        }

        /// <summary>
        /// Will add a client to the match
        /// </summary>
        /// <param name="toAdd"></param>
        /// <param name="message"></param>
        public void AddClient(TcpClient toAdd,string message, NetworkStream stream) {
            Client client = new Client(toAdd, message, stream);
            clients.Add(client);
            this.CheckList();
        }

        /// <summary>
        /// Will remove a client from the match, return true if removed one
        /// </summary>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public bool RemoveClient(TcpClient toRemove) {
            bool toRet = false;
            for(int i = 0; i < clients.Count; i++) {
                if (clients[i].client == toRemove) {
                    toRet = true;
                    clients.RemoveAt(i);
                    break;
                }
            }
            this.CheckList();
            return toRet;
        }

        /// <summary>
        /// Will remove disconnected clients, and if it is full will Start match
        /// </summary>
        protected void CheckList() {
            for(int i = 0; i < clients.Count; i++) {
                if (!clients[i].client.Connected) {
                    clients.RemoveAt(i);
                    i--;
                }
            }
            if(clients.Count == numberOfPlayersForMatch) {
                SendStartMatch();
            }
        }
        
        /// <summary>
        /// Will Send start match message
        /// </summary>
        protected void SendStartMatch() {
            this.matched = true;
            string ip = IPAddress.Any.ToString();
            int choosenPort = Server.GetFreePort(); ;
            string toSend = "Connect to:" + ip + ":" + choosenPort;
            for(int i = 0; i < clients.Count; i++) {
                toSend += clients[i].message + ";";
            }
            byte[] message = Encoding.UTF8.GetBytes(toSend);
            for (int i = 0; i < clients.Count; i++) {
                clients[i].stream.Write(message, 0, message.Length);
            }

            //debug
            Console.WriteLine("Matched!! " + toSend);
        }
    }
}
