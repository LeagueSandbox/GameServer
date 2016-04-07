using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueLib.Files
{
    public class InibinWriter
    {

        private Inibin _inibin;
        private BinaryWriter _writer;
        private MemoryStream _stream;

        //private uint _stringTableLength;
        //private BitArray _format;

        public InibinWriter() { }

        public byte[] SerializeInibin(Inibin inibin)
        {
            _inibin = inibin;
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);

            //_inibin.Version = _reader.ReadByte();
            //_stringTableLength = _reader.ReadUInt16();

            if (_inibin.Version != 2)
                throw new InvalidDataException("Wrong Inibin version");

            //_format = new BitArray(new byte[] { _reader.ReadByte(), _reader.ReadByte() });

            //for (int i = 0; i < _format.Length; i++)
            //{
            //    if (_format[i])
            //    {
            //        if (!SerializeSegment(i))
            //            return null;
            //    }
            //}

            return _stream.ToArray();
        }

        private bool SerializeSegment(int type, bool skipErrors = true)
        {
            //int count = _reader.ReadUInt16();
            var count = 0;
            uint[] keys = DeserializeKeys(count);
            InibinValue[] values = new InibinValue[count];

            if (type == 0) // Unsigned integers
            {
                for (int i = 0; i < count; i++)
                {
                    //values[i] = new InibinValue(type, _reader.ReadUInt32());
                }
            }
            else if (type == 1) // Floats
            {
                for (int i = 0; i < count; i++)
                {
                    //values[i] = new InibinValue(type, _reader.ReadSingle());
                }
            }
            else if (type == 2) // One byte floats - Divide the byte by 10
            {
                for (int i = 0; i < count; i++)
                {
                    //values[i] = new InibinValue(type, (float)(_reader.ReadByte() * 0.1f));
                }
            }
            else if (type == 3) // Unsigned shorts
            {
                for (int i = 0; i < count; i++)
                {
                    //values[i] = new InibinValue(type, _reader.ReadUInt16());
                }
            }
            else if (type == 4) // Bytes
            {
                for (int i = 0; i < count; i++)
                {
                    //values[i] = new InibinValue(type, _reader.ReadByte());
                }
            }
            else if (type == 5) // Booleans
            {
                byte[] bytes = new byte[(int)Math.Ceiling((decimal)count / 8)];
                //_reader.BaseStream.Read(bytes, 0, bytes.Length);
                BitArray bits = new BitArray(bytes);

                for (int i = 0; i < count; i++)
                {
                    values[i] = new InibinValue(type, bits[i]);
                }
            }
            else if (type == 6) // 3x byte values - Resource bar RGB color
            {
                byte[] bytes = new byte[3];

                for (int i = 0; i < count; i++)
                {
                    //_reader.BaseStream.Read(bytes, 0, bytes.Length);
                    values[i] = new InibinValue(type, BitConverter.ToUInt32(new byte[4] { 0, bytes[0], bytes[1], bytes[2] }, 0));
                }
            }
            else if (type == 7) // 3x float values ??????
            {
                if (!skipErrors)
                    throw new Exception("Reading 12 byte values not yet supported");

                //Console.WriteLine("Tried to read 12 byte values");
                for (int i = 0; i < count; i++)
                {
                    // 4 + 4 + 4 = 12
                    //_reader.ReadInt32();
                    //_reader.ReadInt32();
                    //_reader.ReadInt32();
                    values[i] = new InibinValue(type, "NotYetImplemented");
                }
            }
            else if (type == 8) // 1x short ??????
            {
                for (int i = 0; i < count; i++)
                {
                    //values[i] = new InibinValue(type, _reader.ReadUInt16());
                }
            }
            else if (type == 9) // 2x float ??????
            {
                for (int i = 0; i < count; i++)
                {
                    //values[i] = new InibinValue(type, _reader.ReadUInt64());
                }
            }
            else if (type == 10) // 4x bytes * 0.1f ?????
            {
                for (int i = 0; i < count; i++)
                {
                    //values[i] = new InibinValue(type, _reader.ReadUInt32());
                }
            }
            else if (type == 11) // 4x float ?????
            {
                if (!skipErrors)
                    throw new Exception("Reading 16 byte values not yet supported");

                //Console.WriteLine("Tried to read 16 byte values");
                //for (int i = 0; i < count; i++)
                //{
                //    // 8 + 8 = 16
                //    _reader.ReadUInt64();
                //    _reader.ReadUInt64();
                //    values[i] = new InibinValue(type, "NotYetImplemented");
                //}
            }
            else if (type == 12) // Unsigned short - string dictionary offsets
            {
                //long stringListOffset = _reader.BaseStream.Length - _stringTableLength;

                //for (int i = 0; i < count; i++)
                //{
                //    int offset = _reader.ReadInt16();
                //    values[i] = new InibinValue(type, DeserializeString(stringListOffset + offset));
                //}
            }
            else
            {
                if (!skipErrors)
                    throw new Exception("Unknown segment type");

                Console.WriteLine(string.Format("Unknown segment type found in file {0}", _inibin.FilePath));
                return false;
            }

            for (int i = 0; i < keys.Length; i++)
            {
                _inibin.Content.Add(keys[i], values[i]);
            }

            return true;
        }

        private uint[] DeserializeKeys(int count)
        {
            //uint[] result = new uint[count];

            //for (int i = 0; i < result.Length; i++)
            //{
            //    result[i] = _reader.ReadUInt32();
            //}

            //return result;
            return null;
        }

        private string DeserializeString(long offset)
        {
            //long oldPosition = _reader.BaseStream.Position;
            //_reader.BaseStream.Seek(offset, SeekOrigin.Begin);

            //string result = "";
            //int character = _reader.ReadByte();
            //while (character > 0)
            //{
            //    result += (char)character;
            //    character = _reader.ReadByte();
            //}

            //_reader.BaseStream.Seek(oldPosition, SeekOrigin.Begin);

            //return result;

            return "";
        }
    }
}
