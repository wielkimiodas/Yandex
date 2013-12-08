using System;
using System.IO;

namespace Yandex.Utils
{
    public class BufferedBinaryReader : IDisposable
    {
        public const int OPTIMAL_SIZE = 128*1024;

        private BinaryReader reader;
        private readonly int size;
        private byte[] array;
        private int offset;
        private int toRead;
        private bool ready = true;

        public BufferedBinaryReader(String filename)
            : this(filename, OPTIMAL_SIZE)
        {
        }

        public BufferedBinaryReader(BinaryReader binaryReader)
            : this(binaryReader, OPTIMAL_SIZE)
        {
        }

        public BufferedBinaryReader(String filename, int bufferSize)
            : this(new BinaryReader(new FileStream(filename, FileMode.Open)), bufferSize)
        {
        }

        public BufferedBinaryReader(BinaryReader binaryReader, int bufferSize)
        {
            this.reader = binaryReader;
            size = (int) Math.Min(bufferSize, binaryReader.BaseStream.Length);
            array = new byte[size];
            offset = size - 1;
        }

        public void Close()
        {
            reader.Close();
            array = null;
        }

        public void Dispose()
        {
            reader.Dispose();
            array = null;
        }

        public int PeekChar()
        {
            check(1);

            if ((!ready) && (toRead - (offset + 1)) <= 0)
                return -1;

            return array[offset + 1];
        }

        public bool ReadBool()
        {
            check(1);
            offset++;

            return BitConverter.ToBoolean(array, offset);
        }

        public byte ReadByte()
        {
            check(1);
            offset++;

            return array[offset];
        }

        public int ReadInt32()
        {
            check(4);
            offset += 4;
            return BitConverter.ToInt32(array, offset - 3);
        }

        private void check(int neededSize)
        {
            if (offset + neededSize >= array.Length)
            {
                if (offset < array.Length - 1)
                    Array.Copy(array, offset + 1, array, 0, size - offset - 1);

                byte[] bytes = reader.ReadBytes(offset + 1);
                Array.Copy(bytes, 0, array, size - offset - 1, bytes.Length);
                ready = reader.BaseStream.Position != reader.BaseStream.Length;
                toRead = size - offset - 1 + bytes.Length;
                offset = -1;
            }
        }
    }
}