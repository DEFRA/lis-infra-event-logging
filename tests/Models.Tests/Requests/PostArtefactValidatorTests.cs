// <copyright file="PostArtefactValidatorTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Tests.Requests;

using Defra.Lis.EventLogging.Models.Requests.Logging;
using FluentValidation.TestHelper;

public class PostArtefactValidatorTests
{
    private readonly PostArtefactValidator validator = new();

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        var result = validator.TestValidate(CreateValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Stream_Is_Not_Readable()
    {
        var request = CreateValidRequest() with { Content = new UnreadableStream() };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_Have_Error_When_Stream_Is_Null()
    {
        var request = CreateValidRequest() with { Content = null! };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_Size_Is_Not_Positive(long size)
    {
        var request = CreateValidRequest() with { Size = size };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Size);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Have_Error_When_MimeType_Is_Empty(string mimeType)
    {
        var request = CreateValidRequest() with { MimeType = mimeType };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.MimeType);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Have_Error_When_OriginalFilename_Is_Empty(string filename)
    {
        var request = CreateValidRequest() with { OriginalFilename = filename };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OriginalFilename);
    }

    internal static PostArtefact CreateValidRequest()
    {
        return new PostArtefact()
        {
            Content = new MemoryStream([1, 2, 3]),
            MimeType = "application/pdf",
            OriginalFilename = "evidence.pdf",
            Size = 3,
        };
    }

    private sealed class UnreadableStream : Stream
    {
        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => 0;

        public override long Position { get => 0; set => throw new NotSupportedException(); }

        public override void Flush() => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
