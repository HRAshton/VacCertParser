using System;
using System.IO;
using System.IO.Compression;

namespace VacCertParser.Parser
{
    public class GzipHelper
    {
        public static byte[] DecompressData(byte[] inData)
        {
            byte[] cutinput = new byte[inData.Length - 2];
            Array.Copy(inData, 2, cutinput, 0, cutinput.Length);

            var stream = new MemoryStream();

            using (var compressStream = new MemoryStream(cutinput))
            using (var decompressor = new DeflateStream(compressStream, CompressionMode.Decompress))
                decompressor.CopyTo(stream);

            return stream.ToArray();
        }
    }
}