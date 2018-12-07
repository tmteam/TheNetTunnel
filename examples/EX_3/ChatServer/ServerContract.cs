namespace EX_3_ChatServer
{
	public class ServerContract
	{
        [In(3)]  public event Func<int, int> Question;
	    [In(1)]  public event Action<DateTime, string, string> ReceiveMessage;
		[Out(2)] public Func<DateTime, string, string, bool> SendMessage{get;set;}
	}
}

