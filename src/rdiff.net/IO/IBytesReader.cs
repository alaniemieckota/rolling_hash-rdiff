namespace rdiff.net
{
    public interface IBytesReader
    {
        bool GetNext(int size, out byte[] result);

        int GetNextByte();
    }
}
