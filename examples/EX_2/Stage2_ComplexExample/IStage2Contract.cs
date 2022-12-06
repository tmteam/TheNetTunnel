using System;
using ProtoBuf;
using TNT;

namespace EX_2.Stage2_ComplexExample;

/// <summary>
/// Interface (contract) for client server interaction
/// </summary>
public interface IStage2Contract
{
    [TntMessage(1)]
    bool TryAuthorize(string name, string password);

    [TntMessage(2)] 
    //Message type number 1. Returns message id. Throws if client is not authorized
    int Send(DateTime  sentTime, string message);

    [TntMessage(3)]
    //Server callback. Fires on new message received
    Action<ChatMessage> NewMessageReceived { get; set; }
        
}

/// <summary>
/// We are using protobuff structs here
/// </summary>
[ProtoContract]
public class ChatMessage
{
    [ProtoMember(1)]
    public DateTime Timestamp { get; set; }
    [ProtoMember(2)]
    public int MessageId { get; set; }
    [ProtoMember(3)]
    public string User { get; set; }

    [ProtoMember(4)]
    public string Message { get; set; }
}