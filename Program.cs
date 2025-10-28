using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net; // Добавить using для ServicePointManager

namespace AutoRiaAnalyzer
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // ИСПРАВЛЕНИЕ ОШИБКИ:
            // Tls13 не определен в .NET Framework 4.7.2.
            // Устанавливаем TLS 1.2 (и более старые) для обеспечения соединения с современными API,
            // если HttpClient не использует настройки ОС по умолчанию.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}