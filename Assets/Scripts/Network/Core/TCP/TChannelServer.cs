using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace UNetwork
{
    /// <summary>
    /// 封装 Server 端Socket监听
    /// </summary>
    public sealed class TChannelServer : AChannel
    {
        private Socket listener;
        private List<TClientConnection> clientConnections = new List<TClientConnection>();
        private SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();

        private readonly PacketParser parser;

        private readonly TServiceServer service;
		
        private bool isSending;

        private bool isRecving;

        public TChannelServer(IPEndPoint ipEndPoint, TServiceServer service) : base(service, ChannelType.Accept)
        {
            this.service = service;
            this.memoryStream = service.MemoryStreamManager.GetStream("server", ushort.MaxValue);

            this.listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(ipEndPoint);
            listener.Listen(10); // 最大挂起连接队列的长度
            this.listener.NoDelay = true;
            
            Debug.Log("开启监听：" + ipEndPoint);
            this.acceptArgs.Completed += this.OnAcceptComplete;
        }

        public void Update()
        {
            foreach (var connection in clientConnections)
            {
                connection.Update();
            }
        }
        
        public override void Send(MemoryStream stream)
        {
            foreach (var connection in clientConnections)
            {
                connection.Send(stream);
            }
        }

        public override void Dispose()
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

            Debug.Log("客户端连接成功：" + e.RemoteEndPoint);
            Socket clientSocket = e.AcceptSocket;

            TClientConnection clientConnection = new TClientConnection(clientSocket, this.service);
            clientConnections.Add(clientConnection);

            clientConnection.Start();
            clientConnection.OnDisposeCallback=OnDisconnect;
            // Start accepting next client
            this.AcceptAsync();
        }

        public void OnDisconnect(TClientConnection connection)
        {
            clientConnections.Remove(connection);
        }
        
    }
}