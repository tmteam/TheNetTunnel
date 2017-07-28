using System;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;

namespace TNT.Presentation
{
    /// <summary>
    /// Incapsulates basic Tnt IO operations 
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// Sends "Say" message with "values" arguments
        /// </summary>
        ///<exception cref="ArgumentException">wrong message id</exception>
        ///<exception cref="TntCallException"></exception>
        ///<exception cref="LocalSerializationException">one of the argument type serializers is not implemented, or not the same as specified in the contract</exception>
        void Ask(short messageId, short askId, object[] arguments);

        /// <summary>
        /// Sends "Say" message with "values" arguments
        /// </summary>
        ///<exception cref="ArgumentException">wrong message id</exception>
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="LocalSerializationException">one of the argument type serializers is not implemented, or not the same as specified in the contract</exception>
        void Say(short messageId, object[] values);

        /// <summary>
        /// Sends "ans value" message 
        /// </summary>
        ///<exception cref="ArgumentException">wrong message id</exception>
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="LocalSerializationException">answer type serializer is not implemented, or  not the same as specified in the contract</exception>
        void Ans(short messageId, short askId, object value);
        
        /// <summary>
        /// Handles the error, occured during the input message handling.
        /// try to send an error message to remote side
        /// </summary>
        ///<exception cref="InvalidOperationException">Critical implementation exception</exception>
        void HandleRequestProcessingError(ErrorMessage errorInfo, bool isFatal);

        event Action<IMessenger, RequestMessage> OnRequest;

        event Action<IMessenger, short, short, object> OnAns;

        event Action<IMessenger, ErrorMessage> OnRemoteError;

        event Action<IMessenger, ErrorMessage> ChannelIsDisconnected;
    }
}
