using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Network;
using UnityEngine;

public class TestServer : MonoBehaviour
{
    public string address = "127.0.0.1:12346";
//    private string address = "10.200.10.192:3655";

    public static long starttime = 0;

    void Start()
    {
        ServerManager client = ServerManager.Instance;
        client.InitService(NetworkProtocol.TCP);
        client.MessagePacker = new ProtobufPacker();
        client.MessageDispatcher = new OuterMessageDispatcher();

        client.Connect(address);
        client.OnConnect += OnConnect;
        client.OnError += OnError;
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
            var msg = "你好,我是Server";
            var data = Encoding.UTF8.GetBytes(msg);
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
