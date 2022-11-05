using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.Services.Abstractions
{
    /// <summary>
    /// An extension <see cref="class"/> which provides wrapper functions for <see cref="IFile"/>
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Reads an <see cref="IFile"/>'s into a <see cref="byte"/> buffer and returns it
        /// </summary>
        /// <param name="file">The <see cref="IFile"/> to be read</param>
        /// <returns>The <paramref name="file"/>'s contents as <see cref="byte"/>s</returns>
        public static async Task<byte[]?> ReadAllBytes(this IFile file)
        {
            Stream stream = await file.OpenStreamForReadAsync();

            if (stream == null)
            {
                return await Task.FromResult<byte[]?>(null);
            }

            (ulong Size, DateTimeOffset EditTime) fileProperties = await file.GetPropertiesAsync();
            byte[] buffer = new byte[fileProperties.Size];

            using (stream)
            {
                stream.Read(buffer, 0, buffer.Length);
            }

            return buffer;
        }
    }
}
