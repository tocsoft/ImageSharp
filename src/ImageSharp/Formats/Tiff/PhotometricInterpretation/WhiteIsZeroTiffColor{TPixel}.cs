// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using SixLabors.ImageSharp.Formats.Tiff.Utils;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Formats.Tiff.PhotometricInterpretation
{
    /// <summary>
    /// Implements the 'WhiteIsZero' photometric interpretation (for all bit depths).
    /// </summary>
    internal class WhiteIsZeroTiffColor<TPixel> : TiffBaseColorDecoder<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly ushort bitsPerSample0;

        private readonly float factor;

        public WhiteIsZeroTiffColor(TiffBitsPerSample bitsPerSample)
        {
            this.bitsPerSample0 = bitsPerSample.Channel0;
            this.factor = (float)Math.Pow(2, this.bitsPerSample0) - 1.0f;
        }

        /// <inheritdoc/>
        public override void Decode(ReadOnlySpan<byte> data, Buffer2D<TPixel> pixels, int left, int top, int width, int height)
        {
            var color = default(TPixel);

            var bitReader = new BitReader(data);

            for (int y = top; y < top + height; y++)
            {
                Span<TPixel> pixelRow = pixels.DangerousGetRowSpan(y).Slice(left, width);
                for (int x = 0; x < pixelRow.Length; x++)
                {
                    int value = bitReader.ReadBits(this.bitsPerSample0);
                    float intensity = 1.0f - (value / this.factor);

                    color.FromScaledVector4(new Vector4(intensity, intensity, intensity, 1.0f));
                    pixelRow[x] = color;
                }

                bitReader.NextRow();
            }
        }
    }
}
