using System;

namespace AvitoParser.BL.Models
{
    public class AdvertisementItem
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public decimal? Price { get; set; }
        public string Url { get; set; }
        public long? Number { get; set; }
        public string Phone { get; set; }
        public string Location { get; set; }
        public string Seller { get; set; }
        public DateTime? DatePublished { get; set; }
        public string Text { get; set; }
    }
}