using Newtonsoft.Json;
using System.IO.Compression;

namespace Karmak.Integrations.Volvo.Dcds.Api
{
    public static class HttpContentExtractor
    {
        public static async Task<T?> ExtractObject<T>(HttpResponseMessage msg) where T : new()
        {
            var str = await msg.Content.ReadAsStringAsync().ConfigureAwait(false);
            var obj = JsonConvert.DeserializeObject<T>(str);
            return obj;
        }

        public static async Task<string?> ExtractFile(HttpResponseMessage msg)
        {
            Stream stream = await msg.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return TryExtractCompressedResponse(stream, out string? content)
                ? content
                : StreamToString(stream);
        }

        private static bool TryExtractCompressedResponse(Stream stream, out string? content)
        {
            if (TryExtractGzip(stream, out content))
            {
                return true;
            }

            if (TryExtractZip(stream, out content))
            {
                return true;
            }

            content = null;
            return false;
        }

        public static bool TryExtractGzip(Stream stream, out string? contents)
        {
            try
            {
                //unpacks .zip file with gzip encoding
                GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
                contents = ReadZipContents(gZipStream);
                return true;
            }
            catch (Exception)
            {
                stream.Seek(0, SeekOrigin.Begin);
                contents = null;
                return false;
            }
        }

        public static bool TryExtractZip(Stream stream, out string? contents)
        {
            try
            {
                contents = ReadZipContents(stream);
                return true;
            }
            catch (Exception)
            {
                stream.Seek(0, SeekOrigin.Begin);
                contents = null;
                return false;
            }
        }

        private static string ReadZipContents(Stream stream)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            var entry = archive.Entries.First();
            using var connection = entry.Open();
            return StreamToString(connection);
        }

        private static string StreamToString(Stream stream)
        {
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}
