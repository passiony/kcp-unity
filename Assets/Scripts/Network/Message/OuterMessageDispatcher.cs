﻿using System;
using UnityEngine;

namespace Network
{
	public class OuterMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, byte[] buffer)
		{
			ushort opcode = BitConverter.ToUInt16(buffer, Packet.OpcodeIndex);
//			object message = this.Network.MessagePacker.DeserializeFrom(null, memoryStream);
			TestClient.Receive();
			Debug.Log("receive msg：" + opcode);
		}
	}
}
