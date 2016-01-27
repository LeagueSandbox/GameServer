#region

using System;

#endregion

namespace ENet
{
    public class ENetException : Exception
    {
        public ENetException(int code, string message)
            : base(message)
        {
            Code = code;
        }

        public int Code { get; private set; }
    }
}