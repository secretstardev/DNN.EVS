using System;
using System.Collections.Generic;
using System.IO;
using PackageVerification.SqlScanner.Models;

namespace PackageVerification.SqlScanner.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, this is a simple console app used to test the SqlScanner class.");

            var messageList = new List<Azure>();

            var inputFilePath = @"c:\sqltest\tests\input\";
            var outputFilePath = @"c:\sqltest\tests\output\";

            var filePaths = Directory.GetFiles(inputFilePath, "*.SqlDataProvider");

            foreach (var filePath in filePaths)
            {
                var fileName = filePath.Replace(inputFilePath, "");
                var azureScanner = new ScriptScanner();
                messageList.AddRange(azureScanner.ProcessAzure(filePath, outputFilePath + fileName));
            }

            filePaths = Directory.GetFiles(outputFilePath, "*.SqlDataProvider");

            foreach (var filePath in filePaths)
            {
                var fileName = filePath.Replace(inputFilePath, "");
                var azureScanner = new ScriptScanner();
                messageList.AddRange(azureScanner.LogAzure(filePath));
            }

            foreach (var azure in messageList)
            {
                Console.WriteLine(azure.ToString());
            }

            Console.WriteLine("Done.");
            
            Console.ReadKey();
        }
    }
}
