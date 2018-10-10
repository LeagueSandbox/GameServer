﻿using System;

namespace LeagueSandbox.GameServer.Exceptions
{
    internal class ContentNotFoundException : Exception
    {
        public ContentNotFoundException() : base("The given content was not found.")
        {
        }

        public ContentNotFoundException(string message) : base(message)
        {
        }

        public ContentNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
