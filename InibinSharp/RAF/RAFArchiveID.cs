#region LICENSE

// Copyright 2014 - 2014 InibinSharp
// RAFArchiveID.cs is part of InibinSharp.
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
    ///     Struct to hold the id number of an archive
    /// </summary>
    public class RAFArchiveID
    {
        private RAFArchiveID(int a, int b, int c, int d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public int A { get; private set; }
        public int B { get; private set; }
        public int C { get; private set; }
        public int D { get; private set; }

        protected bool Equals(RAFArchiveID other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;

            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        ///     Factory to create an ArchiveID object
        /// </summary>
        /// <param name="inputStr">
        ///     Input string to be parsed into an ID. Should follow the pattern: 'A.B.C.D' If less arguments are
        ///     present, ie. 'A.B', they will be assumed to be the highest order
        /// </param>
        /// <returns>The created ArchiveID object</returns>
        public static RAFArchiveID CreateID(String inputStr)
        {
            var split = inputStr.Split('.');
            if (split.Length == 1)
            {
                int a;
                if (Int32.TryParse(split[0], out a))
                {
                    return new RAFArchiveID(a, 0, 0, 0);
                }
            }
            else if (split.Length == 2)
            {
                int a, b;
                if (Int32.TryParse(split[0], out a) &&
                    Int32.TryParse(split[1], out b))
                {
                    return new RAFArchiveID(a, b, 0, 0);
                }
            }
            else if (split.Length == 3)
            {
                int a, b, c;
                if (Int32.TryParse(split[0], out a) &&
                    Int32.TryParse(split[1], out b) &&
                    Int32.TryParse(split[2], out c))
                {
                    return new RAFArchiveID(a, b, c, 0);
                }
            }
            else if (split.Length == 4)
            {
                int a, b, c, d;
                if (Int32.TryParse(split[0], out a) &&
                    Int32.TryParse(split[1], out b) &&
                    Int32.TryParse(split[2], out c) &&
                    Int32.TryParse(split[3], out d))
                {
                    return new RAFArchiveID(a, b, c, d);
                }
            }

            throw new Exception("ArchiveID construction arguments couldn't be parsed to an int");
        }

        /// <summary>
        ///     Factory to create an ArchiveID object
        /// </summary>
        /// ///
        /// <param name="a">The highest order number</param>
        /// <param name="b">The second order number</param>
        /// <param name="c">The third order number</param>
        /// <param name="d">the lowest order number</param>
        /// <returns>The created ArchiveID object</returns>
        public static RAFArchiveID CreateID(int a, int b = 0, int c = 0, int d = 0)
        {
            return new RAFArchiveID(a, b, c, d);
        }

        public static bool operator ==(RAFArchiveID a, RAFArchiveID b)
        {
            if (null == a || null == b)
                throw new ArgumentNullException();

            return a.A == b.A &&
                   a.B == b.B &&
                   a.C == b.C &&
                   a.D == b.D;
        }

        public static bool operator !=(RAFArchiveID a, RAFArchiveID b)
        {
            if (null == a || null == b)
                throw new ArgumentNullException();

            return a.A != b.A ||
                   a.B != b.B ||
                   a.C != b.C ||
                   a.D != b.D;
        }

        public static bool operator <(RAFArchiveID a, RAFArchiveID b)
        {
            if (null == a || null == b)
                throw new ArgumentNullException();

            // If A is less, we can quit
            if (a.A < b.A)
                return true;
            // Check greater than so we can quit early
            if (a.A > b.A)
                return false;
            // Equality on A, check down the line in the same fashion
            if (a.B < b.B)
                return true;
            if (a.B > b.B)
                return false;
            if (a.C < b.C)
                return true;
            if (a.C > b.C)
                return false;

            return a.D < b.D;
        }

        public static bool operator >(RAFArchiveID a, RAFArchiveID b)
        {
            // If A is greater, we can quit
            if (a.A > b.A)
                return true;
            // Check less than so we can quit early
            if (a.A < b.A)
                return false;
            // Equality on A, check down the line in the same fashion
            if (a.B > b.B)
                return true;
            if (a.B < b.B)
                return false;
            if (a.C > b.C)
                return true;
            if (a.C < b.C)
                return false;

            return a.D > b.D;
        }

        public override String ToString()
        {
            return A + "." + B + "." + C + "." + D;
        }
    }
}