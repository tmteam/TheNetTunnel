﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Channel.Tcp
{
    public class TcpChannel: IChannel
    {
        public TcpChannel(TcpClient client)
        {
            Client = client;
        }
        public bool IsConnected { get { return Client != null && Client.Connected; } }
        /// <summary>
        /// Can LClient-user can handle messages now?.
        /// </summary>
        public bool AllowReceive
        {
            get { return allowReceive; }
            set
            {
                if (allowReceive != value)
                {
                    allowReceive = value;
                    if (value)
                    {
                        if (!readWasStarted)
                        {
                            readWasStarted = true;
                            NetworkStream networkStream = Client.GetStream();
                            byte[] buffer = new byte[Client.ReceiveBufferSize];

                            //start async read operation.
                            //IOException
                            networkStream.BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
                        }
                    }
                    if (!value)
                    {
                        throw new InvalidOperationException("cannot stop reading");
                    }
                }
            }
        }

        private bool allowReceive = false;
        private bool disconnectMsgWasSended = false;
        private bool readWasStarted = false;

        public TcpClient Client { get; }

        public event Action<IChannel, byte[]> OnReceive;
        public event Action<IChannel> OnDisconnect;
        public void Disconnect()
        {
            if (Client.Connected)
                disconnect();
        }
        public async Task<bool> TryWriteAsync(byte[] array)
        {
            var stream = Client.GetStream();
            try
            {
                var task =  stream.WriteAsync(array, 0, array.Length);
                await task;
                if(task.Exception==null)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        //public bool TryWrite(byte[] array)
        //{
        //    var stream = Client.GetStream();
        //    try
        //    {
        //        stream.Write(array, 0, array.Length);
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        return false;
        //    }
        //}
        public void Write(byte[] data)
        {
            if (!Client.Connected)
                return;
            NetworkStream networkStream = Client.GetStream();
            //Start async write operation
            networkStream.BeginWrite(data, 0, data.Length, writeCallback, null);
        }

        private void writeCallback(IAsyncResult result)
        {
            try
            {
                NetworkStream networkStream = Client.GetStream();
                networkStream.EndWrite(result);
            }
            catch
            {
                disconnect();
            }
        }


        private void readCallback(IAsyncResult result)
        {
            try
            {
                var networkStream = Client.GetStream();
                var bytesToRead = networkStream.EndRead(result);

                if (bytesToRead == 0)
                {
                    //The connection has been closed.
                    throw new Exception("Read of 0. Connection was closed");
                }

                var buffer = result.AsyncState as byte[];
                var readed = new byte[bytesToRead];
                //Array.Copy(buffer, readed, read);
                Buffer.BlockCopy(buffer, 0, readed, 0, bytesToRead);

                OnReceive?.Invoke(this, readed);

                //Start reading from the network again.
                networkStream.BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
            }
            catch
            {
                disconnect();
            }
        }


        private void disconnect()
        {
            if (Client.Connected)
                try
                {
                    Client.Close();
                }
                catch
                {
                }

            if (!disconnectMsgWasSended)
            {
                disconnectMsgWasSended = true;

                OnDisconnect?.Invoke(this);
            }
        }

       }
}