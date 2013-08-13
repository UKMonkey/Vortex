using OggVorbisDecoder;

namespace Vortex.Client.Audio.OpenAL
{
    internal static class OpenALOggLoader
    {
        public static bool CanLoad(string filename)
        {
            return filename.ToUpper().EndsWith(".OGG");
        }

        public static OpenALLoadedSample Load(string filename)
        {
            OpenALLoadedSample loadedSample;

            using (var fileStream = new OggVorbisFileStream(filename))
            {
                var buffer = new byte[fileStream.Length];

                int readIdx = 0;

                while (readIdx < fileStream.Length)
                {
                    var readAmt = fileStream.Read(buffer, readIdx, (int) fileStream.Length);
                    readIdx += readAmt;
                }

                loadedSample = new OpenALLoadedSample
                {
                    RawBuffer = buffer,
                    Rate = fileStream.Info.Rate
                };
            }

            return loadedSample;
        }
    }
}