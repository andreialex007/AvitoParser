using System;
using System.Net;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;

namespace AvitoParser.Loader
{
    public static class PageLoader
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static T LoadPage<T>(string url, Func<RemoteWebDriver, T> action) where T : class
        {
            RemoteWebDriver browser = null;

            try
            {
                var driverService = PhantomJSDriverService.CreateDefaultService(@"c:\drivers\");
                driverService.HideCommandPromptWindow = true;
                browser = new PhantomJSDriver(driverService);
                browser.Navigate().GoToUrl(url);
                browser.WaitUntil(driver => browser.Url.Equals(url));
                var result = action(browser);
                browser.Quit();
                return result;
            }
            catch (WebDriverTimeoutException e)
            {
                if (browser != null)
                    browser.Quit();
                throw;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Exception while loading");
                if (browser != null)
                    browser.Quit();
                return null;
            }
        }

        public static bool CheckRedirect(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.AllowAutoRedirect = false;
                using (var response = request.GetResponse() as HttpWebResponse)
                    return response.StatusDescription == "Moved Temporarily";
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Unable to check redirect");
                return false;
            }
        }
    }
}