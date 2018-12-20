// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using Mobigility.Dumping;
using System;
using System.Text;

namespace Mobigility.Extensions.Logging.AzureBlob.Internal
{
    internal class BatchingLogger : ILogger
    {
        private readonly BatchingLoggerProvider _provider;
        private readonly string _category;

        public BatchingLogger(BatchingLoggerProvider loggerProvider, string categoryName)
        {
            _provider = loggerProvider;
            _category = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _provider.IsEnabled;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var logEntry = new LogEntry<TState>
            {
                Category = _category,
                Timestamp = (state as ISupportEventTimestamp)?.Timestamp?.ToUniversalTime() ?? DateTimeOffset.UtcNow,
                LogLevel = logLevel,
                EventId = eventId,
                State = state,
                Exception = exception,
                Formatter = formatter
            };

            _provider.AddMessage(logEntry);
        }
    }
}
