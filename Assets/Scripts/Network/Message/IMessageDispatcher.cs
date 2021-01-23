namespace Network
{
	public interface IMessageDispatcher
	{
		void Dispatch(Session session, byte[] buffer);
	}
}
