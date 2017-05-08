using System;
using System.IO;

namespace Dorado.Extensions
{
    public static class StreamExtensions
    {
        public static bool ToFile(this Stream srcStream, string path)
        {
            if (srcStream == null)
                return false;

            const int BuffSize = 32768;
            bool result = true;
            int len = 0;
            Stream dstStream = null;
            byte[] buffer = new Byte[BuffSize];

            try
            {
                using (dstStream = File.OpenWrite(path))
                {
                    while ((len = srcStream.Read(buffer, 0, BuffSize)) > 0)
                        dstStream.Write(buffer, 0, len);
                }
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (dstStream != null)
                {
                    dstStream.Close();
                    dstStream.Dispose();
                }
            }

            return (result && System.IO.File.Exists(path));
        }

        public static bool ContentsEqual(this Stream src, Stream other)
        {
            Guard.ArgumentNotNull(() => src);
            Guard.ArgumentNotNull(() => other);

            if (src.Length != other.Length)
                return false;

            const int bufferSize = 2048;
            byte[] buffer1 = new byte[bufferSize];
            byte[] buffer2 = new byte[bufferSize];

            while (true)
            {
                int len1 = src.Read(buffer1, 0, bufferSize);
                int len2 = other.Read(buffer2, 0, bufferSize);

                if (len1 != len2)
                    return false;

                if (len1 == 0)
                    return true;

                int iterations = (int)Math.Ceiling((double)len1 / sizeof(Int64));
                for (int i = 0; i < iterations; i++)
                {
                    if (BitConverter.ToInt64(buffer1, i * sizeof(Int64)) != BitConverter.ToInt64(buffer2, i * sizeof(Int64)))
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定流中的所有字节。
        /// </summary>
        /// <param name="stream">指定流。</param>
        /// <returns>所有字节。</returns>
        /// <exception cref="T:System.ArgumentNullException">stream为null时引发。</exception>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            byte[] array;
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                array = memoryStream.ToArray();
            }
            return array;
        }

        public static void CopyTo(this Stream stream, Stream destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (!stream.CanRead && !stream.CanWrite)
            {
                throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
            }
            if (!destination.CanRead && !destination.CanWrite)
            {
                throw new ObjectDisposedException("destination", "ObjectDisposed_StreamClosed");
            }
            if (!stream.CanRead)
            {
                throw new NotSupportedException("NotSupported_UnreadableStream");
            }
            if (!destination.CanWrite)
            {
                throw new NotSupportedException("NotSupported_UnwritableStream");
            }

            byte[] array = new byte[81920];
            int count;
            while ((count = stream.Read(array, 0, array.Length)) != 0)
            {
                destination.Write(array, 0, count);
            }
        }
    }
}