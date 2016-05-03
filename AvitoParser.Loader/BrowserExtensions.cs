using System;
using System.Threading;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace AvitoParser.Loader
{
    public static class BrowserExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static T WaitUntil<T>(this IWebDriver browser, Func<IWebDriver, T> condition, int timeout = 5)
        {
            var wait = new WebDriverWait(browser, new TimeSpan(0, 0, timeout));
            Thread.Sleep(1000);
            return wait.Until(condition);
        }

        public static IJavaScriptExecutor Scripts(this IWebDriver driver)
        {
            return (IJavaScriptExecutor)driver;
        }

        public static IWebElement TryFindElementByXPath(this RemoteWebDriver driver, string xPath)
        {
            try
            {
                return driver.FindElementByXPath(xPath);
            }
            catch (Exception ex)
            {
//                Logger.Debug(ex, "Unable  to find element by xpath");
                return null;
            }
        }

        public static IWebElement TryFindElementByXPaths(this RemoteWebDriver driver, params string[] xPaths)
        {
            foreach (var xpath in xPaths)
            {
                var element = driver.TryFindElementByXPath(xpath);
                if (element != null)
                    return element;
            }

            return null;
        }
    }
}
