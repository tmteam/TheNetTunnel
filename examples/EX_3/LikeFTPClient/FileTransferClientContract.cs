namespace EX_3_FTPLikeClient
{
	[ProtoContract]
	public class pbFileInfo{
		[ProtoMember(1)] public string Name;
		[ProtoMember(2)] public DateTime Modified;
		[ProtoMember(3)] public UInt32 Size;
		[ProtoMember(4)] public bool IsDirrectory;

	}
	public enum ChangeDirrectoryResult:byte{
		Succesfully  = 0,
		InvalidPath  = 1,
		AccessDenied = 2,
	}

	public class FileTransferClientContract
	{
		public string CurrentDirrectory{ get; set;}

		public void UpdateCD()
		{
			CurrentDirrectory = GetCurrentDirrectoryPath ();
		}

		/// <summary>
		/// 1    BOOL   SendTextMessage          STRING: message
		/// </summary>
		/// <value>The send message.</value>
		[Out(1)] public Func<string, bool> SendMessage{ get; set;}
		/// <summary>
		/// <-2    BOOL   SendTextMessageToB       STRING: message
		/// </summary>
		/// <returns><c>true</c>, if message was received, <c>false</c> otherwise.</returns>
		/// <param name="message">Message.</param>
		[In (2)] public bool ReceiveMessage(string message){
			Console.WriteLine("[SERVER SAY] "+ message);
			return true;
		}
		/// <summary>
		/// 3   STRING  GetCurrentDirrectoryPath
		/// </summary>
		/// <value>The get current dirrectory path.</value>
		[Out(3)] public Func<string> GetCurrentDirrectoryPath { get; set;}
		/// <summary>
		/// 4   CDResult  ChangeDirrectoryTo      STRING: relativePath 
		/// Changing current directory   analog of unix’s «cd» command.
		/// Examples of relative path in windows:  «MyDocs», «MyDocs\work», «..\MyDocs», «..\», «C:\MyDocs»
		/// </summary>
		/// <value>The change current dirrectory.</value>
		[Out(4)] public Func<string,ChangeDirrectoryResult> ChangeCurrentDirrectory{get;set;}
		/// <summary>
		/// 5   FILEINFO[] GetCurrentDirrectoryContent
		/// return list of content in current directory. analog of unix’s «ls» command.
		/// </summary>
		/// <value>The content of the get current dirrectory.</value>
		[Out(5)] public Func<pbFileInfo[]> 					GetCurrentDirrectoryContent{ get; set;}
		/// <summary>
		/// 6     VOID   AbortFileTransmition     INT32: FileID
		/// </summary>
		/// <value>The abort file transmition.</value>
		[Out(6)] public Func<int> 							AbortFileTransmition{get;set;}

		/// <summary>
		/// <-7     VOID   AbortFileTransmitionToB  INT32: FileID
		/// </summary>
		/// <param name="fileID">File I.</param>
		[In (7)] public void HandleAbortTransmition(int fileID){
		}

		#region FileDownload
		//10   INT32  InitializeFileDownload    STRING: fileName    UINT32: maxPartSize
		[Out(10)] public Func<string,uint,int> InitializeFileDownload{ get; set;}
		//<-11   BOOL      SendFilePart            INT32: FileID      UINT32: bytesDone          UINT32:bytesLeft
		[In (11)] public bool ReceiveFilePart(int fileID, uint bytesDone, uint bytesLeft)
		{
			return false;
		}
		//12   BYTE[]    DownloadFilePart       STRING: fileName    UINT32: startByteNumber    UINT32:bytesCount
		[Out(12)] public Func<string, uint, uint, byte[]> DownloadFilePart{get;set;}
		#endregion

		#region FileUploading
		//20 INT32  InitializeFileUpload    STRING:fileName   UINT32: fileSize
		[Out(20)] public Func<string, uint,int> InitializeFileUpload{ get; set;}
		//<-21 BOOL   AcceptFileUpload        INT32: fileID     UINT32: maxFilePart
		[In (21)] public bool HandleAcceptFileUpload(Int32 fileID, uint maxFilePart)
		{
			return false;
	    }
		//<-22 VOID   RejectFileUpload        INT32: fileID
		[In (22)] public void HandleRejectFileUpload(Int32 fileID){  
				return ;
		}
		//23 BOOL   UploadFilePart          INT32: fileID     UINT32: bytesDone     UINT32: bytesLeft
		[Out(23)] Func<Int32, UInt32, UInt32, bool> UploadFilePart{get;set;}
		#endregion
		
	}
}
