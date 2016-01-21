using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freedompeace.RiotArchive
{
    /// <summary>
    /// The exception that is thrown when an unsupported file is opened.
    /// </summary>
    public class FileFormatIncorrectException : Exception
    {
        internal FileFormatIncorrectException()
        {
        }
    }
}
