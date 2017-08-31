using System;
using Example.Stage2Example;

namespace Example.Stage2_ComplexExample
{
    /// <summary>
    /// Server interaction contract implementation
    /// </summary>
    public class Stage2ContractImplementation : IStage2Contract
    {
        private readonly Server _server;
        private bool _isAuthorized = false;
        private string _name;
        public Stage2ContractImplementation(Server server)
        {
            _server = server;
        }
        //Callback
        public Action<ChatMessage> NewMessageReceived { get; set; }

        public int Send(DateTime sentTime, string message)
        {
            if (!_isAuthorized)
                throw new InvalidOperationException("User is not authorized");

            return _server.SendBroadCast(sentTime, _name, message);
        }

        public bool TryAuthorize(string name, string password)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(password))
            {
                _isAuthorized = true;
                _name = name;
            }
            return _isAuthorized;
        }
    }
}