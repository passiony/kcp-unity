using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using UnityEngine;

namespace Network
{
    public class WChannel: AChannel
    {
        public HttpListenerWebSocketContext WebSocketContext { get; }

        private readonly WebSocket webSocket;

        private readonly Queue<byte[]> queue = new Queue<byte[]>();

        private bool isSending;

        private bool isConnected;

        private readonly MemoryStream memoryStream;

        private readonly MemoryStream recvStream;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public WChannel(HttpListenerWebSocketContext webSocketContext, AService service): base(service, ChannelType.Accept)
        {
            this.WebSocketContext = webSocketContext;

            this.webSocket = webSocketContext.WebSocket;

            this.memoryStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);
            this.recvStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);

            isConnected = true;
        }

        public WChannel(WebSocket webSocket, AService service): base(service, ChannelType.Connect)
        {
            this.webSocket = webSocket;

            this.memoryStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);
            this.recvStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);

            isConnected = false;
        }

        public void Dispose()
        {
            this.cancellationTokenSource.Cancel();
            this.cancellationTokenSource.Dispose();
            this.cancellationTokenSource = null;

            this.webSocket.Dispose();

            this.memoryStream.Dispose();
        }

        public override MemoryStream Stream
        {
            get
            {
                return this.memoryStream;
            }
        }

        public override void Start()
        {
            if (!this.isConnected)
            {
                return;
            }

            this.StartRecv();
            this.StartSend();
        }

        private WService GetService()
        {
            return (WService) this.Service;
        }

        public async void ConnectAsync(string url)
        {
            try
            {
                await ((ClientWebSocket) this.webSocket).ConnectAsync(new Uri(url), cancellationTokenSource.Token);
                isConnected = true;
                this.Start();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                this.OnError(ErrorCode.ERR_WebsocketConnectError);
            }
        }

        public override void Send(MemoryStream stream)
        {
            byte[] bytes = new byte[stream.Length];
            Array.Copy(stream.GetBuffer(), bytes, bytes.Length);
            this.queue.Enqueue(bytes);

            if (this.isConnected)
            {
                this.StartSend();
            }
        }

        public async void StartSend()
        {
            try
            {
                if (this.isSending)
                {
                    return;
                }

                this.isSending = true;

                while (true)
                {
                    if (this.queue.Count == 0)
                    {
                        this.isSending = false;
                        return;
                    }

                    byte[] bytes = this.queue.Dequeue();
                    try
                    {
                        await this.webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Binary, true, cancellationTokenSource.Token);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        this.OnError(ErrorCode.ERR_WebsocketSendError);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async void StartRecv()
        {
            try
            {
                while (true)
                {

                    WebSocketReceiveResult receiveResult;
                    int receiveCount = 0;
                    do
                    {
                        receiveResult = await this.webSocket.ReceiveAsync(
                            new ArraySegment<byte>(this.recvStream.GetBuffer(), receiveCount, this.recvStream.Capacity - receiveCount), 
                            cancellationTokenSource.Token);
                        receiveCount += receiveResult.Count;
                    }
                    while (!receiveResult.EndOfMessage);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        this.OnError(ErrorCode.ERR_WebsocketPeerReset);
                        return;
                    }

                    if (receiveResult.Count > ushort.MaxValue)
                    {
                        await this.webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, $"message too big: {receiveResult.Count}",
                            cancellationTokenSource.Token);
                        this.OnError(ErrorCode.ERR_WebsocketMessageTooBig);
                        return;
                    }

                    this.recvStream.SetLength(receiveResult.Count);
                    this.OnRead(this.recvStream);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                this.OnError(ErrorCode.ERR_WebsocketRecvError);
            }
        }
    }
}