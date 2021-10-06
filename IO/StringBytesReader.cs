using System.IO;
using System.Text;

namespace rdiff.net
{
    public class StringBytesReader : IBytesReader
    {
        private readonly string text;
        private Stream sourceStream;

        public StringBytesReader(string text)
        {
            this.text = text;
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
                    this.sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(this.text));
                }

                return sourceStream;
            }
        }
    }
}
