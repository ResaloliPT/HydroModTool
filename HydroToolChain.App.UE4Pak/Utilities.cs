using System.IO.Compression;

namespace HydroToolChain.App.UE4Pak;

public static class Utilities
{
    /// <summary>
    /// Compressed a chunk of data using zLib
    /// </summary>
    /// <see cref="https://yal.cc/cs-deflatestream-zlib/">zLib Format</see>
    /// <param name="bytesToCompress"></param>
    /// <returns></returns>
    public static byte[] CompressChunk(byte[] bytesToCompress)
    {
        var compressedChunk = new List<byte>();
        //Set compression level 6 (Default for DeflateStream)
        compressedChunk.Add(0x78); // ZLIB CMF
        compressedChunk.Add(0x9C); // ZLIB FLG

        using var compressedBuffer = new MemoryStream();
        using var deflateStream = new DeflateStream(compressedBuffer, CompressionMode.Compress);
        deflateStream.Write(bytesToCompress, 0, bytesToCompress.Length);
        deflateStream.Close();
        compressedChunk.AddRange(compressedBuffer.ToArray()); // ZLIB compressed data

        compressedChunk.AddRange(BitConverter.GetBytes(Adler32Checksum(bytesToCompress)).Reverse()); // ZLIB Adler32 Checksum Reversed

        return compressedChunk.ToArray();
    }
    
    private static int Adler32Checksum(byte[] blockData)
    {
        var num = 1;
        var num2 = 0;
        foreach (var b in blockData)
        {
            num = (num + b) % 65521;
            num2 = (num2 + num) % 65521;
        }
        return num2 << 16 | num;
    }
}