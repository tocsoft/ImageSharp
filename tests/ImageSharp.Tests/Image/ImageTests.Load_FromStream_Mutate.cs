// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Tests.TestUtilities;
using SixLabors.ImageSharp.Tests.TestUtilities.ImageComparison;
using Xunit;

namespace SixLabors.ImageSharp.Tests
{
    public partial class ImageTests
    {
        public class ImageTests_Load_FromStream_Mutate
        {
            [Theory]
            [InlineData(TestImages.Jpeg.Baseline.Floorplan)]
            [InlineData(TestImages.Jpeg.Progressive.Festzug)]
            public void OnlyResize(string path)
            {
                var file = TestFile.Create(path);
                Action<IImageProcessingContext> pipeline = (p) =>
                {
                    p.Resize(150, 150);
                };

                ImagePipeline
                    .Open(file.Bytes)
                    .WithConfig()

                    .ExecuteAsync();

                using var imgSrc = Image.Load<Rgba32>(Configuration.Default, new MemoryStream(file.Bytes), pipeline, out _);
                using var imgExpected = Image.Load<Rgba32>(Configuration.Default, new MemoryStream(file.Bytes), out _);
                imgExpected.Mutate(pipeline);

                ImageComparer.Exact.CompareImages(imgExpected, imgSrc);
            }
        }
    }
}
