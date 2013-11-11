using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Transformer
{
    public class BufferedBinaryWriter : IDisposable
    {
        public const int OPTIMAL_SIZE = 128 * 1024 * 1024;

        BinaryWriter writer;
        readonly int size;
        byte[] array;
        int offset;

        public BufferedBinaryWriter(String filename)
            : this(filename, OPTIMAL_SIZE) { }

        public BufferedBinaryWriter(BinaryWriter binaryWriter)
            : this(binaryWriter, OPTIMAL_SIZE) { }

        public BufferedBinaryWriter(String filename, int bufferSize)
            : this(new BinaryWriter(new FileStream(filename, FileMode.Create)), bufferSize) { }

        public BufferedBinaryWriter(BinaryWriter binaryWriter, int bufferSize)
        {
            this.writer = binaryWriter;
            size = bufferSize;
            array = new byte[size];
            offset = -1;
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (offset >= 0)
            {
                writer.Write(array, 0, offset + 1);
                offset = -1;
            }

            writer.Dispose();
            array = null;
        }

        public void Write(byte value)
        {
            check(1);
            offset++;

            array[offset] = value;
        }

        public void Write(int value)
        {
            check(4);
            offset += 4;

            Array.Copy(BitConverter.GetBytes(value), 0, array, offset - 3, sizeof(int));
        }

        private void check(int neededSize)
        {
            if (offset + neededSize >= array.Length)
            {
                writer.Write(array, 0, offset + 1);
                offset = -1;
            }
        }
    }
}
