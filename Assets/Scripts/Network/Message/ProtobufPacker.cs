using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Network
{
	public class ProtobufPacker : IMessagePacker
	{
		public byte[] SerializeTo(object obj)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream rems = new MemoryStream();
			formatter.Serialize(rems, obj);
			return rems.GetBuffer();
		}

		public void SerializeTo(object obj, MemoryStream stream)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, obj);
		}

		public object DeserializeFrom(Type type, byte[] bytes, int index, int count)
		{
			return null;
//			return ProtobufHelper.FromBytes(type, bytes, index, count);
		}

		public object DeserializeFrom(object instance, byte[] bytes, int index, int count)
		{
			return null;
//			return ProtobufHelper.FromBytes(instance, bytes, index, count);
		}

		public object DeserializeFrom(Type type, MemoryStream stream)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			return formatter.Deserialize(stream);
		}
		
		public object DeserializeFrom(object instance, MemoryStream stream)
		{
//			return ProtobufHelper.FromStream(instance, stream);
			return null;
		}
	}
}
