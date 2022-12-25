/* *********************************************************************
 * Date: 24 Dec 2022
 * Created by: Zoltan Juhasz
 * E-Mail: forge@jzo.hu
***********************************************************************/

#if NETSTANDARD2_1 || NETCOREAPP3_1_OR_GREATER

using Forge.Shared;
using System;
using System.IO;
using System.IO.Compression;

namespace Forge.Formatters
{

    /// <summary>
    /// Brotli Formatter
    /// </summary>
    public sealed class BrotliFormatter : IGZipFormatter
    {

        #region Field(s)

        private const int BUFFER_SIZE = 8192;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Initializes a new instance of the <see cref="BrotliFormatter"/> class.
        /// </summary>
        public BrotliFormatter()
        {
        }

        #endregion

        #region Public method(s)

        /// <summary>
        /// Determines whether this instance can read the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>
        ///   <c>true</c> if this instance can read the specified stream; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">stream</exception>
        public bool CanRead(Stream stream)
        {
            if (stream == null)
            {
                ThrowHelper.ThrowArgumentNullException("stream");
            }

            bool result = false;
            try
            {
                long pos = stream.Position;
                try
                {
                    using (BrotliStream brotliStream = new BrotliStream(stream, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int numRead = 0;
                        while ((numRead = brotliStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                        }
                    }
                    result = true;
                }
                catch (Exception)
                {
                }
                finally
                {
                    stream.Position = pos;
                }
            }
            catch (Exception)
            {
            }
            return result;
        }

        /// <summary>
        /// Determines whether this instance can write the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if this instance can write the specified item; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">item</exception>
        public bool CanWrite(byte[] item)
        {
            if (item == null)
            {
                ThrowHelper.ThrowArgumentNullException("item");
            }

            return true;
        }

        /// <summary>
        /// Decompress content from the specified stream.
        /// </summary>
        /// <param name="compressedStream">The stream.</param>
        /// <returns>byte[]</returns>
        /// <exception cref="ArgumentNullException">stream</exception>
        /// <exception cref="FormatException"></exception>
        public byte[] Read(Stream compressedStream)
        {
            if (compressedStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("stream");
            }

            byte[] result = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BrotliStream brotliStream = new BrotliStream(compressedStream, CompressionMode.Decompress, true))
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int numRead = 0;
                        while ((numRead = brotliStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            ms.Write(buffer, 0, numRead);
                        }
                    }
                    result = ms.ToArray();
                }
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(ex.Message, ex);
            }
            return result;
        }

        /// <summary>Decompress content from the specified stream.</summary>
        /// <param name="compressedStream">The stream.</param>
        /// <param name="decompressedStream">The stream</param>
        /// <returns>byte[]</returns>
        /// <exception cref="ArgumentNullException">compressedStread or decompressedStream</exception>
        /// <exception cref="FormatException"></exception>
        public void Read(Stream compressedStream, Stream decompressedStream)
        {
            if (compressedStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("compressedStream");
            }
            if (decompressedStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("decompressedStream");
            }

            try
            {
                using (BrotliStream brotliStream = new BrotliStream(compressedStream, CompressionMode.Decompress, true))
                {
                    byte[] buffer = new byte[BUFFER_SIZE];
                    int numRead = 0;
                    while ((numRead = brotliStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        decompressedStream.Write(buffer, 0, numRead);
                    }
                }
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Writes compressed data into the specified stream.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="data">The compressed data.</param>
        /// <exception cref="ArgumentNullException">
        /// stream
        /// or
        /// data
        /// </exception>
        /// <exception cref="FormatException"></exception>
        public void Write(Stream outputStream, byte[] data)
        {
            if (outputStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("stream");
            }
            if (data == null)
            {
                ThrowHelper.ThrowArgumentNullException("data");
            }

            try
            {
                using (BrotliStream brotliStream = new BrotliStream(outputStream, CompressionMode.Compress, true))
                {
                    brotliStream.Write(data, 0, data.Length);
                    brotliStream.Flush();
                }
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Compress input stream data and writes into the output stream.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="inputStream">The compressed input srream.</param>
        /// <exception cref="ArgumentNullException">
        /// outputStream
        /// or
        /// inputStream
        /// </exception>
        /// <exception cref="FormatException"></exception>
        public void Write(Stream outputStream, Stream inputStream)
        {
            if (outputStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("outputStream");
            }
            if (inputStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("inputStream");
            }

            try
            {
                using (BrotliStream brotliStream = new BrotliStream(outputStream, CompressionMode.Compress, true))
                {
                    inputStream.CopyTo(brotliStream);
                    brotliStream.Flush();
                }
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new BrotliFormatter();
        }

        #endregion

    }

}

#endif
