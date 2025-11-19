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
using System.Diagnostics;

namespace AutoRiaAnalyzer
{
    public partial class Form1 : Form
    {
        // --- СЕРВІС ---
        // Використовуємо сервіс для всієї логіки API та обчислень
        private readonly AutoRiaService _service = new AutoRiaService();

        // --- ПЕРЕМЕННЫЕ СОСТОЯНИЯ ---
        private List<CarEntry> currentCarList = new List<CarEntry>();
        private int currentAdvertIndex = -1;

        public Form1()
        {
            InitializeComponent();
            cbEngineVolumeFrom.Text = "Об'єм від (л.)";
            cbEngineVolumeTo.Text = "Об'єм до (л.)";

            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            LoadCarYears();
            LoadFuelTypes();
            LoadEngineVolumes();
            await LoadBrandsAsync();
        }

        // --- МЕТОДЫ ДЛЯ ЗАПОЛНЕНИЯ COMBOBOX'ОВ (UI-Specific - Keep) ---

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
                { "Бензин", 1 },
                { "Дизель", 2 },
                { "Газ", 3 },
                { "Газ/Бензин", 4 },
                { "Гібрид", 5 },
                { "Єлектро", 6 }
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

        // --- МЕТОДЫ API (Используют сервис) ---

        private async Task LoadBrandsAsync()
        {
            cbBrand.Items.Clear();
            cbModel.Items.Clear();
            cbBrand.Text = "Завантаження марок...";

            try
            {
                var brands = await _service.LoadBrandsAsync();

                if (brands != null)
                {
                    cbBrand.Items.AddRange(brands.Where(b => b.Id > 0).ToArray());
                    cbBrand.Text = "Вибір марки авто";
                    if (cbBrand.Items.Count > 0)
                        cbBrand.SelectedIndex = 0;
                }
                else
                {
                    cbBrand.Text = "Помилка завантаження марок";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні марок: {ex.Message}", "Помилка API", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cbBrand.Text = "Помилка завантаження марок";
            }
        }

        private async Task LoadModelsAsync(int markId)
        {
            cbModel.Items.Clear();
            cbModel.Text = "авантаження моделей...";

            try
            {
                var models = await _service.LoadModelsAsync(markId);

                if (models != null)
                {
                    cbModel.Items.AddRange(models.Where(m => m.Id > 0 && !string.IsNullOrEmpty(m.Name)).ToArray());
                    cbModel.Text = "Выбор модели";
                    if (cbModel.Items.Count > 0)
                        cbModel.SelectedIndex = 0;
                }
                else
                {
                    cbModel.Text = "Помилка завантаження моделей";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні моделей: {ex.Message}", "Ошибка API", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cbModel.Text = "Помилка завантаження моделей";
            }
        }

        // --- МЕТОДЫ ДЛЯ ЗАГРУЗКИ ФОТО И АНАЛИЗА ЦЕНЫ (UI Logic) ---

        private void DisplayAdvert(double avgPrice)
        {
            if (currentCarList == null || currentCarList.Count == 0 || currentAdvertIndex < 0 || currentAdvertIndex >= currentCarList.Count)
            {
                pbCarPhoto.Image = null;
                lblPhotoStats.Text = "Немає данних для відображення.";
                return;
            }

            CarEntry entry = currentCarList[currentAdvertIndex];

            // Анализ цены: делегирование в сервис
            string priceStatus = _service.AnalyzePrice(entry.Price, avgPrice);

            lblPhotoStats.Text = $"ID: {entry.AdvertId}\r\nЦена: {entry.Price:N0} $\r\nСтатус: {priceStatus}\r\nОбъявление {currentAdvertIndex + 1} из {currentCarList.Count}";

            _ = LoadPhoto(entry.AdvertId, lblPhotoStats.Text);
        }

        private async Task LoadPhoto(int advertId, string currentStatusText)
        {
            pbCarPhoto.Image = null;

            // Загрузка фото: делегирование в сервис
            var (image, statusMessage) = await _service.GetAdvertPhotoAsync(advertId);

            if (image != null)
            {
                // UI: отображение фото
                pbCarPhoto.Image = image;
            }
            else if (!string.IsNullOrEmpty(statusMessage))
            {
                // UI: отображение ошибки
                lblPhotoStats.Text = currentStatusText + $"\r\n{statusMessage}";
                pbCarPhoto.Image = null;
            }
        }

        // --- МЕТОД: UpdateChartAndGrid (Main Logic - Refactor to use service) ---

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
                lblStats.Text = "Заповніть усі фільтри.";
                return;
            }

            double volFrom;
            double volTo;
            try
            {
                volFrom = double.Parse(volFromStr, CultureInfo.InvariantCulture);
                volTo = double.Parse(volToStr, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                MessageBox.Show("Невірний формат об'єма двигуна.", "Помилка вводу", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int markId = selectedBrand.Id;
            int modelId = selectedModel.Id;
            int fuelId = selectedFuelType.Id;
            string brandName = selectedBrand.Name;
            string modelName = selectedModel.Name;
            string fuelName = selectedFuelType.Name;

            if (yearFrom > yearTo || volFrom > volTo)
            {
                MessageBox.Show("Початкове значення фільтра не може бути більше кінцевого.", "Помилка вводу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblStats.Text = "Завантаження данних з AUTO.RIA...";

            // Получение статистики: делегирование в сервис
            PriceStatistics stats = await _service.GetPricesFromApi(markId, modelId, yearFrom, yearTo, fuelId, volFrom, volTo);

            if (stats == null || stats.prices == null || stats.prices.Count == 0)
            {
                // UI update logic
                lblStats.Text = $"Середня ціна:\r\nДанні по {brandName} {modelName} ({yearFrom}-{yearTo}, {fuelName}, {volFrom}-{volTo}л) не найдены.\r\n(ID модели: {modelId})";
                dataGridCars.DataSource = null;
                chartCars.Series.Clear();
                currentCarList.Clear();
                pbCarPhoto.Image = null;
                lblPhotoStats.Text = "Немає оголошень.";
                return;
            }

            // Data processing (Fill currentCarList)
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

            // --- Обновление Chart (UI Logic) ---
            chartCars.Series.Clear();

            var pricesSeries = new Series("Ціни оголошень")
            {
                ChartType = SeriesChartType.Column,
                Color = System.Drawing.Color.SteelBlue
            };
            int k = 1;
            foreach (var entry in currentCarList)
                pricesSeries.Points.AddXY($"оголошення {k++}", entry.Price);

            var avgSeries = new Series("Середня ціна")
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

            chartCars.ChartAreas[0].AxisX.Title = "Оголошення (від дешевих до дорогих)";
            chartCars.ChartAreas[0].AxisY.Title = "Цена ($)";

            // --- Сброс индикатора загрузки и установка статистики (UI Logic) ---
            double min = stats.prices.Min();
            double max = stats.prices.Max();
            lblStats.Text = $"Середня ціна: {avg:F0} $\r\nДіапазон: {min:F0} – {max:F0} $\r\nКількість оголошень: {stats.prices.Count}\r\nГрафік цін →";

            // Автоматический выбор первого элемента (UI Logic)
            if (currentCarList.Count > 0)
            {
                dataGridCars.Rows[0].Selected = true;
                dataGridCars_CellClick(dataGridCars, new DataGridViewCellEventArgs(0, 0));
            }
            else
            {
                pbCarPhoto.Image = null;
                lblPhotoStats.Text = "Немає оголошень.";
            }
        }


        // --- ОБРАБОТЧИКИ СОБЫТИЙ (UI Logic - Keep) ---

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
            // Empty handler
        }

        private void lblStats_Click(object sender, EventArgs e)
        {

        }

        private void cbFuelType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}