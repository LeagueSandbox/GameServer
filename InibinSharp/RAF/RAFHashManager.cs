#region LICENSE

// Copyright 2014 - 2014 InibinSharp
// RAFHashManager.cs is part of InibinSharp.
// InibinSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// InibinSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with InibinSharp. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;

#endregion

namespace InibinSharp.RAF
{
    /// <summary>
    ///     Manages the handling of hashes for RAF Strings, which is calculated in an unknown
    ///     matter at the moment.
    /// </summary>
    public static class RAFHashManager
    {
        /// <summary>
        ///     Get the hash of a entry file name
        /// </summary>
        /// <param name="s">Entry file name</param>
        /// <returns></returns>
        public static UInt32 GetHash(string s)
        {
            //if (hashes == null) Init();
            //Console.WriteLine("Calc hash of: " + s);
            /* Ported from documented code in RAF Documentation:
             * 
	         *      const char* pStr = 0;
	         *      unsigned long hash = 0;
	         *      unsigned long temp = 0;
             *
	         *      for(pStr = pName; *pStr; ++pStr)
	         *      {
		     *          hash = (hash << 4) + tolower(*pStr);
		     *          if (0 != (temp = hash & 0xf0000000)) 
		     *          {
			 *              hash = hash ^ (temp >> 24);
			 *              hash = hash ^ temp;
		     *          }
	         *      }
	         *      return hash;
             */
            UInt32 hash = 0;
            UInt32 temp = 0;
            for (var i = 0; i < s.Length; i++)
            {
                hash = (hash << 4) + s.ToLower()[i];
                if (0 != (temp = (hash & 0xF0000000)))
                {
                    hash = hash ^ (temp >> 24);
                    hash = hash ^ temp;
                }
            }
            //Console.WriteLine("!");

            //Console.WriteLine("Hash expected: " + hashes[s]);
            //Console.WriteLine("Hash Calculated: " + hash);
            return hash;
        }
    }
}