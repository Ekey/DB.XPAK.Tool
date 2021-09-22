using System;
using System.IO;

namespace DB.Unpacker
{
    class LZMA
    {
        public static Byte[] iFixHeader(Byte[] lpBuffer, Byte[] lpLZMAProps, Int32 dwSize)
        {
            Byte[] result = new Byte[dwSize + 5 + 4]; // 5 lzma props + 4
            Byte[] reserved = new Byte[4];

            Array.Copy(lpLZMAProps, result, lpLZMAProps.Length);
            Array.Copy(lpBuffer, 0, result, 5, 4);
            Array.Copy(reserved, 0, result, 9, 4);
            Array.Copy(lpBuffer, 4, result, 13, lpBuffer.Length - 4);

            return result;
        }

        public static Byte[] iDecompress(Byte[] lpBuffer, Byte[] lpLZMAProps, Int32 dwSize)
        {
            Byte[] result;

            lpBuffer = iFixHeader(lpBuffer, lpLZMAProps, dwSize);
            using (MemoryStream TDstMemoryStream = new MemoryStream())
            {
                using (MemoryStream TSrcMemoryStream = new MemoryStream(lpBuffer))
                {
                    SevenZip.Compression.LZMA.Decoder LZMADecoder = new SevenZip.Compression.LZMA.Decoder();

                    // Read the decoder properties
                    Byte[] lpProperties = new Byte[5];
                    TSrcMemoryStream.Read(lpProperties, 0, 5);

                    // Read in the decompress file size.
                    Byte[] fileLength = new Byte[8];
                    TSrcMemoryStream.Read(fileLength, 0, 8);
                    Int64 dwDecompressedSize = BitConverter.ToInt64(fileLength, 0);

                    LZMADecoder.SetDecoderProperties(lpProperties);
                    LZMADecoder.Code(TSrcMemoryStream, TDstMemoryStream, TSrcMemoryStream.Length, dwDecompressedSize, null);

                    result = TDstMemoryStream.ToArray();
                }
            }
            return result;
        }
    }
}
