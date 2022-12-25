/* *********************************************************************
 * Date: 11 Jun 2009
 * Created by: Zoltan Juhasz
 * E-Mail: forge@jzo.hu
***********************************************************************/

using System;
using System.IO;

namespace Forge.Formatters
{

    /// <summary>
    /// Service interface for formatters
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public interface IDataFormatter<T> : ICloneable
    {

        /// <summary>
        /// Indicate the content of the stream is deserializable
        /// </summary>
        /// <param name="stream">Content stream</param>
        /// <returns>
        /// true if the content can be restored
        /// </returns>
        bool CanRead(Stream stream);

        /// <summary>
        /// Indicate that the item is formattable with the current formatter
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if this instance can write the specified item; otherwise, <c>false</c>.
        /// </returns>
        bool CanWrite(T item);

        /// <summary>
        /// Format the content of the stream
        /// </summary>
        /// <param name="stream">Content stream</param>
        /// <returns>
        /// T object
        /// </returns>
        T Read(Stream stream);

        /// <summary>
        /// Restore the content of the stream
        /// </summary>
        /// <param name="inputStream">Source stream</param>
        /// <param name="outputStream">Output stream</param>
        void Read(Stream inputStream, Stream outputStream);

        /// <summary>
        /// Format the provided object into the supplied stream
        /// </summary>
        /// <param name="outputStream">Stream that the formatted data has been written</param>
        /// <param name="data">Object that will be formatted</param>
        void Write(Stream outputStream, T data);

    }

}
