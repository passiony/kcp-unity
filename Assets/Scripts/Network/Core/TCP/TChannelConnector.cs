using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace UNetwork
{
    /// <summary>
    /// 管理客户端连接的类
    /// </summary>
    public sealed class TClientConnection : AChannel
    {
        private int PacketSizeLength;

        private readonly Socket clientSocket;
        private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
        private readonly SocketAsyncEventArgs outArgs = new SocketAsyncEventArgs();

        private readonly CircularBuffer recvBuffer = new CircularBuffer();
        private readonly CircularBuffer sendBuffer = new CircularBuffer();

        private readonly PacketParser parser;
        private readonly byte[] packetSizeCache;

        private bool isConnected;
        private bool isSending;
        private bool isRecving;

        private readonly SessionConnector session;
        public UnityAction<TClientConnection> OnDisposeCallback;

        public TClientConnection(Socket clientSocket, TServiceServer service) : base(service, ChannelType.Connect)
        {
            this.session = new SessionConnector(this);

            this.clientSocket = clientSocket;
            this.memoryStream = service.MemoryStreamManager.GetStream("client", ushort.MaxValue);

            int packetSize = service.PacketSizeLength;
            this.PacketSizeLength = packetSize;
            this.packetSizeCache = new byte[packetSize];
            this.parser = new PacketParser(packetSize, this.recvBuffer, this.memoryStream);

            this.innArgs.Completed += this.OnComplete;
            this.outArgs.Completed += this.OnComplete;
            isConnected = false;
        }

        public override void Dispose()
        {
            OnDisposeCallback?.Invoke(this);
            this.clientSocket.Close();
            this.innArgs.Dispose();
            this.outArgs.Dispose();
            this.memoryStream.Dispose();
        }

        public override void Start()
        {
            if (!this.isRecving)
            {
                this.isRecving = true;
                this.StartRecv();
                this.isConnected = true;
            }
        }

        public void Update()
        {
            if (isSending)
            {
                return;
            }

            try
            {
                StartSend();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void OnComplete(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    OneThreadSynchronizationContext.Instance.Post(this.OnRecvComplete, e);
                    break;
                case SocketAsyncOperation.Send:
                    OneThreadSynchronizationContext.Instance.Post(this.OnSendComplete, e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    OneThreadSynchronizationContext.Instance.Post(this.OnDisconnectComplete, e);
                    break;
                default:
                    throw new Exception($"socket error: {e.LastOperation}");
            }
        }

        private void OnDisconnectComplete(object o)
        {
            SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;
            this.OnError((int)e.SocketError);
        }

        private void StartRecv()
        {
            int size = this.recvBuffer.ChunkSize - this.recvBuffer.LastIndex;
            this.RecvAsync(this.recvBuffer.Last, this.recvBuffer.LastIndex, size);
        }

        public void RecvAsync(byte[] buffer, int offset, int count)
        {
            try
            {
                this.innArgs.SetBuffer(buffer, offset, count);
            }
            catch (Exception e)
            {
                throw new Exception($"socket set buffer error: {buffer.Length}, {offset}, {count}", e);
            }

            if (this.clientSocket.ReceiveAsync(this.innArgs))
            {
                return;
            }

            OnRecvComplete(this.innArgs);
        }

        private void OnRecvComplete(object o)
        {
            if (this.clientSocket == null)
            {
                return;
            }

            SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;

            if (e.SocketError != SocketError.Success)
            {
                this.OnError((int)e.SocketError);
                return;
            }

            if (e.BytesTransferred == 0)
            {
                this.OnError(ErrorCode.ERR_PeerDisconnect);
                return;
            }

            this.recvBuffer.LastIndex += e.BytesTransferred;
            if (this.recvBuffer.LastIndex == this.recvBuffer.ChunkSize)
            {
                this.recvBuffer.AddLast();
                this.recvBuffer.LastIndex = 0;
            }

            // 收到消息回调
            while (true)
            {
                try
                {
                    if (!this.parser.Parse())
                    {
                        break;
                    }
                }
                catch (Exception ee)
                {
                    Debug.LogError($"Client {clientSocket.RemoteEndPoint} {ee}");
                    this.OnError(ErrorCode.ERR_SocketError);
                    return;
                }

                try
                {
                    this.OnRead(this.parser.GetPacket());
                }
                catch (Exception ee)
                {
                    Debug.LogError(ee);
                }
            }

            this.StartRecv();
        }

        public override void Send(MemoryStream stream)
        {
            switch (PacketSizeLength)
            {
                case Packet.PacketSizeLength4:
                    if (stream.Length > ushort.MaxValue * 16)
                    {
                        throw new Exception($"send packet too large: {stream.Length}");
                    }

                    this.packetSizeCache.WriteTo(0, (int)stream.Length);
                    break;
                case Packet.PacketSizeLength2:
                    if (stream.Length > ushort.MaxValue)
                    {
                        throw new Exception($"send packet too large: {stream.Length}");
                    }

                    this.packetSizeCache.WriteTo(0, (ushort)stream.Length);
                    break;
                default:
                    throw new Exception("packet size must be 2 or 4!");
            }

            this.sendBuffer.Write(this.packetSizeCache, 0, this.packetSizeCache.Length);
            this.sendBuffer.Write(stream);
        }

        public void StartSend()
        {
            if (!this.isConnected)
            {
                return;
            }

            // 没有数据需要发送
            if (this.sendBuffer.Length == 0)
            {
                this.isSending = false;
                return;
            }

            if (!this.isSending)
            {
                this.isSending = true;
            }

            int sendSize = this.sendBuffer.ChunkSize - this.sendBuffer.FirstIndex;
            if (sendSize > this.sendBuffer.Length)
            {
                sendSize = (int)this.sendBuffer.Length;
            }

            this.SendAsync(this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize);
        }

        public void SendAsync(byte[] buffer, int offset, int count)
        {
            try
            {
                this.outArgs.SetBuffer(buffer, offset, count);
            }
            catch (Exception e)
            {
                throw new Exception($"socket set buffer error: {buffer.Length}, {offset}, {count}", e);
            }

            if (this.clientSocket.SendAsync(this.outArgs))
            {
                return;
            }

            OnSendComplete(this.outArgs);
        }

        private void OnSendComplete(object o)
        {
            SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;

            if (e.SocketError != SocketError.Success)
            {
                this.OnError((int)e.SocketError);
                return;
            }

            if (e.BytesTransferred == 0)
            {
                this.OnError(ErrorCode.ERR_PeerDisconnect);
                return;
            }

            this.sendBuffer.FirstIndex += e.BytesTransferred;
            if (this.sendBuffer.FirstIndex == this.sendBuffer.ChunkSize)
            {
                this.sendBuffer.FirstIndex = 0;
                this.sendBuffer.RemoveFirst();
            }

            this.StartSend();
        }

        private new void OnError(int error)
        {
            Debug.LogError($"Socket error: {error}");
            this.Dispose();
        }

        private void OnMessage(byte[] obj)
        {
            var msg = Encoding.UTF8.GetString(obj);
            Debug.Log($"Receive{obj.Length}" + msg);
        }
    }
}