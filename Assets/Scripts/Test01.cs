using System;
using System.Collections;
using System.Collections.Generic;
using ETModel;
using UnityEngine;

public class Test01 : MonoBehaviour
{
    private string address = "127.0.0.1:12346";
    
    void Start()
    {
        NetworkManager network = NetworkManager.Instance;
        network.InitService(NetworkProtocol.TCP);
        network.MessagePacker = new ProtobufPacker();
        network.MessageDispatcher = new OuterMessageDispatcher();
        
        network.Connect(address);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            NetworkManager.Instance.Send(2201);
        }
    }
}
