using System;
using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;

public class TestClient : MonoBehaviour
{
    public string address = "127.0.0.1:12346";
//    private string address = "10.200.10.192:3655";

    public static long starttime = 0;

    void Start()
    {
        ClientManager client = ClientManager.Instance;
        client.InitService(NetworkProtocol.KCP);
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
            ClientManager.Instance.Send(2201);
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
