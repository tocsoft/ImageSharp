// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Processing.Processors.Transforms
{
    /// <summary>
    /// Defines an image resizing operation with the given <see cref="IResampler"/> and dimensional parameters.
    /// </summary>
    public class ResizeProcessor : CloningImageProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResizeProcessor"/> class.
        /// </summary>
        /// <param name="options">The resize options.</param>
        public ResizeProcessor(ResizeOptions options)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(options.Sampler, nameof(options.Sampler));
            Guard.MustBeValueType(options.Sampler, nameof(options.Sampler));

            this.Options = options;
            this.DestinationSize = options.Size;
        }

        /// <summary>
        /// Gets the destination width.
        /// </summary>
        public int DestinationWidth => this.DestinationSize.Width;

        /// <summary>
        /// Gets the destination height.
        /// </summary>
        public int DestinationHeight => this.DestinationSize.Height;

        /// <summary>
        /// Gets the destination size.
        /// </summary>
        public Size DestinationSize { get; }

        /// <summary>
        /// Gets the resize options.
        /// </summary>
        public ResizeOptions Options { get; }

        /// <inheritdoc />
        public override ICloningImageProcessor<TPixel> CreatePixelSpecificCloningProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle)
            => new ResizeProcessor<TPixel>(configuration, this, source, sourceRectangle);
    }
}
