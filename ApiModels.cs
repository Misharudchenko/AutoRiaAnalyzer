using System.Collections.Generic;
using Newtonsoft.Json;
using System.Drawing;

namespace AutoRiaAnalyzer
{
    // --- 1. Классы для марок, моделей, топлива ---
    public class ApiItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int value { set => Id = value; }

        public override string ToString() => Name;
    }

    // --- 2. Класс для статистики цен (/average_price) ---
    public class PriceStatistics
    {
        public double total { get; set; }
        public double arithmeticMean { get; set; }
        public double interQuartileMean { get; set; }
        public Dictionary<string, double> percentiles { get; set; }
        public List<double> prices { get; set; }
        public List<int> classifieds { get; set; }
    }

    // --- 3. Классы для информации об объявлении (/info) ---

    public class AutoInfoData
    {
        [JsonProperty("autoData")]
        public AutoData AutoData { get; set; }
        [JsonProperty("photoData")]
        public PhotoData PhotoData { get; set; }
        [JsonProperty("USD")]
        public int USD { get; set; }
        // Добавьте сюда другие поля, если они понадобятся, например, "linkToView"
    }

    public class AutoData
    {
        public int year { get; set; }
    }

    // ИСПРАВЛЕНО: Добавлены SeoLinkB и SeoLinkF
    public class PhotoData
    {
        [JsonProperty("seoLinkM")]
        public string SeoLinkM { get; set; }
        [JsonProperty("seoLinkB")]
        public string SeoLinkB { get; set; }
        [JsonProperty("seoLinkF")]
        public string SeoLinkF { get; set; }
        // Добавьте сюда другие поля, если они понадобятся, например, "seoLinkSX"
    }

    // --- 4. Класс для DataGridView ---
    public class CarEntry
    {
        public int AdvertId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string YearRange { get; set; }
        public string FuelType { get; set; }
        public int Price { get; set; }
        public double AvgPrice { get; set; }
    }
}