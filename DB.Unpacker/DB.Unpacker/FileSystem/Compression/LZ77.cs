using System;
using System.IO;
using System.IO.Compression;
using Brotli;

namespace DB.Unpacker
{
    class Brotli_LZ77
    {
        public static Byte[] iDecompress(Byte[] lpBuffer)
        {
            Byte[] temp = new Byte[lpBuffer.Length - 4];
            Array.Copy(lpBuffer, 4, temp, 0, lpBuffer.Length - 4);

            using (MemoryStream TSrcMemoryStream = new MemoryStream(temp))
            using (BrotliStream TBrotliStream = new BrotliStream(TSrcMemoryStream, CompressionMode.Decompress))
            using (MemoryStream TDstMemoryStream = new MemoryStream())
            {
                TBrotliStream.CopyTo(TDstMemoryStream);
                TDstMemoryStream.Seek(0, SeekOrigin.Begin);
                var result = TDstMemoryStream.ToArray();
                return result;
            }
        }
    }
}
