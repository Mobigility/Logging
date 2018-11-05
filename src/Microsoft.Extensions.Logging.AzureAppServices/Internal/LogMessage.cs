// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Mobigility.Extensions.Logging.AzureBlob.Internal
{
    public struct LogMessage
    {
        public DateTimeOffset Timestamp { get; set; }
        public ArraySegment<byte> Buffer { get; set; }
    }
}