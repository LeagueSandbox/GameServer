using System;
using Soly.ByteArray;
namespace ROIDForumServer
{
	public abstract class MessageData
	{
		public abstract void AddToByteData(ByteArray byteData, int loc);
		public abstract uint GetLength();
	}

	public class MessageDataUint8 : MessageData
	{
		byte value;

		public MessageDataUint8(byte value)
		{
			this.value = value;
		}

		public override void AddToByteData(ByteArray byteData, int loc)
		{
			byteData.Write(this.value, loc, Endianess.BigEndian);
		}

		public override uint GetLength()
		{
			return 1;
		}
	}



	public class MessageDataInt8 : MessageData
	{
		sbyte value;

		public MessageDataInt8(sbyte value)
		{
			this.value = value;
		}

		public override void AddToByteData(ByteArray byteData, int loc)
		{
            byteData.Write(this.value, loc, Endianess.BigEndian);
		}

		public override uint GetLength()
		{
			return 1;
		}
	}
    
	public class MessageDataUint16 : MessageData
	{
		ushort value;

		public MessageDataUint16(ushort value)
		{
			this.value = value;
		}

		public override void AddToByteData(ByteArray byteData, int loc)
		{
            byteData.Write(this.value, loc, Endianess.BigEndian);
		}

		public override uint GetLength()
		{
			return 2;
		}
	}
    
	public class MessageDataInt16 : MessageData
	{
		short value;

		public MessageDataInt16(short value)
		{
			this.value = value;
		}

		public override void AddToByteData(ByteArray byteData, int loc)
		{
            byteData.Write(this.value, loc, Endianess.BigEndian);
		}

		public override uint GetLength()
		{
			return 2;
		}
	}

	public class MessageDataUint32 : MessageData
	{
		uint value;

		public MessageDataUint32(uint value)
		{
			this.value = value;
		}

		public override void AddToByteData(ByteArray byteData, int loc)
		{
            byteData.Write(this.value, loc, Endianess.BigEndian);
		}

		public override uint GetLength()
		{
			return 4;
		}
	}

	public class MessageDataInt32 : MessageData
	{
		int value;

		public MessageDataInt32(int value)
		{
			this.value = value;
		}

		public override void AddToByteData(ByteArray byteData, int loc)
		{
            byteData.Write(this.value, loc, Endianess.BigEndian);
		}

		public override uint GetLength()
		{
			return 4;
		}
	}

	public class MessageDataFloat32 : MessageData
	{
		float value;

		public MessageDataFloat32(float value)
		{
			this.value = value;
		}

		public override void AddToByteData(ByteArray byteData, int loc)
		{
            byte[] bytes = BitConverter.GetBytes(this.value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            
            byteData.Write(bytes, 0, (int)GetLength(), loc);
		}

		public override uint GetLength()
		{
			return 4;
		}
	}

	public class MessageDataFloat64 : MessageData
	{
		double value;

		public MessageDataFloat64(double value)
		{
			this.value = value;
		}

		public override void AddToByteData(ByteArray byteData, int loc)
		{
            byteData.Write(this.value, loc, Endianess.BigEndian);
		}

		public override uint GetLength()
		{
			return 8;
		}
	}

	public class MessageDataString : MessageData
	{
		byte[] value;
		readonly uint totalLength;

		public MessageDataString(string value)
		{
			this.value = System.Text.Encoding.Unicode.GetBytes(value);
			//Total length is buffer plus length of buffer
			this.totalLength = 4 + (uint)this.value.Length;
		}

		public override void AddToByteData(ByteArray byteData, int loc)
		{
			//Write total length (total length + string length)
			byteData.Write(this.totalLength, loc, Endianess.BigEndian);
            //Write string
			byteData.Write(value, 0, value.Length, loc + 4);
		}
        
		public override uint GetLength()
		{
			return this.totalLength;
		}
	}

	public class MessageDataBinary : MessageData
	{
		byte[] value;
		readonly uint totalLength;

		public MessageDataBinary(byte[] value)
		{
			this.value = value;
			//Total length is buffer plus length of buffer
			this.totalLength = 4 + (uint)this.value.Length;
		}

		public override void AddToByteData(ByteArray byteData, int loc)
		{
            //Write total length (total length + string length)
            byteData.Write(this.totalLength, loc, Endianess.BigEndian);
            //Write string
            byteData.Write(value, 0, value.Length, loc + 4);
		}

		public override uint GetLength()
		{
			return this.totalLength;
		}
	}
}