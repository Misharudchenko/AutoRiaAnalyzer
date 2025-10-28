using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoRiaAnalyzer
{
    public class ApiItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int value { set => Id = value; }
        public override string ToString() => Name;
    }

    public class PriceStatistics
    {
        public double arithmeticMean { get; set; }
        // Обратите внимание, что API возвращает 'prices' как List<double>
        public List<double> prices { get; set; }
    }

    public class CarEntry
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public string YearRange { get; set; }
        public string FuelType { get; set; }
        public int Price { get; set; }
    }
}