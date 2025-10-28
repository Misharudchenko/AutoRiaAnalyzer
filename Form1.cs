using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net; // Для HttpStatusCode

namespace AutoRiaAnalyzer
{
    // --- ОСНОВНОЙ КЛАСС ФОРМЫ ---
    public partial class Form1 : Form
    {
        // ВАШИ КОНСТАНТЫ
        private const string API_KEY = "CWM2k01NieocZCUVYWzakTO2MGTfB6gE5JlD7t1h";
        private const int USER_ID = 11469251;
        private const int CATEGORY_ID = 1;
        private const string BASE_URL = "https://developers.ria.com/auto";

        private readonly HttpClient client = new HttpClient();

        public Form1()
        {
            InitializeComponent();
            _ = LoadInitialDataAsync();
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            // Ваш существующий метод
        }

        private async Task LoadInitialDataAsync()
        {
            LoadCarYears();
            LoadFuelTypes();
            await LoadBrandsAsync();
        }

        // --- МЕТОДЫ ДЛЯ ЗАПОЛНЕНИЯ COMBOBOX'ОВ ---

        private void LoadCarYears()
        {
            cbYearFrom.Items.Clear();
            cbYearTo.Items.Clear();
            int currentYear = DateTime.Now.Year;

            for (int year = currentYear; year >= 1980; year--)
            {
                cbYearFrom.Items.Add(year);
                cbYearTo.Items.Add(year);
            }

            // Установка дефолтных значений: 
            // Год ДО: Текущий год; Год С: Текущий год - 1 (узкий диапазон)

            int defaultYearTo = currentYear;
            int defaultYearFrom = currentYear - 1;

            if (cbYearTo.Items.Contains(defaultYearTo))
                cbYearTo.SelectedItem = defaultYearTo;
            else if (cbYearTo.Items.Count > 0)
                cbYearTo.SelectedIndex = 0;

            if (cbYearFrom.Items.Contains(defaultYearFrom))
                cbYearFrom.SelectedItem = defaultYearFrom;
            else if (cbYearFrom.Items.Count > 0)
                cbYearFrom.SelectedIndex = 0;
        }

        private void LoadFuelTypes()
        {
            var fuelTypes = new Dictionary<string, int>
            {
                { "Бензин", 1 },
                { "Дизель", 2 },
                { "Газ", 3 },
                { "Газ/Бензин", 4 },
                { "Гибрид", 5 },
                { "Электро", 6 }
            };

            cbFuelType.Items.Clear();
            foreach (var kvp in fuelTypes)
            {
                cbFuelType.Items.Add(new ApiItem { Id = kvp.Value, Name = kvp.Key });
            }
            if (cbFuelType.Items.Count > 0)
                cbFuelType.SelectedIndex = 0;
        }

        // --- МЕТОДЫ ДЛЯ РАБОТЫ С API ---

        private async Task<T> FetchApiData<T>(string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(responseBody);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных с API: {ex.Message}", "Ошибка API", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return default;
            }
        }

        private async Task LoadBrandsAsync()
        {
            cbBrand.Items.Clear();
            cbModel.Items.Clear();
            cbBrand.Text = "Загрузка марок...";

            string url = $"{BASE_URL}/categories/{CATEGORY_ID}/marks?api_key={API_KEY}";
            var brands = await FetchApiData<List<ApiItem>>(url);

            if (brands != null)
            {
                cbBrand.Items.AddRange(brands.Where(b => b.Id > 0).ToArray());
                cbBrand.Text = "Выбор марки авто";
                if (brands.Count > 0)
                    cbBrand.SelectedIndex = 0;
            }
            else
            {
                cbBrand.Text = "Ошибка загрузки марок";
            }
        }

        private async Task LoadModelsAsync(int markId)
        {
            cbModel.Items.Clear();
            cbModel.Text = "Загрузка моделей...";

            // Используем стандартный эндпоинт для получения списка моделей
            string url = $"{BASE_URL}/categories/{CATEGORY_ID}/marks/{markId}/models?api_key={API_KEY}";
            var models = await FetchApiData<List<ApiItem>>(url);

            if (models != null)
            {
                // Отфильтровываем элементы с ID = 0 и с пустым именем
                cbModel.Items.AddRange(models.Where(m => m.Id > 0 && !string.IsNullOrEmpty(m.Name)).ToArray());
                cbModel.Text = "Выбор модели";
                if (cbModel.Items.Count > 0)
                    cbModel.SelectedIndex = 0;
            }
            else
            {
                cbModel.Text = "Ошибка загрузки моделей";
            }
        }

        // --- ГЛАВНЫЙ МЕТОД ПОЛУЧЕНИЯ СТАТИСТИКИ ЦЕН (ИСПОЛЬЗУЕМ GET) ---

