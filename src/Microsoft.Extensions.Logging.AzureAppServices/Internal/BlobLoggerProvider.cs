﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mobigility.Extensions.Logging.AzureBlob.Internal
{
    /// <summary>
    /// The <see cref="ILoggerProvider"/> implementation that stores messages by appending them to Azure Blob in batches.
    /// </summary>
    public abstract class BlobLoggerProvider : BatchingLoggerProvider
    {
        private readonly string _appName;
        private readonly string _fileName;
        private readonly Func<string, ICloudAppendBlob> _blobReferenceFactory;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new instance of <see cref="BlobLoggerProvider"/>
        /// </summary>
        /// <param name="options"></param>
        public BlobLoggerProvider(IOptionsMonitor<AzureBlobLoggerOptions> options)
            : this(options, null)
        {
            _blobReferenceFactory = name => new BlobAppendReferenceWrapper(
                options.CurrentValue.ContainerUrl,
                name,
                _httpClient);
        }

        /// <summary>
        /// Creates a new instance of <see cref="BlobLoggerProvider"/>
        /// </summary>
        /// <param name="blobReferenceFactory">The container to store logs to.</param>
        /// <param name="options"></param>
        public BlobLoggerProvider(
            IOptionsMonitor<AzureBlobLoggerOptions> options,
            Func<string, ICloudAppendBlob> blobReferenceFactory) :
            base(options)
        {
            var value = options.CurrentValue;
            _appName = value.ApplicationName;
            _fileName = value.BlobName;
            _blobReferenceFactory = blobReferenceFactory;
            _httpClient = new HttpClient();
        }

        protected override async Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken cancellationToken)
        {
            var eventGroups = messages.GroupBy(GetBlobKey);
            foreach (var eventGroup in eventGroups)
            {
                var key = eventGroup.Key;
                var blobName = $"{key.ApplicationName}/{key.Year}/{key.Month:00}/{key.Day:00}/{key.Hour:00}/{_fileName}";

                var blob = _blobReferenceFactory(blobName);

                var content = new byte[eventGroup.Sum(i => i.Buffer.Count)];
                var contentOffset = 0;

                foreach (var logEvent in eventGroup)
                {
                    var src = logEvent.Buffer;
                    Buffer.BlockCopy(src.Array, src.Offset, content, contentOffset, src.Count);
                    contentOffset += src.Count;
                }

                await blob.AppendAsync(new ArraySegment<byte>(content), cancellationToken);
            }
        }

        private (string ApplicationName, int Year, int Month, int Day, int Hour) GetBlobKey(LogMessage e)
        {
            return (!string.IsNullOrEmpty(e.ApplicationName) ? e.ApplicationName : _appName,
                e.Timestamp.Year,
                e.Timestamp.Month,
                e.Timestamp.Day,
                e.Timestamp.Hour);
        }
    }
}