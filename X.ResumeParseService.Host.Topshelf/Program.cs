using System;
using System.IO;
using X.ResumeParseService.Scanner;

namespace X.ResumeParseService.Host.Topshelf
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));

            ResumeParser parser = new ResumeParser();
            parser.Start();
            Console.ReadKey();
        }
    }
}