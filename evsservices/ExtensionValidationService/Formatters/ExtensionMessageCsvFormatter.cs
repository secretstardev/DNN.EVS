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
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using EVSAppController.Models;

namespace ExtensionValidationService.Formatters
{
    public class ExtensionMessageCsvFormatter : BufferedMediaTypeFormatter
    {
        private string _fileName;

        public ExtensionMessageCsvFormatter()
        {
            _fileName = "results.csv";
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/csv"));
        }

        public ExtensionMessageCsvFormatter(string fileName)
        {
            _fileName = fileName.Replace(' ', '_').Replace('.', '_');
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/csv"));
        }

        public override bool CanWriteType(Type type)
        {
            if (type == typeof(ExtensionMessage))
            {
                return true;
            }

            var enumerableType = typeof(IEnumerable<ExtensionMessage>);
            return enumerableType.IsAssignableFrom(type);
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override void WriteToStream(Type type, object value, Stream stream, HttpContent content)
        {
            WriteToStream(value, stream);
        }

        public void WriteToStream(object value, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {

                var messages = value as IEnumerable<ExtensionMessage>;
                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        WriteItem(message, writer);
                    }
                }
                else
                {
                    var singleMessage = value as ExtensionMessage;
                    if (singleMessage == null)
                    {
                        throw new InvalidOperationException("Cannot serialize type");
                    }
                    WriteItem(singleMessage, writer);
                }
            }
            stream.Close();
        }

        // Helper methods for serializing Products to CSV format. 
        private void WriteItem(ExtensionMessage message, StreamWriter writer)
        {
            writer.WriteLine("{0},{1},{2},{3}", Escape(message.MessageID), Escape(message.MessageType), Escape(message.Rule), Escape(message.Message));
        }

        static readonly char[] SpecialChars = new[] { ',', '\n', '\r', '"' };

        private static string Escape(object o)
        {
            if (o == null)
            {
                return "";
            }

            var field = o.ToString();

            return field.IndexOfAny(SpecialChars) != -1 ? String.Format("\"{0}\"", field.Replace("\"", "\"\"")) : field;
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.Add("Content-Disposition", "attachment; filename=" + _fileName);
        }

    }
}