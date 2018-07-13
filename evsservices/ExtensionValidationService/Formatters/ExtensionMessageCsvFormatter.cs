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