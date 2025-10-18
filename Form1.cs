using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization;
using System.Windows.Forms.DataVisualization.Charting;


namespace AutoRiaAnalyzer
{
    public partial class Form1 : Form
    {
        private Dictionary<string, Dictionary<string, List<int>>> carData =
            new Dictionary<string, Dictionary<string, List<int>>>
            {
                ["Toyota"] = new Dictionary<string, List<int>>
                {
                    ["Corolla"] = new List<int> { 12000, 12500, 13000, 11800, 14000 },
                    ["Camry"] = new List<int> { 18000, 19000, 20000, 17500, 21000 },
                    ["RAV4"] = new List<int> { 23000, 24000, 24500, 25000, 25500 }
                },
                ["BMW"] = new Dictionary<string, List<int>>
                {
                    ["3 Series"] = new List<int> { 22000, 23000, 21000, 23500, 24000 },
                    ["5 Series"] = new List<int> { 30000, 32000, 31000, 29500, 31500 },
                    ["X5"] = new List<int> { 40000, 42000, 39000, 41000, 43000 }
                },
                ["Audi"] = new Dictionary<string, List<int>>
                {
                    ["A3"] = new List<int> { 19000, 19500, 20000, 18500, 21000 },
                    ["A4"] = new List<int> { 25000, 26000, 25500, 24500, 27000 },
                    ["Q5"] = new List<int> { 35000, 34000, 36000, 37000, 35500 }
                },
                ["Mercedes"] = new Dictionary<string, List<int>>
                {
                    ["C-Class"] = new List<int> { 26000, 27000, 25500, 26500, 27500 },
                    ["E-Class"] = new List<int> { 32000, 33000, 34000, 31500, 33500 },
                    ["GLC"] = new List<int> { 40000, 42000, 41000, 39500, 42500 }
                },
                ["Volkswagen"] = new Dictionary<string, List<int>>
                {
                    ["Golf"] = new List<int> { 15000, 16000, 15500, 14500, 15800 },
                    ["Passat"] = new List<int> { 20000, 21000, 19500, 20500, 21500 },
                    ["Tiguan"] = new List<int> { 26000, 27000, 26500, 25500, 27500 }
                },
                ["Mazda"] = new Dictionary<string, List<int>>
                {
                    ["Mazda3"] = new List<int> { 15000, 15500, 14800, 16000, 16200 },
                    ["CX-5"] = new List<int> { 24000, 24500, 23800, 25000, 25500 },
                    ["CX-9"] = new List<int> { 32000, 33000, 32500, 33500, 34000 }
                }
            };

        public Form1()
        {
            InitializeComponent();
            LoadBrands();
        }

        private void UpdateDataGrid()
        {
            if (cbBrand.SelectedItem == null || cbModel.SelectedItem == null)
                return;

            string brand = cbBrand.SelectedItem.ToString();
            string model = cbModel.SelectedItem.ToString();

            var prices = carData[brand][model];

            // Создаем список для DataGrid
            List<CarEntry> gridData = new List<CarEntry>();
            int i = 1;
            foreach (var price in prices)
            {
                gridData.Add(new CarEntry
                {
                    Brand = brand,
                    Model = model,
                    Price = price
                });
                i++;
            }

            // Привязка к DataGridView
            dataGridCars.DataSource = gridData;
        }


        private void chart1_Click(object sender, EventArgs e)
        {
            // Можно что-то добавить сюда, например:
            // MessageBox.Show("Щёлкнули по графику!");
        }


        private void LoadBrands()
        {
            cbBrand.Items.Clear();
            cbBrand.Items.AddRange(carData.Keys.ToArray());
        }

        private void cmbBrand_SelectedIndexChanged(object sender, EventArgs e)
        {
            string brand = cbBrand.SelectedItem.ToString();
            cbModel.Items.Clear();
            cbModel.Items.AddRange(carData[brand].Keys.ToArray());
            cbModel.SelectedIndex = 0;
        }

        private void cmbModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateChart();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateChart();
            UpdateDataGrid();
        }

        private void UpdateChart()
        {
            if (cbBrand.SelectedItem == null || cbModel.SelectedItem == null)
                return;

            string brand = cbBrand.SelectedItem.ToString();
            string model = cbModel.SelectedItem.ToString();
            var prices = carData[brand][model];

            chartCars.Series.Clear();
            var series = new Series($"{brand} {model}")
            {
                ChartType = SeriesChartType.Column,
                Color = System.Drawing.Color.SteelBlue
            };

            int i = 1;
            foreach (var price in prices)
                series.Points.AddXY($"Объявление {i++}", price);

            chartCars.Series.Add(series);

            double avg = prices.Average();
            lblStats.Text = $"Средняя цена: {avg:F0} $";
        }
    }
    public class CarEntry
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Price { get; set; }
    }

}