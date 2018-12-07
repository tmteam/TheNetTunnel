namespace EX_3_FTPLikeClient
{
	public class CmdCenter
	{
		FileTransferClientContract contract;
		public CmdCenter(FileTransferClientContract contract)
		{
			Commands = new Dictionary<string, CmdBase> ();
			this.contract = contract;
		}

		public Dictionary<string, CmdBase> Commands{ get; protected set; }

		public 	void RegistrateCmd(CmdBase cmd)	{
			Commands.Add (cmd.Signature, cmd);
			cmd.Contract = contract;
		}

		public void RunCommand(string cmd)
		{
			if (string.IsNullOrEmpty (cmd))
				return;
			var arr = cmd.Split (new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if(!Commands.ContainsKey(arr[0].ToLower()))
				Console.WriteLine("unknown command \""+ arr[0]+"\"");
			else
				Commands[arr[0].ToLower()].Run(cmd.Remove(0,arr[0].Length).Trim());
		}
	}
}

