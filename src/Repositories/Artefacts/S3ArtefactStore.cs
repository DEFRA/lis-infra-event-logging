// <copyright file="S3ArtefactStore.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Artefacts;

using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

public class S3ArtefactStore(
    IAmazonS3 s3Client,
    IOptions<ArtefactStorageOptions> options) : IArtefactStore
{
    public async Task PutAsync(
        string objectKey,
        Stream content,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        EnsureBucketIsConfigured();

        await s3Client.PutObjectAsync(
            new PutObjectRequest()
            {
                BucketName = options.Value.BucketName,
                Key = objectKey,
                InputStream = content,
                ContentType = mimeType,
                AutoCloseStream = false,
            },
            cancellationToken);
    }

    public async Task<StoredArtefact?> GetAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        EnsureBucketIsConfigured();

        try
        {
            var response = await s3Client.GetObjectAsync(
                new GetObjectRequest()
                {
                    BucketName = options.Value.BucketName,
                    Key = objectKey,
                },
                cancellationToken);

            return new StoredArtefact()
            {
                Content = response.ResponseStream,
                ContentLength = response.ContentLength,
            };
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        EnsureBucketIsConfigured();

        await s3Client.DeleteObjectAsync(
            options.Value.BucketName,
            objectKey,
            cancellationToken);
    }

    private void EnsureBucketIsConfigured()
    {
        if (string.IsNullOrWhiteSpace(options.Value.BucketName))
        {
            throw new InvalidOperationException("ArtefactStorage:BucketName configuration is required.");
        }
    }
}
