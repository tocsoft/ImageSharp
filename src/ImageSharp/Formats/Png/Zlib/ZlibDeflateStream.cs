// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.IO.Compression;
using SixLabors.Memory;

namespace SixLabors.ImageSharp.Formats.Png.Zlib
{
    /// <summary>
    /// Provides methods and properties for compressing streams by using the Zlib Deflate algorithm.
    /// </summary>
    internal sealed class ZlibDeflateStream : Stream
    {
        /// <summary>
        /// The raw stream containing the uncompressed image data.
        /// </summary>
        private readonly Stream rawStream;

        /// <summary>
        /// Computes the checksum for the data stream.
        /// </summary>
        private readonly Adler32 adler32 = new Adler32();

        /// <summary>
        /// A value indicating whether this instance of the given entity has been disposed.
        /// </summary>
        /// <value><see langword="true"/> if this instance has been disposed; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// If the entity is disposed, it must not be disposed a second
        /// time. The isDisposed field is set the first time the entity
        /// is disposed. If the isDisposed field is true, then the Dispose()
        /// method will not dispose again. This help not to prolong the entity's
        /// life in the Garbage Collector.
        /// </remarks>
        private bool isDisposed;

        /// <summary>
        /// The stream responsible for compressing the input stream.
        /// </summary>
        // private DeflateStream deflateStream;
        private DeflaterOutputStream deflateStream;

        private Deflater deflater;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZlibDeflateStream"/> class.
        /// </summary>
        /// <param name="memoryAllocator">The memory allocator to use for buffer allocations.</param>
        /// <param name="stream">The stream to compress.</param>
        /// <param name="compressionLevel">The compression level.</param>
        public ZlibDeflateStream(MemoryAllocator memoryAllocator, Stream stream, int compressionLevel)
        {
            this.rawStream = stream;

            // Write the zlib header : http://tools.ietf.org/html/rfc1950
            // CMF(Compression Method and flags)
            // This byte is divided into a 4 - bit compression method and a
            // 4-bit information field depending on the compression method.
            // bits 0 to 3  CM Compression method
            // bits 4 to 7  CINFO Compression info
            //
            //   0   1
            // +---+---+
            // |CMF|FLG|
            // +---+---+
            int cmf = 0x78;
            int flg = 218;

            // http://stackoverflow.com/a/2331025/277304
            if (compressionLevel >= 5 && compressionLevel <= 6)
            {
                flg = 156;
            }
            else if (compressionLevel >= 3 && compressionLevel <= 4)
            {
                flg = 94;
            }
            else if (compressionLevel <= 2)
            {
                flg = 1;
            }

            // Just in case
            flg -= ((cmf * 256) + flg) % 31;

            if (flg < 0)
            {
                flg += 31;
            }

            this.rawStream.WriteByte((byte)cmf);
            this.rawStream.WriteByte((byte)flg);

            // Initialize the deflate Stream.
            // CompressionLevel level = CompressionLevel.Optimal;
            //
            // if (compressionLevel >= 1 && compressionLevel <= 5)
            // {
            //    level = CompressionLevel.Fastest;
            // }
            // else if (compressionLevel == 0)
            // {
            //    level = CompressionLevel.NoCompression;
            // }
            this.deflater = new Deflater(memoryAllocator, compressionLevel, true);
            this.deflateStream = new DeflaterOutputStream(this.rawStream, this.deflater) { IsStreamOwner = false };

            // this.deflateStream = new DeflateStream(this.rawStream, level, true);
        }

        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            this.deflateStream?.Flush();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.deflateStream.Write(buffer, offset, count);
            this.adler32.Update(buffer.AsSpan(offset, count));
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // dispose managed resources
                if (this.deflateStream != null)
                {
                    this.deflateStream.Dispose();
                    this.deflateStream = null;

                    // TODO: Remove temporal coupling here.
                    this.deflater.Dispose();
                    this.deflater = null;
                }
                else
                {
                    // Hack: empty input?
                    this.rawStream.WriteByte(3);
                    this.rawStream.WriteByte(0);
                }

                // Add the crc
                uint crc = (uint)this.adler32.Value;
                this.rawStream.WriteByte((byte)((crc >> 24) & 0xFF));
                this.rawStream.WriteByte((byte)((crc >> 16) & 0xFF));
                this.rawStream.WriteByte((byte)((crc >> 8) & 0xFF));
                this.rawStream.WriteByte((byte)(crc & 0xFF));
            }

            base.Dispose(disposing);

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // Note disposing is done.
            this.isDisposed = true;
        }
    }
}
