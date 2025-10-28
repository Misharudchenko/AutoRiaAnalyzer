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
using System.Net;
using System.IO;
using System.Globalization;

namespace AutoRiaAnalyzer
{
    // Вспомогательные классы (AutoInfoData, PhotoData, CarEntry и т.д.)
    // ДОЛЖНЫ БЫТЬ ВЫНЕСЕНЫ В ОТДЕЛЬНЫЙ ФАЙЛ ApiModels.cs.

    public partial class Form1 : Form
    {
        // ВАШИ КОНСТАНТЫ
        private const string API_KEY = "CWM2k01NieocZCUVYWzakTO2MGTfB6gE5JlD7t1h";
        private const int USER_ID = 11469251;
        private const int CATEGORY_ID = 1;
        private const string BASE_URL = "https://developers.ria.com/auto";

        private readonly HttpClient client = new HttpClient();

        // --- ПЕРЕМЕННЫЕ СОСТОЯНИЯ ---
        private List<CarEntry> currentCarList = new List<CarEntry>();
        private int currentAdvertIndex = -1;

        public Form1()
        {
            InitializeComponent();
            cbEngineVolumeFrom.Text = "Объем от (л.)";
            cbEngineVolumeTo.Text = "Объем до (л.)";

            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            LoadCarYears();
            LoadFuelTypes();
            LoadEngineVolumes();
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
                { "Бенсин", 1 },
                { "Дизель", 2 },
                { "Газ", 3 },
                { "Газ/Бенсин", 4 },
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

        private void LoadEngineVolumes()
        {
            cbEngineVolumeFrom.Items.Clear();
            cbEngineVolumeTo.Items.Clear();

            for (double volume = 0.5; volume <= 7.0; volume += 0.1)
            {
                string volStr = volume.ToString("F1", CultureInfo.InvariantCulture);
                cbEngineVolumeFrom.Items.Add(volStr);
                cbEngineVolumeTo.Items.Add(volStr);
            }

            cbEngineVolumeFrom.SelectedItem = "1.0";
            cbEngineVolumeTo.SelectedItem = "3.0";
        }

        // --- МЕТОДЫ API (FetchApiData, LoadBrandsAsync, LoadModelsAsync) ---

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
                if (cbBrand.Items.Count > 0)
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

            string url = $"{BASE_URL}/categories/{CATEGORY_ID}/marks/{markId}/models?api_key={API_KEY}";
            var models = await FetchApiData<List<ApiItem>>(url);

            if (models != null)
            {
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

        private async Task<PriceStatistics> GetPricesFromApi(int markId, int modelId, int yearFrom, int yearTo, int fuelId, double volFrom, double volTo)
        {
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append($"{BASE_URL}/average_price?api_key={API_KEY}");

            urlBuilder.Append($"&marka_id={markId}");
            urlBuilder.Append($"&model_id={modelId}");

            urlBuilder.Append($"&yers={yearFrom}");
            urlBuilder.Append($"&yers={yearTo}");

            urlBuilder.Append($"&fuel_id={fuelId}");

            urlBuilder.Append($"&engineVolumeFrom={volFrom.ToString(CultureInfo.InvariantCulture)}");
            urlBuilder.Append($"&engineVolumeTo={volTo.ToString(CultureInfo.InvariantCulture)}");

            urlBuilder.Append($"&with_photo=1");

            string url = urlBuilder.ToString();

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    if (errorBody.Contains("Not Enough Data"))
                    {
                        return null;
                    }
                }

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<PriceStatistics>(responseBody);
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении запроса: {ex.Message}", "Критическая ошибка сети/парсинга", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // --- МЕТОДЫ ДЛЯ ЗАГРУЗКИ ФОТО И АНАЛИЗА ЦЕНЫ ---

        private async Task<AutoInfoData> GetAdvertDetails(int advertId)
        {
            string url = $"{BASE_URL}/info?api_key={API_KEY}&auto_id={advertId}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var resultList = JsonConvert.DeserializeObject<List<AutoInfoData>>(responseBody);
                return resultList?.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private void DisplayAdvert(double avgPrice)
        {
            if (currentCarList == null || currentCarList.Count == 0 || currentAdvertIndex < 0 || currentAdvertIndex >= currentCarList.Count)
            {
                pbCarPhoto.Image = null;
                lblPhotoStats.Text = "Нет данных для отображения.";
                return;
            }

            CarEntry entry = currentCarList[currentAdvertIndex];

            string priceStatus = AnalyzePrice(entry.Price, avgPrice);

            lblPhotoStats.Text = $"ID: {entry.AdvertId}\r\nЦена: {entry.Price:N0} $\r\nСтатус: {priceStatus}\r\nОбъявление {currentAdvertIndex + 1} из {currentCarList.Count}";

            _ = LoadPhoto(entry.AdvertId);
        }

        private async Task LoadPhoto(int advertId)
        {
            pbCarPhoto.Image = null;

            AutoInfoData details = await GetAdvertDetails(advertId);

            // Каскадный поиск ссылки: SeoLinkF (самая большая) -> SeoLinkB -> SeoLinkM
            string photoUrl = details?.PhotoData?.SeoLinkF ??
                              details?.PhotoData?.SeoLinkB ??
                              details?.PhotoData?.SeoLinkM ?? "";

            string currentStatusText = lblPhotoStats.Text;

            if (string.IsNullOrEmpty(photoUrl))
            {
                lblPhotoStats.Text = currentStatusText + "\r\nФото недоступно (Ссылка отсутствует в API).";
                return;
            }

            // ПРОВЕРКА: Если ссылка протоколо-относительная, делаем ее абсолютной
            if (photoUrl.StartsWith("//"))
            {
                photoUrl = "https:" + photoUrl;
            }

            // Загрузка
            try
            {
                using (var stream = await client.GetStreamAsync(photoUrl))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        ms.Position = 0; // Rewind the stream

                        Image originalImage = Image.FromStream(ms);
                        pbCarPhoto.Image = new Bitmap(originalImage);
                        originalImage.Dispose();
                    }
                }
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                lblPhotoStats.Text = currentStatusText + $"\r\nФото удалено (404 Not Found).";
                pbCarPhoto.Image = null;
            }
            catch (Exception ex) // --- ИЗМЕНЕНИЕ: ВЫВОД ПОЛНОГО ИСКЛЮЧЕНИЯ ---
            {
                lblPhotoStats.Text = currentStatusText + $"\r\nОшибка загрузки фото.";
                pbCarPhoto.Image = null;
                // Показываем детальную информацию об ошибке
                MessageBox.Show($"Ошибка при загрузке или обработке фото:\n{ex.ToString()}\n\nURL: {photoUrl}",
                                "Ошибка Загрузки Изображения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- МЕТОД: AnalyzePrice ---
        private string AnalyzePrice(int currentPrice, double avgPrice)
        {
            if (avgPrice == 0) return "Нет статистики";

            double deviation = (double)currentPrice / avgPrice;

            if (deviation > 1.1)
            {
                return "❌ Завышена";
            }
            else if (deviation < 0.9)
            {
                return "✅ Занижена";
            }
            else
            {
                return "⚖️ Средняя";
            }
        }


        // --- МЕТОД: UpdateChartAndGrid ---

        private async void UpdateChartAndGrid()
        {
            if (!(cbBrand.SelectedItem is ApiItem selectedBrand) ||
                !(cbModel.SelectedItem is ApiItem selectedModel) ||
                !(cbFuelType.SelectedItem is ApiItem selectedFuelType) ||
                !(cbYearFrom.SelectedItem is int yearFrom) ||
                !(cbYearTo.SelectedItem is int yearTo) ||
                !(cbEngineVolumeFrom.SelectedItem is string volFromStr) ||
                !(cbEngineVolumeTo.SelectedItem is string volToStr))
            {
                lblStats.Text = "Заполните все фильтры.";
                return;
            }

            double volFrom = double.Parse(volFromStr, CultureInfo.InvariantCulture);
            double volTo = double.Parse(volToStr, CultureInfo.InvariantCulture);

            int markId = selectedBrand.Id;
            int modelId = selectedModel.Id;
            int fuelId = selectedFuelType.Id;
            string brandName = selectedBrand.Name;
            string modelName = selectedModel.Name;
            string fuelName = selectedFuelType.Name;

            if (yearFrom > yearTo || volFrom > volTo)
            {
                MessageBox.Show("Начальное значение фильтра не может быть больше конечного.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblStats.Text = "Загрузка данных с AUTO.RIA...";

            PriceStatistics stats = await GetPricesFromApi(markId, modelId, yearFrom, yearTo, fuelId, volFrom, volTo);

            if (stats == null || stats.prices == null || stats.prices.Count == 0)
            {
                lblStats.Text = $"Средняя цена:\r\nДанные по {brandName} {modelName} ({yearFrom}-{yearTo}, {fuelName}, {volFrom}-{volTo}л) не найдены.\r\n(ID модели: {modelId})";
                dataGridCars.DataSource = null;
                chartCars.Series.Clear();
                currentCarList.Clear();
                return;
            }

            currentCarList.Clear();
            double avg = stats.arithmeticMean;

            for (int i = 0; i < stats.prices.Count; i++)
            {
                currentCarList.Add(new CarEntry
                {
                    AdvertId = stats.classifieds[i],
                    Brand = brandName,
                    Model = modelName,
                    YearRange = $"{yearFrom}-{yearTo}",
                    FuelType = fuelName,
                    Price = (int)Math.Round(stats.prices[i]),
                    AvgPrice = avg
                });
            }

            currentCarList = currentCarList.OrderBy(e => e.Price).ToList();
            dataGridCars.DataSource = currentCarList;

            // --- Обновление Chart ---
            chartCars.Series.Clear();

            var pricesSeries = new Series("Цены объявлений")
            {
                ChartType = SeriesChartType.Column,
                Color = System.Drawing.Color.SteelBlue
            };
            int k = 1;
            foreach (var entry in currentCarList)
                pricesSeries.Points.AddXY($"Объявление {k++}", entry.Price);

            var avgSeries = new Series("Средняя цена")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 3,
                Color = System.Drawing.Color.Red,
                MarkerStyle = MarkerStyle.None
            };
            for (int i = 0; i < pricesSeries.Points.Count; i++)
                avgSeries.Points.AddXY(pricesSeries.Points[i].AxisLabel, avg);

            chartCars.Series.Add(pricesSeries);
            chartCars.Series.Add(avgSeries);

            chartCars.ChartAreas[0].AxisX.Title = "Объявления (от дешевых к дорогим)";
            chartCars.ChartAreas[0].AxisY.Title = "Цена ($)";

            // --- Сброс индикатора загрузки и установка статистики ---
            double min = stats.prices.Min();
            double max = stats.prices.Max();
            lblStats.Text = $"Средняя цена: {avg:F0} $\r\nДиапазон: {min:F0} – {max:F0} $\r\nКоличество объявлений: {stats.prices.Count}\r\nГрафик цен →";

            // Автоматический выбор первого элемента
            if (currentCarList.Count > 0)
            {
                dataGridCars.Rows[0].Selected = true;
                dataGridCars_CellClick(dataGridCars, new DataGridViewCellEventArgs(0, 0));
            }
            else
            {
                pbCarPhoto.Image = null;
                lblPhotoStats.Text = "Нет объявлений.";
            }
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

        private void dataGridCars_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                currentAdvertIndex = e.RowIndex;
                if (currentCarList.Count > currentAdvertIndex)
                {
                    CarEntry selectedEntry = currentCarList[currentAdvertIndex];
                    DisplayAdvert(selectedEntry.AvgPrice);
                }
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (currentCarList.Count > 0 && currentAdvertIndex > 0)
            {
                currentAdvertIndex--;
                dataGridCars.Rows[currentAdvertIndex].Selected = true;
                dataGridCars.FirstDisplayedScrollingRowIndex = currentAdvertIndex;
                DisplayAdvert(currentCarList[currentAdvertIndex].AvgPrice);
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentCarList.Count > 0 && currentAdvertIndex < currentCarList.Count - 1)
            {
                currentAdvertIndex++;
                dataGridCars.Rows[currentAdvertIndex].Selected = true;
                dataGridCars.FirstDisplayedScrollingRowIndex = currentAdvertIndex;
                DisplayAdvert(currentCarList[currentAdvertIndex].AvgPrice);
            }
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}