using System;
using System.Text;
using UNetwork;
using UnityEngine;

public class TestServer : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 12346;
    
    public string sendMessage = "server";
    public static long starttime = 0;

    void Start()
    {
        ServerManager client = ServerManager.Instance;
        client.InitService(NetworkProtocol.TCP);
        client.MessagePacker = new ProtobufPacker();
        client.MessageDispatcher = new OuterMessageDispatcher();

        client.Connect(ip, port);
        client.OnConnect += OnConnect;
        client.OnError += OnError;
        client.OnMessage += OnMessage;
    }

    private void OnMessage(byte[] obj)
    {
        var msg = Encoding.UTF8.GetString(obj);
        Debug.Log(msg);
    }

    private void OnError(int e)
    {
        Debug.LogError("网络错误：" + e);
    }

    private void OnConnect(int c)
    {
        Debug.Log("连接成功");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var data = Encoding.UTF8.GetBytes(sendMessage);
            Debug.Log($"Send{data.Length}:" + sendMessage);

            ServerManager.Instance.Send(data);
            starttime = GetTimeStamp();
        }
    }

    /// <summary>
    /// 获取时间戳
    /// </summary>
    /// <returns></returns>
    public static long GetTimeStamp()
    {
        return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
    }

    public static void Receive()
    {
        var inteval = GetTimeStamp() - starttime;
        Debug.LogWarning(inteval);
    }
}