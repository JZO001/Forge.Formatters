﻿/* *********************************************************************
 * Date: 4 Jul 2022
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
    /// Aes encryption formatter
    /// WARNING: does not supported on platform 'browser'
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public sealed class AesFormatter<T> : IDataFormatter<T>
    {

        #region Field(s)

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete
        private IDataFormatter<T> mInternalFormatter = new BinaryFormatter<T>();
#pragma warning restore CS0618 // Type or member is obsolete

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private X509Certificate2 mCertificate = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private byte[] mIV = new byte[16];

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private byte[] mKey = new byte[32];

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Initializes a new instance of the <see cref="AesFormatter{T}"/> class.
        /// </summary>
        public AesFormatter()
        {
            Random rnd = new Random();
            rnd.NextBytes(mIV);
            rnd.NextBytes(mKey);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AesFormatter{T}"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        /// <param name="key">The key.</param>
        /// <exception cref="System.ArgumentNullException">
        /// iv
        /// or
        /// key
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// </exception>
        public AesFormatter(byte[] iv, byte[] key)
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
        /// Initializes a new instance of the <see cref="AesFormatter{T}"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        /// <param name="key">The key.</param>
        /// <param name="internalFormatter">The internal formatter.</param>
        public AesFormatter(byte[] iv, byte[] key, IDataFormatter<T> internalFormatter) : this(iv, key)
        {
            if (internalFormatter == null) ThrowHelper.ThrowArgumentNullException("internalFormatter");
            this.mInternalFormatter = internalFormatter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AesFormatter{T}" /> class.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        public AesFormatter(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                ThrowHelper.ThrowArgumentNullException("certificate");
            }

            this.mCertificate = certificate;

            Buffer.BlockCopy(certificate.PublicKey.EncodedKeyValue.RawData, 0, mIV, 0, mIV.Length);
            Buffer.BlockCopy(certificate.PublicKey.EncodedKeyValue.RawData, certificate.PublicKey.EncodedKeyValue.RawData.Length - mKey.Length, mKey, 0, mKey.Length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AesFormatter{T}" /> class.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <param name="internalFormatter">The internal formatter.</param>
        public AesFormatter(X509Certificate2 certificate, IDataFormatter<T> internalFormatter)
            : this(certificate)
        {
            if (internalFormatter == null) ThrowHelper.ThrowArgumentNullException("internalFormatter");
            this.mInternalFormatter = internalFormatter;
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
        /// Gets or sets the internal formatter.
        /// </summary>
        /// <value>
        /// The internal formatter.
        /// </value>
        public IDataFormatter<T> InternalFormatter
        {
            get { return mInternalFormatter; }
            set
            {
                if (value == null)
                {
                    ThrowHelper.ThrowArgumentNullException("value");
                }

                mInternalFormatter = value;
            }
        }

        /// <summary>
        /// Gets or sets the iv.
        /// </summary>
        /// <value>
        /// The iv.
        /// </value>
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <exception cref="System.IO.InvalidDataException"></exception>
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
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <exception cref="System.IO.InvalidDataException"></exception>
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
        /// <exception cref="System.ArgumentNullException">stream</exception>
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
                    using (Aes r = Aes.Create())
                    {
                        r.IV = mIV;
                        r.Key = mKey;
                        using (ICryptoTransform decryptor = r.CreateDecryptor())
                        {
                            CryptoStream csDecrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
                            this.mInternalFormatter.Read(csDecrypt);
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
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if this instance can write the specified item; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">item</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public bool CanWrite(T item)
        {
            if (item == null)
            {
                ThrowHelper.ThrowArgumentNullException("item");
            }

            bool result = false;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    this.mInternalFormatter.Write(ms, item);
                    ms.Position = 0;

                    using (Aes r = Aes.Create())
                    {
                        r.IV = mIV;
                        r.Key = mKey;
                        using (ICryptoTransform encryptor = r.CreateEncryptor())
                        {
                            using (MemoryStream temp = new MemoryStream())
                            {
                                using (CryptoStream csEncrypt = new CryptoStream(temp, encryptor, CryptoStreamMode.Write))
                                {
                                    //csEncrypt.Write(ms.ToArray(), 0, (int)ms.Length);
                                    ms.WriteTo(csEncrypt);
                                    csEncrypt.FlushFinalBlock();
                                    temp.SetLength(0);
                                }
                            }
                        }
                    }

                    result = true;
                }
            }
            catch (Exception)
            {
            }
            return result;
        }

        /// <summary>
        /// Reads the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">stream</exception>
        /// <exception cref="System.FormatException"></exception>
        public T Read(Stream stream)
        {
            if (stream == null)
            {
                ThrowHelper.ThrowArgumentNullException("stream");
            }

            try
            {
                using (Aes r = Aes.Create())
                {
                    r.IV = mIV;
                    r.Key = mKey;
                    using (ICryptoTransform decryptor = r.CreateDecryptor())
                    {
                        CryptoStream csDecrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
                        return (T)this.mInternalFormatter.Read(csDecrypt);
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

        /// <summary>Restore the content of the stream</summary>
        /// <param name="inputStream">Source stream</param>
        /// <param name="outputStream">Output stream</param>
        /// <exception cref="System.NotImplementedException">In all cases</exception>
        public void Read(Stream inputStream, Stream outputStream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the specified stream.
        /// </summary>
        /// <param name="outputStream">The stream.</param>
        /// <param name="data">The data.</param>
        /// <exception cref="System.ArgumentNullException">
        /// stream
        /// or
        /// data
        /// </exception>
        /// <exception cref="System.FormatException"></exception>
        public void Write(Stream outputStream, T data)
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
                using (MemoryStream ms = new MemoryStream())
                {
                    this.mInternalFormatter.Write(ms, data);
                    ms.Position = 0;

                    using (Aes r = Aes.Create())
                    {
                        r.IV = mIV;
                        r.Key = mKey;
                        using (ICryptoTransform encryptor = r.CreateEncryptor())
                        {
                            CryptoStream csEncrypt = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write);
                            ms.WriteTo(csEncrypt);
                            //csEncrypt.Write(ms.ToArray(), 0, (int)ms.Length);
                            csEncrypt.FlushFinalBlock();
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
            AesFormatter<T> cloned = new AesFormatter<T>();
            cloned.mCertificate = this.mCertificate;
            cloned.mInternalFormatter = this.mInternalFormatter;
            cloned.mIV = this.mIV;
            cloned.mKey = this.mKey;
            return cloned;
        }

        #endregion

    }

}
