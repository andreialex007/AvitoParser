using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using AutoParse;
using AvitoParser.BL.Models;
using AvitoParser.Data.DataModel;
using AvitoParser.Loader;
using NLog;
using NLog.Fluent;
using OpenQA.Selenium;

namespace AvitoParser.BL
{
    public class MainParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Run()
        {
            InfiniteParseList();
        }

        public static void InfiniteParseList()
        {
            while (true)
            {
                Console.WriteLine("Parsing...");
                ParseList("https://www.avito.ru/krasnodar/telefony", "Мобильные телефоны");
                CheckSoldOutLatestWeek();
                Thread.Sleep(10 * 60 * 1000);
            }
        }

        public static void ParseList(string url, string type)
        {
            try
            {
                using (var db = new AvitoParserDbEntities())
                {
                    var advertLinks =
                        ParseAvitoAdvertisementsUrls(url)
                            .Take(10)
                            .ToList();

                    var existingItems = db.Set<Advertisment>().Where(x => advertLinks.Contains(x.Url)).ToList();
                    var newItems = advertLinks.Where(x => existingItems.All(i => i.Url != x)).Take(10).ToList();

                    var parsedItems = newItems
                        .Take(10)
                        .AsParallel()
                        .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                        .WithDegreeOfParallelism(2)
                        .Select(ParseAvitoDetailsPage)
                        .Where(x => x != null)
                        .ToList();

                    var newDbItems = parsedItems
                        .Select(item => new Advertisment
                        {
                            Url = item.Url,
                            Title = item.Title,
                            DatePublished = item.DatePublished,
                            Price = item.Price,
                            Text = item.Text,
                            Location = item.Location,
                            Seller = item.Seller,
                            Id = item.Id,
                            Number = item.Number,
                            Phone = item.Phone,
                            Type = type
                        })
                        .ToList();

                    db.Set<Advertisment>().AddRange(newDbItems);
                    db.SaveChanges();
                }
            }
            catch (WebDriverException e)
            {
                
            }
        }

        public static AdvertisementItem ParseAvitoDetailsPage(string url)
        {
            return PageLoader.LoadPage(url, browser =>
            {
                var title = browser.Title.Replace("купить в Краснодарском крае на Avito — Объявления на сайте Avito", string.Empty);

                var priceText = browser.TryFindElementByXPaths(
                    "//*[@id=\"item\"]/div[4]/div[1]/div[2]/div[2]/div[3]/div[1]/div[1]/div[2]/span/span",
                    "//*[@id=\"item\"]/div[4]/div[1]/div[2]/div[2]/div[2]/div[1]/div[1]/div[2]/span/span"
                    ).Text;

                var sellerText = browser.TryFindElementByXPaths("//*[@id=\"seller\"]/strong", "//*[@id=\"seller\"]/a/strong").Text;

                var description = browser.TryFindElementByXPaths(
                    "//*[@id=\"desc_text\"]",
                    "//*[@id=\"item\"]/div[4]/div[1]/div[2]/div[2]/div[3]/div[2]/div[2]/div").Text;

                var priceReplaced = priceText.Replace(" руб.", string.Empty);
                var price = priceReplaced.TryParseNullable<decimal>();

                return new AdvertisementItem
                {
                    Title = title,
                    Text = description,
                    Url = url,
                    Seller = sellerText,
                    Price = price,
                    DatePublished = DateTime.Now
                };
            });
        }

        public static List<string> ParseAvitoAdvertisementsUrls(string url)
        {
            return PageLoader.LoadPage(url, browser =>
            {
                var pageSource = browser.PageSource;
                var links = new List<string>();
                try
                {
                    links = browser.FindElementsByCssSelector("h3.title a").Select(x => x.GetAttribute("href")).ToList();
                }
                catch (Exception ex)
                {
                    Logger.Error(pageSource);
                    throw;

                }
                return links;
            });
        }

        public static void CheckSoldOutLatestWeek()
        {
            DateTime? weekAgo = DateTime.Now.AddDays(-7);

            using (var db = new AvitoParserDbEntities())
            {
                var advertisments = db.Advertisments
                    .Where(x => x.Type == "Мобильные телефоны")
                    .Where(x => x.SoldOut == null)
                    .Where(x => DbFunctions.DiffDays(x.DatePublished, weekAgo.Value) <= 7)
                    .ToList();

                foreach (var advert in advertisments)
                {
                    if (CheckSoldOut(advert.Url))
                    {
                        advert.SoldOut = true;
                        advert.DateSold = DateTime.Now;
                    }

                    db.SaveChanges();
                }
            }
        }

        public static bool CheckSoldOut(string url)
        {
            return PageLoader.CheckRedirect(url);
        }
    }
}