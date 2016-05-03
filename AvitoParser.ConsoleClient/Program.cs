using System;
using System.Runtime.InteropServices;
using AvitoParser.BL;
using NLog;

// ReSharper disable InconsistentNaming

namespace AvitoParser.ConsoleClient
{
    internal class Program
    {
        private static Logger _logger = null;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("Kernel32")]
        private static extern IntPtr GetConsoleWindow();

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static void Main(string[] args)
        {
            _logger = LogManager.GetCurrentClassLogger();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            EmailHelper.SetConfig(System.Configuration.ConfigurationManager.AppSettings);

            var hwnd = GetConsoleWindow();
            ShowWindow(hwnd, SW_HIDE);

            MainParser.Run();
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var exception = unhandledExceptionEventArgs.ExceptionObject as Exception;
            _logger.Fatal(exception, "Unhandled Exception");
        }
    }
}