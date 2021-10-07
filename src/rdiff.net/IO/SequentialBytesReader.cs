using System.IO;
using System.Text;

namespace rdiff.net
{
    public class SequentialBytesReader : IBytesReader
    {
        private readonly byte[] bytes;
        private Stream sourceStream;

        public SequentialBytesReader(string text)
        {
            this.bytes = Encoding.UTF8.GetBytes(text);
        }

        public SequentialBytesReader(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public bool GetNext(int size, out byte[] result)
        {
            result = new byte[size];
            var bytesRead = this.Source.Read(result, 0, size);
            result = result[..bytesRead]; // check if wont explode on 0

            return bytesRead > 0;
        }

        public int GetNextByte()
        {
            return this.Source.ReadByte();
        }

        private Stream Source
        {
            get
            {
                if (this.sourceStream == null)
                {
                    this.sourceStream = new MemoryStream(bytes);
                }

                return sourceStream;
            }
        }
    }
}
