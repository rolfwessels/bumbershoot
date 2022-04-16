using System;
using System.IO;
using System.Text;

namespace Bumbershoot.Utilities.Helpers
{
    public static class StreamHelper
    {
        public static MemoryStream ToStream(this string stringValue)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(stringValue));
        }

        public static byte[] ToBytes(this string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }
        
        public static string AsUtf8String(this byte[] values)
        {
            return Encoding.UTF8.GetString(values);
        }

        public static byte[] Combine(this byte[] a1, byte[] a2)
        {
            //https://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp
            var combine = new byte[a1.Length + a2.Length ];
            Buffer.BlockCopy(a1, 0, combine, 0, a1.Length);
            Buffer.BlockCopy(a2, 0, combine, a1.Length, a2.Length);
            return combine;
        }

        public static (byte[], byte[]) Split(this byte[] a1, int firstSize)
        {
            var rv = new byte[firstSize];
            var rv1 = new byte[a1.Length - firstSize];
            Buffer.BlockCopy(a1, 0, rv, 0, rv.Length);
            Buffer.BlockCopy(a1, firstSize, rv1, 0, rv1.Length);
            return (rv, rv1);
        }


        public static byte[] ToBytes(this Stream input)
        {
            if (input is MemoryStream memoryStream) return memoryStream.ToArray();
            using var ms = new MemoryStream();
            input.Position = 0;
            input.CopyTo(ms);
            return ms.ToArray();
        }

        public static MemoryStream ToMemoryStream(this Stream stringValue)
        {
            if (stringValue is MemoryStream stream)
            {
                stream.Position = 0;
                return stream;
            }

            var memoryStream = new MemoryStream();
            stringValue.CopyTo(memoryStream);
            memoryStream.Flush();
            memoryStream.Position = 0;
            return memoryStream;
        }

        public static string ReadToString(this Stream stream)
        {
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        public static FileInfo SaveTo(this Stream stream, string stringValue)
        {
            using (var fileStream = File.OpenWrite(stringValue))
            {
                stream.CopyTo(fileStream);
            }

            return new FileInfo(stringValue);
        }

        public static ReadOnlyMemory<byte> AsReadOnlyMemory(this string argData)
        {
            return new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(argData));
        }
    }
}