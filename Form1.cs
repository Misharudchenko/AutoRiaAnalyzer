using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing; // Для кольорів та шрифтів
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;
using AutoRiaAnalyzer.Core;

namespace AutoRiaAnalyzer
{
    public partial class Form1 : Form
    {
        // --- СЕРВІС ---
        private readonly AutoRiaService _service = new AutoRiaService();

        // --- ЗМІННІ СТАНУ ---
        private List<CarEntry> currentCarList = new List<CarEntry>();
        private int currentAdvertIndex = -1;

        // --- КОЛЬОРОВА ПАЛІТРА (High Contrast) ---
        // Трохи темніший фон, щоб білі елементи "світилися"
        private readonly Color BackgroundColor = Color.FromArgb(225, 230, 235);

        // Основні кольори
        private readonly Color PrimaryColor = Color.FromArgb(52, 152, 219);   // Синій
        private readonly Color SecondaryColor = Color.FromArgb(44, 62, 80);  // Темно-синій
        private readonly Color AccentColor = Color.FromArgb(39, 174, 96);    // Темно-зелений (більш контрастний)
        private readonly Color TextColor = Color.FromArgb(44, 62, 80);       // Темний текст
        private readonly Color BorderColor = Color.FromArgb(189, 195, 199);  // Колір рамок

        public Form1()
        {
            InitializeComponent();

            // 1. Спочатку налаштовуємо дизайн
            ApplyContrastDesign();

            // 2. Встановлюємо тексти
            SetupTexts();

            // 3. Запускаємо завантаження
            _ = LoadInitialDataAsync();
        }

        // ==========================================
        //         НАЛАШТУВАННЯ ДИЗАЙНУ
        // ==========================================
        private void ApplyContrastDesign()
        {
            this.BackColor = BackgroundColor;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.Text = "AUTO.RIA Analyzer 2025";

            // --- Заголовок ---
            lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblTitle.ForeColor = SecondaryColor;

            // --- Кнопки ---
            StyleButton(btnLoad, AccentColor);
            StyleButton(btnPrev, PrimaryColor);
            StyleButton(btnNext, PrimaryColor);

            // --- Випадаючі списки (ComboBox) ---
            // Додаємо візуальний акцент для підписів
            foreach (Control c in this.Controls)
            {
                if (c is Label lbl && lbl != lblTitle && lbl != lblStats && lbl != lblPhotoStats)
                {
                    lbl.ForeColor = Color.FromArgb(80, 80, 80); // Темно-сірий для підписів
                    lbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
            }

            // --- Таблиця ---
            StyleDataGridView(dataGridCars);

            // --- Графік ---
            StyleChart(chartCars);

            // --- Фото ---
            pbCarPhoto.BackColor = Color.White;
            pbCarPhoto.BorderStyle = BorderStyle.FixedSingle; // Рамка обов'язкова!

            // --- Статистика (Інфо-панель) ---
            lblStats.ForeColor = TextColor;
            lblStats.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            // Робимо панель статусу фото схожою на картку
            lblPhotoStats.BackColor = Color.White;
            lblPhotoStats.BorderStyle = BorderStyle.FixedSingle;
            lblPhotoStats.ForeColor = Color.Black;
            lblPhotoStats.Font = new Font("Segoe UI", 10F);
        }

        private void StyleButton(Button btn, Color color)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;        // Додаємо тонку рамку
            btn.FlatAppearance.BorderColor = ControlPaint.Dark(color, 0.2f); // Рамка трохи темніша за кнопку
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            // Чітка рамка навколо таблиці
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.BackgroundColor = Color.White;

            // Сітка всередині таблиці (GridLines)
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dgv.GridColor = Color.FromArgb(230, 230, 230); // Світло-сіра сітка

            // Заголовок
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = SecondaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            // Рядки
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = TextColor;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 234, 248); // Ніжно-блакитний при виділенні
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10F);

