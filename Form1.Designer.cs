namespace AutoRiaAnalyzer
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.lblTitle = new System.Windows.Forms.Label();
            this.cbBrand = new System.Windows.Forms.ComboBox();
            this.cbModel = new System.Windows.Forms.ComboBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.dataGridCars = new System.Windows.Forms.DataGridView();
            this.lblStats = new System.Windows.Forms.Label();
            this.chartCars = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.cbYearFrom = new System.Windows.Forms.ComboBox();
            this.cbFuelType = new System.Windows.Forms.ComboBox();
            this.cbYearTo = new System.Windows.Forms.ComboBox();
            this.lblYearTo = new System.Windows.Forms.Label();
            this.lblYearFrom = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCars)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartCars)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Black", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTitle.ForeColor = System.Drawing.Color.Navy;
            this.lblTitle.Location = new System.Drawing.Point(28, 21);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(297, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Анализатор цен AUTO.RIA";
            // 
            // cbBrand
            // 
            this.cbBrand.FormattingEnabled = true;
            this.cbBrand.Location = new System.Drawing.Point(35, 78);
            this.cbBrand.Name = "cbBrand";
            this.cbBrand.Size = new System.Drawing.Size(121, 21);
            this.cbBrand.TabIndex = 1;
            this.cbBrand.Text = "Выбор марки авто";
            this.cbBrand.SelectedIndexChanged += new System.EventHandler(this.cmbBrand_SelectedIndexChanged);
            // 
            // cbModel
            // 
            this.cbModel.FormattingEnabled = true;
            this.cbModel.Location = new System.Drawing.Point(200, 78);
            this.cbModel.Name = "cbModel";
            this.cbModel.Size = new System.Drawing.Size(121, 21);
            this.cbModel.TabIndex = 2;
            this.cbModel.Text = "Выбор модели";
            // 
            // btnLoad
            // 
            this.btnLoad.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoad.ForeColor = System.Drawing.Color.White;
            this.btnLoad.Location = new System.Drawing.Point(200, 130);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(121, 23);
            this.btnLoad.TabIndex = 6;
            this.btnLoad.Text = "Загрузить данные";
            this.btnLoad.UseVisualStyleBackColor = false;
            this.btnLoad.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // dataGridCars
            // 
            this.dataGridCars.AllowUserToAddRows = false;
            this.dataGridCars.AllowUserToDeleteRows = false;
            this.dataGridCars.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridCars.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            this.dataGridCars.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dataGridCars.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridCars.GridColor = System.Drawing.Color.LightGray;
            this.dataGridCars.Location = new System.Drawing.Point(40, 180);
            this.dataGridCars.Name = "dataGridCars";
            this.dataGridCars.ReadOnly = true;
            this.dataGridCars.RowHeadersVisible = false;
            this.dataGridCars.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridCars.Size = new System.Drawing.Size(240, 211);
            this.dataGridCars.TabIndex = 7;
            // 
            // lblStats
            // 
            this.lblStats.AutoSize = true;
            this.lblStats.BackColor = System.Drawing.Color.CornflowerBlue;
            this.lblStats.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStats.ForeColor = System.Drawing.Color.DarkSlateBlue;
            this.lblStats.Location = new System.Drawing.Point(391, 65);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(167, 63);
            this.lblStats.TabIndex = 8;
            this.lblStats.Text = "Средняя цена:\r\nДиапазон: min–max\r\nГрафик цен →\r\n";
            // 
            // chartCars
            // 
            this.chartCars.BackColor = System.Drawing.Color.WhiteSmoke;
            chartArea1.Name = "ChartArea1";
            this.chartCars.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chartCars.Legends.Add(legend1);
            this.chartCars.Location = new System.Drawing.Point(317, 180);
            this.chartCars.Name = "chartCars";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartCars.Series.Add(series1);
            this.chartCars.Size = new System.Drawing.Size(261, 231);
            this.chartCars.TabIndex = 9;
            this.chartCars.Text = "chart1";
            this.chartCars.Click += new System.EventHandler(this.chart1_Click);
            // 
            // cbYearFrom
            // 
            this.cbYearFrom.FormattingEnabled = true;
            this.cbYearFrom.Location = new System.Drawing.Point(78, 104);
            this.cbYearFrom.Name = "cbYearFrom";
            this.cbYearFrom.Size = new System.Drawing.Size(88, 21);
            this.cbYearFrom.TabIndex = 3;
            this.cbYearFrom.Text = "С года";
            // 
            // cbFuelType
            // 
            this.cbFuelType.FormattingEnabled = true;
            this.cbFuelType.Location = new System.Drawing.Point(35, 131);
            this.cbFuelType.Name = "cbFuelType";
            this.cbFuelType.Size = new System.Drawing.Size(121, 21);
            this.cbFuelType.TabIndex = 5;
            this.cbFuelType.Text = "Выбор топлива";
            // 
            // cbYearTo
            // 
            this.cbYearTo.FormattingEnabled = true;
            this.cbYearTo.Location = new System.Drawing.Point(246, 104);
            this.cbYearTo.Name = "cbYearTo";
            this.cbYearTo.Size = new System.Drawing.Size(89, 21);
            this.cbYearTo.TabIndex = 4;
            this.cbYearTo.Text = "По год";
            // 
            // lblYearTo
            // 
            this.lblYearTo.AutoSize = true;
            this.lblYearTo.Location = new System.Drawing.Point(197, 107);
            this.lblYearTo.Name = "lblYearTo";
            this.lblYearTo.Size = new System.Drawing.Size(43, 13);
            this.lblYearTo.TabIndex = 10;
            this.lblYearTo.Text = "Год до:";
            // 
            // lblYearFrom
            // 
            this.lblYearFrom.AutoSize = true;
            this.lblYearFrom.Location = new System.Drawing.Point(35, 107);
            this.lblYearFrom.Name = "lblYearFrom";
            this.lblYearFrom.Size = new System.Drawing.Size(37, 13);
            this.lblYearFrom.TabIndex = 9;
            this.lblYearFrom.Text = "Год с:";
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(590, 424);
            this.Controls.Add(this.lblYearTo);
            this.Controls.Add(this.lblYearFrom);
            this.Controls.Add(this.cbYearTo);
            this.Controls.Add(this.cbFuelType);
            this.Controls.Add(this.cbYearFrom);
            this.Controls.Add(this.chartCars);
            this.Controls.Add(this.lblStats);
            this.Controls.Add(this.dataGridCars);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.cbModel);
            this.Controls.Add(this.cbBrand);
            this.Controls.Add(this.lblTitle);
            this.Name = "Form1";
            this.Text = "Price";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCars)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartCars)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ComboBox cbBrand;
        private System.Windows.Forms.ComboBox cbModel;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.DataGridView dataGridCars;
        private System.Windows.Forms.Label lblStats;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartCars;
        private System.Windows.Forms.ComboBox cbYearFrom; // ИЗМЕНЕНО: ОТ
        private System.Windows.Forms.ComboBox cbFuelType;
        private System.Windows.Forms.ComboBox cbYearTo; // НОВОЕ: ДО
        private System.Windows.Forms.Label lblYearTo;
        private System.Windows.Forms.Label lblYearFrom;
    }
}