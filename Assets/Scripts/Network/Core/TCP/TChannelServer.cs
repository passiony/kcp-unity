using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.IO;
using UnityEngine;

namespace Network
{
    /// <summary>
    /// 封装Socket,将回调push到主线程处理
    /// </summary>
    public sealed class TChannelServer : AChannel
    {
        private Socket listener;
        private List<TClientConnection> clientConnections = new List<TClientConnection>();
        private SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();

        private readonly MemoryStream memoryStream;
        public override MemoryStream Stream=>this.memoryStream;

        private readonly PacketParser parser;

        private readonly TServiceServer service;
        
        public TChannelServer(IPEndPoint ipEndPoint, TServiceServer service) : base(service, ChannelType.Connect)
        {
            this.service = service;
            int packetSize = service.PacketSizeLength;
            this.memoryStream = service.MemoryStreamManager.GetStream("server", ushort.MaxValue);
            
            this.listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(ipEndPoint);
            listener.Listen(10); // 最大挂起连接队列的长度
            this.listener.NoDelay = true;

            this.acceptArgs.Completed += this.OnAcceptComplete;
        }

        public override void Send(MemoryStream stream)
        {
            foreach (var connection in clientConnections)
            {
                connection.Send(stream);
            }
        }

        public void Dispose()
        {
            this.listener.Close();
            foreach (var clientConnection in this.clientConnections)
            {
                clientConnection.Dispose();
            }
            this.acceptArgs.Dispose();
            this.listener = null;
        }

        public override void Start()
        {
            this.AcceptAsync();
        }

        private void AcceptAsync()
        {
            acceptArgs.AcceptSocket = null; // Reset AcceptSocket
            listener.AcceptAsync(acceptArgs);
        }

        private void OnAcceptComplete(object sender, SocketAsyncEventArgs e)
        {
            if (this.listener == null)
            {
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                this.OnError((int)e.SocketError);
                return;
            }

            Socket clientSocket = e.AcceptSocket;

            TClientConnection clientConnection = new TClientConnection(clientSocket, this.service);
            clientConnections.Add(clientConnection);

            clientConnection.Start();

            // Start accepting next client
            this.AcceptAsync();
        }
    }
}