using System;
using System.IO;

namespace rdiff.net
{
    public class FileBytesReader : IBytesReader, IDisposable
    {
        private readonly FileInfo fileInfo;
        private Stream sourceStream;

        public FileBytesReader(FileInfo file)
        {
            if(file == null)
            {
                throw new ArgumentException($"{nameof(file)} cannot be null.", nameof(file));
            }

            this.fileInfo = file;
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
           
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.sourceStream?.Dispose();
            }
        }

        private Stream Source
        {
            get 
            {
                if (this.sourceStream == null)
                {
                    this.sourceStream = this.fileInfo.OpenRead();
                }

                return sourceStream;
            }
        }
    }
}
