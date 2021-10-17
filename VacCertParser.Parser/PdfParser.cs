using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VacCertParser.Parser.Extensions;

namespace VacCertParser.Parser
{
    /// <summary>
    /// Парсер PDF.
    /// </summary>
    internal class PdfParser
    {
        private static readonly byte[] Stream = Encoding.ASCII.GetBytes("stream\n");
        private static readonly byte[] EndStream = Encoding.ASCII.GetBytes("\nendstream");
        private static readonly byte[] Tj = Encoding.ASCII.GetBytes("\n(");
        private static readonly byte[] EndTj = Encoding.ASCII.GetBytes(")Tj\n");

        /// <summary>
        /// Распарсить PDF по фрагментам и получить все текстовые части.
        /// </summary>
        /// <param name="fileContent">Содержимое PDF.</param>
        /// <returns>Строки, найденные в документе.</returns>
        public string[] Parse(byte[] fileContent)
        {
            var streams = GetStreams(fileContent);
            UnpackStreams(streams);

            if (streams.Length == 0)
            {
                return new string[0];
            }

            var transformStream = streams.Last();
            var transforms = ParseTransforms(transformStream);

            var encodedTextBlocks = GetEncodedTextBlocks(streams);
            var parsedTextBlocks = ParseEncodedTextBlocks(encodedTextBlocks);
            var text = DecodeTexts(parsedTextBlocks, transforms);

            return text;
        }

        /// <summary>
        /// Получить потоки.
        /// </summary>
        /// <param name="fileContent">Содержимое файла.</param>
        /// <returns>Потоки PDF.</returns>
        private static byte[][] GetStreams(byte[] fileContent)
        {
            var streams = new List<byte[]>();

            var previousStreamIndex = 0;
            var streamBlock = FindNextStream(fileContent, ref previousStreamIndex);
            while (streamBlock != null)
            {
                streams.Add(streamBlock);

                streamBlock = FindNextStream(fileContent, ref previousStreamIndex);
            }

            return streams.ToArray();
        }

        /// <summary>
        /// Распаковать потоки.
        /// </summary>
        /// <param name="streams">Потоки PDF.</param>
        private static void UnpackStreams(byte[][] streams)
        {
            for (var index = 0; index < streams.Length; index++)
            {
                try
                {
                    streams[index] = GzipHelper.DecompressData(streams[index]);
                }
                catch
                {
                    streams[index] = new byte[0];
                }
            }
        }

        /// <summary>
        /// Получить содержимое таблицы преобразования символов.
        /// </summary>
        /// <param name="stream">Поток с таблицей преобразования символов.</param>
        /// <returns>Словарь, в ключах которого коды символов до преобразования, в значениях - после преобразования.</returns>
        private static Dictionary<ushort, char> ParseTransforms(byte[] stream)
        {
            var regex = new Regex("<([0-9a-f]{4})><([0-9a-f]{4})><([0-9a-f]{4})>",
                RegexOptions.Compiled | RegexOptions.Singleline);
            var text = Encoding.ASCII.GetString(stream);
            var matches = regex.Matches(text);

            var transforms = new Dictionary<ushort, char>();
            foreach (Match match in matches)
            {
                var from = match.Groups[2].Value;
                var to = match.Groups[3].Value;

                transforms.Add(Convert.ToUInt16(from, 16), (char) Convert.ToUInt16(to, 16));
            }

            return transforms;
        }

        /// <summary>
        /// Получить зашифрованное содержимое текстовых блоков ( (...)Tj ).
        /// </summary>
        /// <param name="streams">Потоки.</param>
        /// <returns>Содержимое текстовых блоков.</returns>
        private static byte[][] GetEncodedTextBlocks(byte[][] streams)
        {
            var textBlocks = new List<byte[]>();

            foreach (var stream in streams)
            {
                var previousTextBlockEndIndex = 0;
                var textBlock = FindTextBlock(stream, ref previousTextBlockEndIndex);
                while (textBlock != null)
                {
                    textBlocks.Add(textBlock);

                    textBlock = FindTextBlock(stream, ref previousTextBlockEndIndex);
                }
            }

            return textBlocks.ToArray();
        }

        /// <summary>
        /// Преобразовать содержимое щашифрованных блоков текста из byte[] в ushort[] по 2 байта.
        /// </summary>
        /// <param name="textBlocks">Содержимое текстовых блоков.</param>
        /// <returns>Содержимое текстовых блоков.</returns>
        private static ushort[][] ParseEncodedTextBlocks(byte[][] textBlocks)
        {
            var encodedTexts = new ushort[textBlocks.Length][];

            for (var i = 0; i < textBlocks.Length; i++)
            {
                var textBlock = textBlocks[i];
                var decodedBlock = encodedTexts[i] = new ushort[textBlock.Length / 2];

                for (var j = 0; j < encodedTexts[i].Length; j++)
                {
                    decodedBlock[j] = (ushort) ((textBlock[j * 2] << 8) + textBlock[j * 2 + 1]);
                }
            }

            return encodedTexts;
        }

        /// <summary>
        /// Расшифровать содержимое текстовых блоков.
        /// </summary>
        /// <param name="encodedTexts">Содержимое текстовых блоков</param>
        /// <param name="transforms">Таблица преобразований символов.</param>
        /// <returns>Строки, найденные в документах.</returns>
        private static string[] DecodeTexts(ushort[][] encodedTexts, Dictionary<ushort, char> transforms)
        {
            var text = new StringBuilder();
            foreach (var encodedText in encodedTexts)
            {
                foreach (var chr in encodedText)
                {
                    var correctedChar = transforms.TryGetValue(chr, out var rep) ? rep : chr;
                    text.Append(char.ConvertFromUtf32(correctedChar));
                }

                text.AppendLine();
            }

            return text.ToString().Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
        }

        private static byte[] FindNextStream(byte[] fileContent, ref int previousStreamEndIndex)
        {
            return FindNextSection(fileContent, ref previousStreamEndIndex, Stream, EndStream);
        }

        private static byte[] FindTextBlock(byte[] streamContent, ref int previousTextBlockEndIndex)
        {
            return FindNextSection(streamContent, ref previousTextBlockEndIndex, Tj, EndTj);
        }

        private static byte[] FindNextSection(byte[] content, ref int prevPosition, byte[] begin, byte[] end)
        {
            var index1 = content.IndexOf(begin, prevPosition);
            if (index1 == -1) return null;

            var index2 = content.IndexOf(end, index1);
            if (index2 == -1) return null;

            prevPosition = index2 + end.Length;

            var startIndex = index1 + begin.Length;
            var length = index2 - index1 - begin.Length;
            var result = new byte[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = content[startIndex + i];
            }

            return result;
        }
    }
}