using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freedompeace.RiotArchive
{
    internal class RiotArchiveSearchEnumerator : IEnumerator<RiotArchiveFile>
    {
        readonly IEnumerator<RiotArchiveFile> enumerator;
        readonly Predicate<string> predicate;

        public RiotArchiveSearchEnumerator(IEnumerator<RiotArchiveFile> enumerator, Predicate<string> predicate)
        {
            this.enumerator = enumerator;
            this.predicate = predicate;
        }

        public void Dispose()
        {
            enumerator.Dispose();
        }

        public bool MoveNext()
        {
            while (enumerator.MoveNext())
            {
                if (predicate.Invoke(enumerator.Current.FullName))
                    return true;
            }
            return false;
        }

        public void Reset()
        {
            enumerator.Reset();
        }

        object IEnumerator.Current { get { return Current; } }
        public RiotArchiveFile Current { get { return enumerator.Current; } }
    }
}
