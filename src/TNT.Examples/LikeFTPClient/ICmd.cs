using System;
using System.Linq;

namespace SomeFTPlikeClient_Example
{

	public abstract class CmdBase
	{
		public abstract void Run (string arg);

		public FileTransferClientContract Contract {get;set;}
		public string Signature { get; protected set;}
	}

	public class SendMessage: CmdBase{
		public SendMessage(){ Signature = "msg";}
		public override void Run (string arg){
			Contract.SendMessage(arg);
		}
	}

	public class GetCurrentDirrectory: CmdBase{
		public GetCurrentDirrectory(){
			Signature = "cur";
		}
		public override void Run (string arg)
		{	
			Contract.UpdateCD();
			Console.WriteLine ("Current dirrectory: " + Contract.CurrentDirrectory);
		}
	}

	public class ChangeDirrectory:CmdBase{
		public ChangeDirrectory(){ Signature = "cd";}
		public override void Run (string arg)
		{
			var res = Contract.ChangeCurrentDirrectory(arg);
			if (res != ChangeDirrectoryResult.Succesfully) {
				Console.WriteLine ("Cannot change current dirrectory because of " + res);
			}
		}
	}

	public class GetDirContent: CmdBase{
		public GetDirContent(){ Signature = "ls";}
		public override void Run (string arg)
		{
			var cont = Contract.GetCurrentDirrectoryContent();
			if (cont == null)
				Console.WriteLine ("LS - Error. Got null content");
			else {
				var sorted = cont.OrderBy (c => c.Size).ToArray ();
				Console.WriteLine ("[" + cont.Length + "] Content of " + Contract.CurrentDirrectory + " : ");
				foreach(var c in sorted)
				{
					Console.Write("\t"+ c.Name+"\t\t");
					if(c.IsDirrectory) 
						Console.Write("[DIRRECTORY]\r\n");
					else
						Console.Write(" "+ c.Size+"b\t\t "+ c.Modified.ToShortTimeString()+"\r\n");
				}
			}
		}
	}
	public class GetFullFile: CmdBase{
		public GetFullFile(){ Signature = "get";}
		public override void Run (string arg)
		{
			var fi = Contract.GetCurrentDirrectoryContent ().FirstOrDefault (f => f.Name == arg);

			if (fi == null) {
				Console.WriteLine ("File " + arg + " not found");
				return;
			} else if (fi.IsDirrectory) {
				Console.WriteLine (arg + " is a dirrectory");
				return;
			}
				else {
				try
				{
				Console.WriteLine ("Downloading " + arg + " with size " + fi.Size);
				var bytearr = Contract.DownloadFilePart(arg, 0, fi.Size);
					if(bytearr==null)
					{
						Console.WriteLine("Cannot get file");
						return;
					}
				var curdir = System.IO.Directory.GetCurrentDirectory();
				Console.WriteLine(bytearr.Length+" downloaded. Saving to "+ curdir);
				System.IO.File.WriteAllBytes(curdir+"/"+ arg, bytearr);
				Console.WriteLine("saved");
				}
				catch(Exception ex) {
					Console.WriteLine ("Ex: " + ex.ToString ());
				}
			}
	
		}
	}
}

