using System;
using System.Text;
using UnityEngine;

namespace UNetwork
{
	public class OuterMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, byte[] buffer)
		{
			// ushort opcode = BitConverter.ToUInt16(buffer, Packet.OpcodeIndex);
			string opcode = Encoding.UTF8.GetString(buffer);
//			object message = this.Network.MessagePacker.DeserializeFrom(null, memoryStream);
			TestClient.Receive();
			Debug.Log("receive msg：" + opcode);
		}
	}
}
