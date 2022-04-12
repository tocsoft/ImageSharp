// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Threading;
using SixLabors.ImageSharp.IO;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace SixLabors.ImageSharp.Formats.Jpeg
{
    /// <summary>
    /// Image decoder for generating an image out of a jpg stream.
    /// </summary>
    public sealed class JpegDecoder : IImageDecoder, IJpegDecoderOptions, IImageInfoDetector, IResizableImageDecoder
    {
        /// <inheritdoc/>
        public bool IgnoreMetadata { get; set; }

        /// <inheritdoc/>
        public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Guard.NotNull(stream, nameof(stream));

            using var decoder = new JpegDecoderCore(configuration, this);
            return decoder.Decode<TPixel>(configuration, stream, cancellationToken);
        }

        /// <inheritdoc />
        public Image Decode(Configuration configuration, Stream stream, CancellationToken cancellationToken)
            => this.Decode<Rgb24>(configuration, stream, cancellationToken);

        /// <summary>
        /// Placeholder summary.
        /// </summary>
        /// <param name="configuration">Placeholder2</param>
        /// <param name="stream">Placeholder3</param>
        /// <param name="targetSize">Placeholder4</param>
        /// <param name="cancellationToken">Placeholder5</param>
        /// <returns>Placeholder6</returns>
        public Image Experimental__DecodeInto(Configuration configuration, Stream stream, ResizeProcessor targetResizeOperation, CancellationToken cancellationToken)
            => this.Experimental__DecodeInto<Rgb24>(configuration, stream, targetResizeOperation, cancellationToken);

        /// <summary>
        /// Placeholder summary.
        /// </summary>
        /// <typeparam name="TPixel">Placeholder1</typeparam>
        /// <param name="configuration">Placeholder2</param>
        /// <param name="stream">Placeholder3</param>
        /// <param name="targetSize">Placeholder4</param>
        /// <param name="cancellationToken">Placeholder5</param>
        /// <returns>Placeholder6</returns>
        public Image<TPixel> Experimental__DecodeInto<TPixel>(Configuration configuration, Stream stream, ResizeProcessor targetResizeOperation, CancellationToken cancellationToken)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Guard.NotNull(stream, nameof(stream));

            using var decoder = new JpegDecoderCore(configuration, this);

            using var bufferedReadStream = new BufferedReadStream(configuration, stream);
            try
            {
                Image<TPixel> img = decoder.Experimental__DecodeInto<TPixel>(bufferedReadStream, targetResizeOperation.DestinationSize, cancellationToken);
                if (img.Size() != targetResizeOperation.DestinationSize)
                {
                    using (IImageProcessor<TPixel> specificProcessor = ((IImageProcessor)targetResizeOperation).CreatePixelSpecificProcessor(configuration, img, img.Bounds()))
                    {
                        specificProcessor.Execute();
                    }
                }

                return img;
            }
            catch (InvalidMemoryOperationException ex)
            {
                throw new InvalidImageContentException(((IImageDecoderInternals)decoder).Dimensions, ex);
            }
        }

        /// <inheritdoc/>
        public IImageInfo Identify(Configuration configuration, Stream stream, CancellationToken cancellationToken)
        {
            Guard.NotNull(stream, nameof(stream));

            using var decoder = new JpegDecoderCore(configuration, this);
            return decoder.Identify(configuration, stream, cancellationToken);
        }
    }
}
