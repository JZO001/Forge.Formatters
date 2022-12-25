/* *********************************************************************
 * Date: 18 Nov 2016
 * Created by: Zoltan Juhasz
 * E-Mail: forge@jzo.hu
***********************************************************************/

using Forge.Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Forge.Formatters
{

    /// <summary>
    /// Rijndael formatter
    /// WARNING: does not supported on platform 'browser'
    /// </summary>
#if NET40
#else
    [Obsolete]
#endif
    public sealed class RijndaelByteArrayFormatter : IDataFormatter<Stream>
    {

        #region Field(s)

        private const int BUFFER_SIZE = 8192;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private X509Certificate2 mCertificate = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private byte[] mIV = new byte[16];

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private byte[] mKey = new byte[32];

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Initializes a new instance of the <see cref="RijndaelByteArrayFormatter"/> class.
        /// </summary>
        public RijndaelByteArrayFormatter()
        {
            Random rnd = new Random();
            rnd.NextBytes(mIV);
            rnd.NextBytes(mKey);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RijndaelByteArrayFormatter"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        /// <param name="key">The key.</param>
        /// <exception cref="ArgumentNullException">
        /// iv
        /// or
        /// key
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// </exception>
        public RijndaelByteArrayFormatter(byte[] iv, byte[] key)
        {
            if (iv == null)
                throw new ArgumentNullException("iv");

            if (key == null)
                throw new ArgumentNullException("key");

            if (iv.Length != 16)
                throw new InvalidDataException();

            if (key.Length != 32)
                throw new InvalidDataException();

            mIV = iv;
            mKey = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RijndaelByteArrayFormatter" /> class.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        public RijndaelByteArrayFormatter(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                ThrowHelper.ThrowArgumentNullException("certificate");
            }

            mCertificate = certificate;

            Buffer.BlockCopy(certificate.PublicKey.EncodedKeyValue.RawData, 0, mIV, 0, mIV.Length);
            Buffer.BlockCopy(certificate.PublicKey.EncodedKeyValue.RawData, certificate.PublicKey.EncodedKeyValue.RawData.Length - mKey.Length, mKey, 0, mKey.Length);
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the certificate.
        /// </summary>
        /// <value>
        /// The certificate.
        /// </value>
        public X509Certificate2 Certificate
        {
            get { return mCertificate; }
            set
            {
                if (value == null)
                {
                    ThrowHelper.ThrowArgumentNullException("value");
                }

                mCertificate = value;

                Buffer.BlockCopy(value.PublicKey.EncodedKeyValue.RawData, 0, mIV, 0, mIV.Length);
                Buffer.BlockCopy(value.PublicKey.EncodedKeyValue.RawData, value.PublicKey.EncodedKeyValue.RawData.Length - mKey.Length, mKey, 0, mKey.Length);
            }
        }

        /// <summary>
        /// Gets or sets the iv.
        /// </summary>
        /// <value>
        /// The iv.
        /// </value>
        /// <exception cref="ArgumentNullException">value</exception>
        /// <exception cref="InvalidDataException"></exception>
        public byte[] IV
        {
            get { return mIV; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Length != 16)
                    throw new InvalidDataException();

                mIV = value;
            }
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        /// <exception cref="ArgumentNullException">value</exception>
        /// <exception cref="InvalidDataException"></exception>
        public byte[] Key
        {
            get { return mKey; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Length != 32)
                    throw new InvalidDataException();

                mKey = value;
            }
        }

        #endregion

        #region Public methods

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

            bool result = true;
            try
            {
                long pos = stream.Position;
                try
                {
                    using (RijndaelManaged r = new RijndaelManaged())
                    {
                        r.IV = mIV;
                        r.Key = mKey;
                        using (ICryptoTransform decryptor = r.CreateDecryptor())
                        {
                            CryptoStream csDecrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
                            byte[] buffer = new byte[BUFFER_SIZE];
                            int numRead = 0;
                            while ((numRead = csDecrypt.Read(buffer, 0, buffer.Length)) != 0)
                            {
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    result = false;
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
        /// <param name="sourceStream">The item.</param>
        /// <returns>
        ///   <c>true</c> if this instance can write the specified item; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">item</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public bool CanWrite(Stream sourceStream)
        {
            if (sourceStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("sourceStream");
            }

            bool result = false;
            try
            {
                using (RijndaelManaged r = new RijndaelManaged())
                {
                    r.IV = mIV;
                    r.Key = mKey;
                    using (ICryptoTransform encryptor = r.CreateEncryptor())
                    {
                        using (MemoryStream temp = new MemoryStream())
                        {
                            using (CryptoStream csEncrypt = new CryptoStream(temp, encryptor, CryptoStreamMode.Write))
                            {
                                byte[] buffer = new byte[BUFFER_SIZE];
                                int numRead = 0;
                                while ((numRead = sourceStream.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    csEncrypt.Write(buffer, 0, numRead);
                                }

                                csEncrypt.FlushFinalBlock();
                                temp.SetLength(0);
                            }
                        }
                    }
                }

                result = true;
            }
            catch (Exception)
            {
            }
            return result;
        }

        /// <summary>
        /// Reads the specified stream.
        /// </summary>
        /// <param name="inputStream">The stream.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">inputStream</exception>
        /// <exception cref="FormatException"></exception>
        public Stream Read(Stream inputStream)
        {
            if (inputStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("inputStream");
            }

            try
            {
                MemoryStream ms = new MemoryStream();
                Read(inputStream, ms);
                ms.Position = 0;
                return ms;
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

        /// <summary>Restore the content from the input stream and writes into the output stream.</summary>
        /// <param name="inputStream">Source stream</param>
        /// <param name="outputStream">Output stream</param>
        /// <exception cref="System.FormatException"></exception>
        public void Read(Stream inputStream, Stream outputStream)
        {
            if (inputStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("inputStream");
            }
            if (outputStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("outputStream");
            }

            try
            {
                using (RijndaelManaged r = new RijndaelManaged())
                {
                    r.IV = mIV;
                    r.Key = mKey;
                    using (ICryptoTransform decryptor = r.CreateDecryptor())
                    {
                        CryptoStream csDecrypt = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read);
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int numRead = 0;
                        while ((numRead = csDecrypt.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            outputStream.Write(buffer, 0, numRead);
                        }
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
        /// Writes the specified source stream content into the target stream.
        /// </summary>
        /// <param name="outputStream">The targetStream.</param>
        /// <param name="inputStream">The sourceStream.</param>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ArgumentNullException">stream
        /// or
        /// data</exception>
        public void Write(Stream outputStream, Stream inputStream)
        {
            if (outputStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("targetStream");
            }
            if (inputStream == null)
            {
                ThrowHelper.ThrowArgumentNullException("sourceStream");
            }

            try
            {
                using (RijndaelManaged r = new RijndaelManaged())
                {
                    r.IV = mIV;
                    r.Key = mKey;
                    using (ICryptoTransform encryptor = r.CreateEncryptor())
                    {
                        CryptoStream csEncrypt = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write);
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int numRead = 0;
                        while ((numRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            csEncrypt.Write(buffer, 0, numRead);
                        }
                        csEncrypt.FlushFinalBlock();
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

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            RijndaelByteArrayFormatter cloned = new RijndaelByteArrayFormatter();
            cloned.mCertificate = mCertificate;
            cloned.mIV = mIV;
            cloned.mKey = mKey;
            return cloned;
        }

        #endregion

    }

}