        private async Task<PriceStatistics> GetPricesFromApi(int markId, int modelId, int yearFrom, int yearTo, int fuelId)
        {
            // Используем GET метод average_price
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append($"{BASE_URL}/average_price?api_key={API_KEY}");

            urlBuilder.Append($"&marka_id={markId}");
            urlBuilder.Append($"&model_id={modelId}");

            // Диапазон годов: yers=от&yers=до (повторение параметра)
            urlBuilder.Append($"&yers={yearFrom}");
            urlBuilder.Append($"&yers={yearTo}");

            // Тип топлива: fuel_id=ID
            urlBuilder.Append($"&fuel_id={fuelId}");

            string url = urlBuilder.ToString();

            // --- ОТЛАДОЧНЫЙ ВЫВОД ---
            MessageBox.Show($"URL (GET): {url}",
                            "Сформированный GET запрос к AUTO.RIA API (Для отладки)",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            // --- КОНЕЦ ОТЛАДОЧНОГО ВЫВОДА ---

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                // Обработка случая "Not Enough Data", который часто возвращает 400 Bad Request
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    if (errorBody.Contains("Not Enough Data"))
                    {
                        // Возвращаем null, чтобы отобразить сообщение "Данные не найдены"
                        return null;
                    }
                }

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                // Десериализация в наш класс статистики
                var result = JsonConvert.DeserializeObject<PriceStatistics>(responseBody);
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении запроса: {ex.Message}", "Критическая ошибка сети/парсинга", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }


        // --- ОБНОВЛЕНИЕ ГРАФИКА И ТАБЛИЦЫ ---
        private async void UpdateChartAndGrid()
        {
            if (!(cbBrand.SelectedItem is ApiItem selectedBrand) ||
                !(cbModel.SelectedItem is ApiItem selectedModel) ||
                !(cbFuelType.SelectedItem is ApiItem selectedFuelType) ||
                !(cbYearFrom.SelectedItem is int yearFrom) ||
                !(cbYearTo.SelectedItem is int yearTo))
            {
                lblStats.Text = "Выберите марку, модель, диапазон годов и топливо.";
                return;
            }

            int markId = selectedBrand.Id;
            int modelId = selectedModel.Id;
            int fuelId = selectedFuelType.Id;
            string brandName = selectedBrand.Name;
            string modelName = selectedModel.Name;
            string fuelName = selectedFuelType.Name;

            if (yearFrom > yearTo)
            {
                MessageBox.Show("Начальный год (Год с) не может быть больше конечного года (Год до).", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblStats.Text = "Загрузка данных с AUTO.RIA...";

            PriceStatistics stats = await GetPricesFromApi(markId, modelId, yearFrom, yearTo, fuelId);

            if (stats == null || stats.prices == null || stats.prices.Count == 0)
            {
                // Выводим ID модели, чтобы пользователю было проще отладить
                lblStats.Text = $"Средняя цена:\r\nДанные по {brandName} {modelName} ({yearFrom}-{yearTo}, {fuelName}) не найдены.\r\n(ID модели: {modelId})";
                dataGridCars.DataSource = null;
                chartCars.Series.Clear();
                return;
            }

            // --- Обновление DataGrid ---
            List<CarEntry> gridData = new List<CarEntry>();
            foreach (var price in stats.prices)
            {
                gridData.Add(new CarEntry
                {
                    Brand = brandName,
                    Model = modelName,
                    YearRange = $"{yearFrom}-{yearTo}",
                    FuelType = fuelName,
                    Price = (int)Math.Round(price)
                });
            }
            dataGridCars.DataSource = gridData;

            // --- Обновление Chart ---
            chartCars.Series.Clear();
            var series = new Series($"{brandName} {modelName} ({yearFrom}-{yearTo}, {fuelName})")
            {
                ChartType = SeriesChartType.Column,
                Color = System.Drawing.Color.SteelBlue
            };

            int j = 1;
            foreach (var price in stats.prices.OrderBy(p => p))
                series.Points.AddXY($"Объявление {j++}", price);

            chartCars.Series.Add(series);

            // --- Обновление Stats Label ---
            double avg = stats.arithmeticMean;
            double min = stats.prices.Min();
            double max = stats.prices.Max();

            lblStats.Text = $"Средняя цена: {avg:F0} $\r\nДиапазон: {min:F0} – {max:F0} $\r\nКоличество объявлений: {stats.prices.Count}\r\nГрафик цен →";
        }


        // --- ОБРАБОТЧИКИ СОБЫТИЙ ---

        private async void cmbBrand_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBrand.SelectedItem is ApiItem selectedBrand)
            {
                await LoadModelsAsync(selectedBrand.Id);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateChartAndGrid();
        }
    }
}