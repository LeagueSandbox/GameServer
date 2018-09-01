using System;
using Soly.ByteArray;
namespace ROIDForumServer
{
	public class MessageReader
	{
		private uint currentLoc;
		private ByteArray byteData;
		private readonly uint byteLength;

		public MessageReader(byte[] messageData)
		{
			this.byteData = new ByteArray(messageData);
			this.currentLoc = 0;
			this.byteLength = this.GetUint32();
			//Throw an error if the message is an incorrect length
			if (this.byteLength != messageData.Length)
			{
				throw new Exception("Message Incorrect Length");
			}
		}
		public bool IsAtEndOfData()
		{
			return this.byteLength == this.currentLoc;
		}

		public bool HasUint8()
		{
			return (this.currentLoc + 1) <= this.byteLength;
		}

		public byte GetUint8()
		{
			var data = this.byteData.ReadU8((int)this.currentLoc, Endianess.BigEndian);
			this.currentLoc += 1;
			return data;
		}

		public bool HasInt8()
		{
			return (this.currentLoc + 1) <= this.byteLength;
		}

		public sbyte GetInt8()
		{
			var data = this.byteData.ReadI8((int)this.currentLoc, Endianess.BigEndian);

			this.currentLoc += 1;
			return data;

		}

		public bool HasUint16()
		{
			return (this.currentLoc + 2) <= this.byteLength;
		}

		public ushort GetUint16()
		{
			var data = this.byteData.ReadU16((int)this.currentLoc, Endianess.BigEndian);

			this.currentLoc += 2;
			return data;

		}

		public bool HasInt16()
		{
			return (this.currentLoc + 2) <= this.byteLength;
		}

		public short GetInt16()
		{
			var data = this.byteData.ReadI16((int)this.currentLoc, Endianess.BigEndian);

			this.currentLoc += 2;
			return data;

		}

		public bool HasUint32()
		{
			return (this.currentLoc + 4) <= this.byteLength;
		}

		public uint GetUint32()
		{
			var data = this.byteData.ReadU32((int)this.currentLoc, Endianess.BigEndian);
			this.currentLoc += 4;
			return data;
		}

		public bool HasInt32()
		{
			return (this.currentLoc + 4) <= this.byteLength;
		}

		public int GetInt32()
		{
			var data = this.byteData.ReadI32((int)this.currentLoc, Endianess.BigEndian);
			this.currentLoc += 4;
			return data;
		}

		public double GetFloat64()
		{
			var data = this.byteData.ReadF64((int)this.currentLoc, Endianess.BigEndian);
			this.currentLoc += 8;
			return data;
		}

		public bool HasFloat64()
		{
			return (this.currentLoc + 8) <= this.byteLength;
		}

		public bool HasFloat32()
		{
			return (this.currentLoc + 4) <= this.byteLength;
		}

		public float GetFloat32()
		{
			var data = this.byteData.ReadF32((int)this.currentLoc, Endianess.BigEndian);
			this.currentLoc += 4;
			return data;
		}

		public bool HasString()
		{
			var length = this.byteData.ReadU32((int)this.currentLoc, Endianess.BigEndian);
			return (this.currentLoc + length) <= this.byteLength;
		}

		public string GetString()
		{
			var length = this.byteData.ReadU32((int)this.currentLoc, Endianess.BigEndian);
			this.currentLoc += 4;
			var innerLength = length - 4;

			var byteArray = new byte[innerLength];
			this.byteData.Read(byteArray, 0, (int)innerLength, (int)this.currentLoc);         
			var returnString = System.Text.Encoding.Unicode.GetString(byteArray);
			this.currentLoc += innerLength;
			return returnString;
		}

		public bool HasBinary()
		{
			var length = this.byteData.ReadU32((int)this.currentLoc, Endianess.BigEndian);
			return (this.currentLoc + length) <= this.byteLength;
		}

		public byte[] GetBinary()
		{
			var length = this.byteData.ReadU32((int)this.currentLoc, Endianess.BigEndian);
			this.currentLoc += 4;
			var innerLength = length - 4;
            var byteArray = new byte[innerLength];
            this.byteData.Read(byteArray, 0, (int)innerLength, (int)this.currentLoc);  
			this.currentLoc += innerLength;
			return byteArray;
		}
        
		public uint GetLength()
		{
			return this.byteLength;
		}
	}
}