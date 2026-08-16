using SitecoreErrorAgent.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SitecoreErrorAgent
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.Title =
                "Sitecore Agentic AI Error Processor";

            Console.WriteLine(
                "==============================================");

            Console.WriteLine(
                " Sitecore Agentic AI Error Processor");

            Console.WriteLine(
                "==============================================");

            try
            {
                string inputFile =
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Input",
                        "ErrorEmails.txt");

                Console.WriteLine();
                Console.WriteLine(
                    "Input file:");

                Console.WriteLine(inputFile);

                var parser =
                    new EmailParser();

                var emails =
                    parser.ParseFile(inputFile);

                Console.WriteLine();
                Console.WriteLine(
                    "Emails found: " +
                    emails.Count);

                if (emails.Count == 0)
                {
                    Console.WriteLine(
                        "No emails found.");

                    return;
                }

                var processor =
                    new ErrorProcessor();

                await processor.ProcessAsync(
                    emails);

                Console.WriteLine();
                Console.WriteLine(
                    "==============================================");

                Console.WriteLine(
                    "PROCESSING COMPLETE");

                Console.WriteLine(
                    "==============================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "APPLICATION ERROR:");

                Console.WriteLine(
                    ex.ToString());
            }

            Console.WriteLine();
            Console.WriteLine(
                "Press any key to exit...");

            Console.ReadKey();
        }
    }
}