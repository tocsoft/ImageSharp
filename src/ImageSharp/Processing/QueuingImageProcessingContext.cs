// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace SixLabors.ImageSharp.Processing
{
    internal class ResizeInterceptingImageProcessingContext : IImageProcessingContext
    {
        public ResizeInterceptingImageProcessingContext(Configuration configuration)
            => this.Configuration = configuration;

        public ResizeProcessor ResizeProcessor { get; private set; }

        public List<(IImageProcessor Processor, Rectangle? Rectangle)> Operations { get; } = new List<(IImageProcessor Processor, Rectangle? Rectangle)>();

        public Configuration Configuration { get; }

        public IDictionary<object, object> Properties { get; set; } = new Dictionary<object, object>();

        public IImageProcessingContext ApplyProcessor(IImageProcessor processor, Rectangle rectangle)
        {
            this.Operations.Add((processor, rectangle));
            return this;
        }

        public IImageProcessingContext ApplyProcessor(IImageProcessor processor)
        {
            if (this.Operations.Count == 0 && processor is ResizeProcessor rp)
            {
                // keep replaceing if multiple are applied (not quite acurate but conceptually similar)
                this.ResizeProcessor = rp;
            }
            else
            {
                this.Operations.Add((processor, null));
            }

            return this;
        }

        public Size GetCurrentSize() =>
            // if we call this we need to bomb out and try again with the standard mutate???
            throw new NotImplementedException();

        public Action<IImageProcessingContext> BuildDelegate()
            => (processor) =>
                {
                    foreach ((IImageProcessor Processor, Rectangle? Rectangle) op in this.Operations)
                    {
                        if (op.Rectangle == null)
                        {
                            processor.ApplyProcessor(op.Processor);
                        }
                        else
                        {
                            processor.ApplyProcessor(op.Processor, op.Rectangle.Value);
                        }
                    }
                };

        public static bool TryIntercept(Configuration configuration, Action<IImageProcessingContext> action, out ResizeInterceptingImageProcessingContext context)
        {
            try
            {
                context = new ResizeInterceptingImageProcessingContext(configuration);
                action(context);
                return true;
            }
            catch (NotImplementedException)
            {
                context = null;
                return false;
            }
        }
    }
}
