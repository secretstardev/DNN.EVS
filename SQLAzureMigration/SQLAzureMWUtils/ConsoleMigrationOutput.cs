#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.IO;

namespace SQLAzureMWUtils
{
    /// <summary>
    /// This is the ConsoleMigrationOutput class.
    /// </summary>
    /// <devdoc>
    /// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    /// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    /// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
    /// PARTICULAR PURPOSE.
    /// </devdoc>
    /// <author name="Josh Maletz" />
    /// <history>
    ///     <change date="10/7/2010" user="Josh Maletz">
    ///         Added headers, etc.
    ///     </change>
    /// </history>
    public class ConsoleMigrationOutput : IMigrationOutput
    {
        public string OutputFile { get; private set; }
        public bool ShouldWriteToConsole { get; private set; }

        public ConsoleMigrationOutput(string outputFile, bool shouldWriteToConsole)
        {
            OutputFile = outputFile;
            ShouldWriteToConsole = shouldWriteToConsole;
        }

        public void StatusUpdateHandler(AsyncNotificationEventArgs args)
        {
            WriteToFile(args);
            WriteToConsole(args);
        }

        private void WriteToFile(AsyncNotificationEventArgs args)
        {
            if (!string.IsNullOrEmpty(OutputFile))
            {
                if (args.FunctionCode == NotificationEventFunctionCode.SqlOutput)
                {
                    File.AppendAllText(OutputFile, args.DisplayText);
                }
            }
        }

        private void WriteToConsole(AsyncNotificationEventArgs args)
        {
            if (ShouldWriteToConsole)
            {
                Console.Write(args.DisplayText);
            }
        }
    }
}
