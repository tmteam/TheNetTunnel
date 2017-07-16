﻿using System;

namespace Expirements.General
{
    public class Connection<TContract, TChannel>: IDisposable 
        //where TChannel: IChannel
    {
        public TContract Contract { get; }
        public TChannel Channel { get; }
        public void Dispose()
        {
        }
    }
}