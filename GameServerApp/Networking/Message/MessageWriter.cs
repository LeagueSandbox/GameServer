using System;
using System.Collections.Generic;
using Soly.ByteArray;

namespace ROIDForumServer
{
    /*
     * Writes a message and produces a binary buffer.
     * Format:
     * length (4 bytes) + binary data (any size)
     */
    /*
      This is currently very slow. Use ideas from:
      https://github.com/brianc/node-buffer-writer/blob/master/index.js
    */
	public class MessageWriter
	{
		List<MessageData> dataArray;
		uint innerByteLength;
		public MessageWriter()
		{
			dataArray = new List<MessageData>();
			//Only the length of bytes being stored
			innerByteLength = 0;
		}

		public byte[] ToBuffer()
		{
			//Take length of all data and add the message length holder
			uint totalLength = this.innerByteLength + 4;
			ByteArray byteData = new ByteArray((int)totalLength);
			uint loc = 0;
			//Append the message length
			byteData.Write(totalLength, (int)loc, Endianess.BigEndian);
			loc += 4;
			//Append the message
			foreach (MessageData data in dataArray)
			{
				data.AddToByteData(byteData, (int)loc);
				loc += data.GetLength();
			}
			return byteData.Buffer;
		}

		public void AddUint8(byte value)
		{
			var data = new MessageDataUint8(value);
			this.dataArray.Add(data);
			this.innerByteLength += data.GetLength();
		}

		public void AddInt8(sbyte value)
		{
			var data = new MessageDataInt8(value);
			this.dataArray.Add(data);
			this.innerByteLength += data.GetLength();
		}

		public void AddUint16(ushort value)
		{
			var data = new MessageDataUint16(value);
			this.dataArray.Add(data);
			this.innerByteLength += data.GetLength();
		}

		public void AddInt16(short value)
		{
			var data = new MessageDataInt16(value);
			this.dataArray.Add(data);
			this.innerByteLength += data.GetLength();
		}

		public void AddUint32(uint value)
		{
			var data = new MessageDataUint32(value);
			this.dataArray.Add(data);
			this.innerByteLength += data.GetLength();
		}

		public void AddInt32(int value)
		{
			var data = new MessageDataInt32(value);
			this.dataArray.Add(data);
			this.innerByteLength += data.GetLength();
		}

		public void AddFloat64(double value)
		{
			var data = new MessageDataFloat64(value);
			this.dataArray.Add(data);
			this.innerByteLength += data.GetLength();
		}

		public void AddFloat32(float value)
		{
			var data = new MessageDataFloat32(value);
			this.dataArray.Add(data);
			this.innerByteLength += data.GetLength();
		}

		public void AddString(string value)
		{
			var data = new MessageDataString(value);
			this.dataArray.Add(data);
			this.innerByteLength += data.GetLength();
		}
        
		public void AddBinary(byte[] value)
		{
			var data = new MessageDataBinary(value);
			this.dataArray.Add(data);
			this.innerByteLength += data.GetLength();
		}

		public uint GetLength()
		{
			return this.innerByteLength + 4;
		}

	}
}