using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using Microsoft.IO;
using UnityEngine;

namespace Network
{
    public class WService: AService
    {
        private readonly HttpListener httpListener;

        private WChannel channel;
        
        public RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();

        public WService()
        {
        }
        
        public override AChannel GetChannel()
        {
            return channel;
        }

        public override AChannel ConnectChannel(IPEndPoint ipEndPoint)
        {
            throw new NotImplementedException();
        }

        public override AChannel ConnectChannel(string address)
        {
			ClientWebSocket webSocket = new ClientWebSocket();
            channel = new WChannel(webSocket, this);
            channel.ConnectAsync(address);
            return channel;
        }

        public override void Update()
        {
            
        }

        public override void Dispose()
        {
            
        }
        
    }
}