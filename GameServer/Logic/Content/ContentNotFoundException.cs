using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Content
{
    class ContentNotFoundException : Exception
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