            dgv.RowHeadersVisible = false;
        }

        private void StyleChart(Chart chart)
        {
            // Робимо графік "карткою" з білим фоном
            chart.BackColor = Color.White;
            chart.BorderlineColor = BorderColor;
            chart.BorderlineDashStyle = ChartDashStyle.Solid;
            chart.BorderlineWidth = 1;

            chart.ChartAreas[0].BackColor = Color.Transparent;
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;

            if (chart.Legends.Count > 0)
            {
                chart.Legends[0].BackColor = Color.Transparent;
                chart.Legends[0].Font = new Font("Segoe UI", 9F);
            }
        }

        private void SetupTexts()
        {
            lblTitle.Text = "Аналітика цін AUTO.RIA";
            btnLoad.Text = "Оновити дані";
            btnPrev.Text = "◀ Назад";
            btnNext.Text = "Вперед ▶";
            lblStats.Text = "Завантаження фільтрів...";

            // Перевіряємо, чи це не Label, щоб не перейменовувати підписи
            cbEngineVolumeFrom.Text = "Об'єм від";
            cbEngineVolumeTo.Text = "Об'єм до";
            cbBrand.Text = "Марка";
            cbModel.Text = "Модель";
            cbFuelType.Text = "Паливо";
            cbYearFrom.Text = "Рік з";
            cbYearTo.Text = "Рік по";
        }

        // ==========================================
        //         ЛОГІКА ЗАВАНТАЖЕННЯ
        // ==========================================

        private async Task LoadInitialDataAsync()
        {
            LoadCarYears();
            LoadFuelTypes();
            LoadEngineVolumes();
            await LoadBrandsAsync();
            lblStats.Text = "Оберіть параметри та натисніть 'Оновити дані'";
        }

        private void LoadCarYears()
        {
            cbYearFrom.Items.Clear();
            cbYearTo.Items.Clear();
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear; year >= 1990; year--)
            {
                cbYearFrom.Items.Add(year);
                cbYearTo.Items.Add(year);
            }
            if (cbYearTo.Items.Count > 0) cbYearTo.SelectedIndex = 0;
            if (cbYearFrom.Items.Count > 0) cbYearFrom.SelectedIndex = 0;
        }

        private void LoadFuelTypes()
        {
            var fuelTypes = new Dictionary<string, int>
            {
                { "Бензин", 1 }, { "Дизель", 2 }, { "Газ", 3 },
                { "Газ/Бензин", 4 }, { "Гібрид", 5 }, { "Електро", 6 }
            };
            cbFuelType.Items.Clear();
            foreach (var kvp in fuelTypes)
                cbFuelType.Items.Add(new ApiItem { Id = kvp.Value, Name = kvp.Key });
            if (cbFuelType.Items.Count > 0) cbFuelType.SelectedIndex = 0;
        }

        private void LoadEngineVolumes()
        {
            cbEngineVolumeFrom.Items.Clear();
            cbEngineVolumeTo.Items.Clear();
            for (double volume = 0.5; volume <= 6.0; volume += 0.1)
            {
                string volStr = volume.ToString("F1", CultureInfo.InvariantCulture);
                cbEngineVolumeFrom.Items.Add(volStr);
                cbEngineVolumeTo.Items.Add(volStr);
            }
            cbEngineVolumeFrom.SelectedItem = "1.0";
            cbEngineVolumeTo.SelectedItem = "3.0";
        }

        private async Task LoadBrandsAsync()
        {
            cbBrand.Items.Clear();
            cbBrand.Text = "Завантаження...";
            try
            {
                var brands = await _service.LoadBrandsAsync();
                if (brands != null)
                {
                    cbBrand.Items.AddRange(brands.Where(b => b.Id > 0).ToArray());
                    if (cbBrand.Items.Count > 0) cbBrand.SelectedIndex = 0;
                }
            }
            catch { cbBrand.Text = "Помилка"; }
        }

        private async Task LoadModelsAsync(int markId)
        {
            cbModel.Items.Clear();
            cbModel.Text = "Завантаження...";
            try
            {
                var models = await _service.LoadModelsAsync(markId);
                if (models != null)
                {
                    cbModel.Items.AddRange(models.Where(m => m.Id > 0 && !string.IsNullOrEmpty(m.Name)).ToArray());
                    if (cbModel.Items.Count > 0) cbModel.SelectedIndex = 0;
                }
            }
            catch { cbModel.Text = "Помилка"; }
        }

        // ==========================================
        //         ОБРОБНИКИ ПОДІЙ
        // ==========================================

        private void btnUpdate_Click(object sender, EventArgs e) => UpdateChartAndGrid();

        private async void cmbBrand_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBrand.SelectedItem is ApiItem selectedBrand)
                await LoadModelsAsync(selectedBrand.Id);
        }

        private void dataGridCars_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && currentCarList.Count > e.RowIndex)
            {
                currentAdvertIndex = e.RowIndex;
                DisplayAdvert(currentCarList[currentAdvertIndex].AvgPrice);
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (currentCarList.Count > 0 && currentAdvertIndex > 0)
            {
                currentAdvertIndex--;
                UpdateGridSelection();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentCarList.Count > 0 && currentAdvertIndex < currentCarList.Count - 1)
            {
                currentAdvertIndex++;
                UpdateGridSelection();
            }
        }

        // Заглушки для дизайнера
        private void lblStats_Click(object sender, EventArgs e) { }
        private void cbFuelType_SelectedIndexChanged(object sender, EventArgs e) { }


        // ==========================================
        //         ЛОГІКА ВІДОБРАЖЕННЯ
        // ==========================================

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
                MessageBox.Show("Заповніть всі поля!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            double volFrom = double.Parse(volFromStr, CultureInfo.InvariantCulture);
            double volTo = double.Parse(volToStr, CultureInfo.InvariantCulture);

            if (yearFrom > yearTo) { MessageBox.Show("Рік 'З' не може бути більшим за 'По'."); return; }

            lblStats.Text = "⏳ Аналіз даних... Це може зайняти кілька секунд.";
            lblStats.ForeColor = AccentColor;
            btnLoad.Enabled = false;
            this.Cursor = Cursors.WaitCursor; // Показуємо годинник

            int markId = selectedBrand.Id;
            int modelId = selectedModel.Id;
            int fuelId = selectedFuelType.Id;

            PriceStatistics stats = await _service.GetPricesFromApi(markId, modelId, yearFrom, yearTo, fuelId, volFrom, volTo);

            btnLoad.Enabled = true;
            this.Cursor = Cursors.Default;
            lblStats.ForeColor = TextColor;

            if (stats == null || stats.prices == null || stats.prices.Count == 0)
            {
                lblStats.Text = "На жаль, оголошень за такими параметрами не знайдено.";
                return;
            }

            currentCarList.Clear();
            double avg = stats.arithmeticMean;

            for (int i = 0; i < stats.prices.Count; i++)
            {
                currentCarList.Add(new CarEntry
                {
                    AdvertId = stats.classifieds[i],
                    Brand = selectedBrand.Name,
                    Model = selectedModel.Name,
                    YearRange = $"{yearFrom}-{yearTo}",
                    FuelType = selectedFuelType.Name,
                    Price = (int)Math.Round(stats.prices[i]),
                    AvgPrice = avg
                });
            }

            currentCarList = currentCarList.OrderBy(e => e.Price).ToList();
            dataGridCars.DataSource = currentCarList;

            // Оновлення графіка
            chartCars.Series.Clear();
            var pricesSeries = new Series("Ціни ($)") { ChartType = SeriesChartType.Column, Color = PrimaryColor };
            var avgSeries = new Series("Середня") { ChartType = SeriesChartType.Line, BorderWidth = 2, Color = Color.Red };

            int k = 1;
            foreach (var entry in currentCarList)
            {
                pricesSeries.Points.AddXY(k++, entry.Price);
                avgSeries.Points.AddXY(k - 1, avg);
            }
            chartCars.Series.Add(pricesSeries);
            chartCars.Series.Add(avgSeries);

            double min = stats.prices.Min();
            double max = stats.prices.Max();
            lblStats.Text = $"✅ Аналіз завершено!\nСередня ціна: {avg:N0} $\nДіапазон: {min:N0} $ — {max:N0} $\nВсього оголошень: {stats.prices.Count}";

            if (currentCarList.Count > 0)
            {
                dataGridCars.Rows[0].Selected = true;
                dataGridCars_CellClick(dataGridCars, new DataGridViewCellEventArgs(0, 0));
            }
        }

        private void UpdateGridSelection()
        {
            dataGridCars.ClearSelection();
            dataGridCars.Rows[currentAdvertIndex].Selected = true;
            dataGridCars.FirstDisplayedScrollingRowIndex = currentAdvertIndex;
            DisplayAdvert(currentCarList[currentAdvertIndex].AvgPrice);
        }

        private void DisplayAdvert(double avgPrice)
        {
            if (currentCarList == null || currentCarList.Count == 0 || currentAdvertIndex < 0) return;

            CarEntry entry = currentCarList[currentAdvertIndex];
            string priceStatus = _service.AnalyzePrice(entry.Price, avgPrice);

            // Використовуємо відступи для кращої читабельності
            lblPhotoStats.Text = $"   Авто: {entry.Brand} {entry.Model}\n" +
                                 $"   Рік: {entry.YearRange}\n" +
                                 $"   Паливо: {entry.FuelType}\n" +
                                 $"   -----------------------------\n" +
                                 $"   Ціна: {entry.Price:N0} $\n" +
                                 $"   Оцінка: {priceStatus}\n\n" +
                                 $"   [Оголошення {currentAdvertIndex + 1} з {currentCarList.Count}]";

            _ = LoadPhoto(entry.AdvertId);
        }

        private async Task LoadPhoto(int advertId)
        {
            pbCarPhoto.Image = null;
            var (image, statusMessage) = await _service.GetAdvertPhotoAsync(advertId);

            if (image != null)
            {
                pbCarPhoto.SizeMode = PictureBoxSizeMode.Zoom;
                pbCarPhoto.Image = image;
            }
            else if (!string.IsNullOrEmpty(statusMessage))
            {
                lblPhotoStats.Text += $"\n\n   (Фото: {statusMessage})";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}