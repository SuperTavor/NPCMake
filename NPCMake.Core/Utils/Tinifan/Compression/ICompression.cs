namespace NPCMake.Core.Utils.Tinifan.Compression
{
    public interface ICompression
    {
        byte[] Compress(byte[] data);

        byte[] Decompress(byte[] data);
    }
}