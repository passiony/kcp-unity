using UnityEngine;

namespace ETModel
{
	public class OuterMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, ushort opcode, object message)
		{
			//消息
//			MessageInfo messageInfo = new MessageInfo(opcode, message);
			
			Debug.Log(opcode);
		}
	}
}
