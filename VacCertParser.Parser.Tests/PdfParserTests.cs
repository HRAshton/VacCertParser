using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace VacCertParser.Parser.Tests
{
    [TestFixture]
    public class PdfParserTests
    {
        [Test]
        public void ExpressIntegrationTest()
        {
            var certPdfContent = File.ReadAllBytes("test_hse_cert.pdf");

            GetStreams(certPdfContent, out var streams);

            var streamsToDecode = streams.Select(stream => (byte[]) stream.Clone()).ToArray();
            UnpackStreams(ref streamsToDecode);

            GetEncodedTextBlocks(streamsToDecode, out var encodedTextBlocks);
            ParseEncodedTextBlocks(encodedTextBlocks, out var encodedTexts);

            var transformsStream = streamsToDecode.Last();
            ParseTransforms(transformsStream, out var transforms);

            DecodeTexts(encodedTexts, transforms);
        }

        private void GetStreams(byte[] content, out byte[][] streams)
        {
            var method = typeof(PdfParser).GetMethod("GetStreams",
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException();
            streams = (byte[][]) method.Invoke(null, new object[] {content});

            Assert.AreEqual(PdfParserTestsConstants.RawStreamLengths.Length, streams.Length);

            for (var streamIndex = 0; streamIndex < PdfParserTestsConstants.RawStreamLengths.Length; streamIndex++)
            {
                var stream = streams[streamIndex];

                var streamLength = PdfParserTestsConstants.RawStreamLengths[streamIndex];
                var streamFactLength = stream.Length;
                Assert.AreEqual(streamLength, streamFactLength);

                var firstBytes = PdfParserTestsConstants.RawStreamsFirstBytes[streamIndex];
                for (var byteIndex = 0; byteIndex < firstBytes.Length; byteIndex++)
                {
                    Assert.AreEqual(firstBytes[byteIndex], stream[byteIndex]);
                }
            }

            Assert.True(true);
        }

        private void UnpackStreams(ref byte[][] streams)
        {
            var method = typeof(PdfParser).GetMethod("UnpackStreams",
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException();
            method.Invoke(null, new object[] {streams});

            Assert.AreEqual(PdfParserTestsConstants.UnpackedStreamLengths.Length, streams.Length);

            for (var streamIndex = 0; streamIndex < PdfParserTestsConstants.UnpackedStreamLengths.Length; streamIndex++)
            {
                var stream = streams[streamIndex];

                var streamLength = PdfParserTestsConstants.UnpackedStreamLengths[streamIndex];
                var streamFactLength = stream.Length;
                Assert.AreEqual(streamLength, streamFactLength);

                var firstBytes = PdfParserTestsConstants.UnpackedStreamsFirstBytes[streamIndex];
                for (var byteIndex = 0; byteIndex < firstBytes.Length; byteIndex++)
                {
                    Assert.AreEqual(firstBytes[byteIndex], stream[byteIndex]);
                }
            }
        }

        private void ParseTransforms(byte[] transformsStream, out Dictionary<ushort, char> transforms)
        {
            var method = typeof(PdfParser).GetMethod("ParseTransforms",
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException();
            transforms = (Dictionary<ushort, char>) method.Invoke(null, new object[] {transformsStream});

            Assert.AreEqual(115, transforms.Count);

            Assert.IsTrue(transforms.ContainsKey(138));
            Assert.AreEqual('К', transforms[138]);
        }

        private void GetEncodedTextBlocks(byte[][] streams, out byte[][] encodedTextBlocks)
        {
            var method = typeof(PdfParser).GetMethod("GetEncodedTextBlocks",
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException();
            encodedTextBlocks = (byte[][]) method.Invoke(null, new object[] {streams});
        }
        
        private void ParseEncodedTextBlocks(byte[][] textBlocks, out ushort[][] encodedTexts)
        {
            var method = typeof(PdfParser).GetMethod("ParseEncodedTextBlocks",
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException();
            encodedTexts = (ushort[][]) method.Invoke(null, new object[] {textBlocks});

            for (var i = 0; i < PdfParserTestsConstants.EncodedTextContent.Length; i++)
            {
                var textBlock = PdfParserTestsConstants.EncodedTextContent[i];
                var factTextBlock = encodedTexts[i];
                CollectionAssert.AreEqual(textBlock, factTextBlock);
            }
        }

        private void DecodeTexts(ushort[][] encodedTexts, Dictionary<ushort,char> transforms)
        {
            var method = typeof(PdfParser).GetMethod("DecodeTexts",
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException();
            var decodedLines = (string[]) method.Invoke(null, new object[] {encodedTexts, transforms});

            CollectionAssert.Contains(decodedLines, "ФГБУ НИЦЭМ ИМ. Н.Ф. ГАМАЛЕИ МИНЗДРАВА РОССИИ");
        }
    }
}