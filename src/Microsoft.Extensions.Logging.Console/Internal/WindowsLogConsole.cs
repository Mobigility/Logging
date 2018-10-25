// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.Extensions.Logging.Console.Internal
{
    public class WindowsLogConsole : IConsole
    {
        private readonly TextWriter _textWriter;

        /// <inheritdoc />
        public WindowsLogConsole(bool stdErr = false)
        {
            _textWriter = stdErr? System.Console.Error : System.Console.Out;
        }

        private void SetColor(ConsoleColor? background, ConsoleColor? foreground)
        {
            if (background.HasValue)
            {
                System.Console.BackgroundColor = background.Value;
            }

            if (foreground.HasValue)
            {
                System.Console.ForegroundColor = foreground.Value;
            }
        }

        private void ResetColor()
        {
            System.Console.ResetColor();
        }

        public void Write(string message, ConsoleColor? background, ConsoleColor? foreground)
        {
            SetColor(background, foreground);
            _textWriter.Write(message);
            ResetColor();
        }

        public void WriteLine(string message, ConsoleColor? background, ConsoleColor? foreground)
        {
            SetColor(background, foreground);
            _textWriter.WriteLine(message);
            ResetColor();
        }

        public void Flush()
        {
            // No action required as for every write, data is sent directly to the console
            // output stream
        }
    }
}