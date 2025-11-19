using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AutoRiaAnalyzer
{
    /// <summary>
    /// Сервісний клас для роботи з API AUTO.RIA та бізнес-логікою.
    /// Відокремлює логіку даних від UI (Form1).
    /// </summary>
    public class AutoRiaService
    {
        // Константи API
        private const string API_KEY = "CWM2k01NieocZCUVYWzakTO2MGTfB6gE5JlD7t1h";
        private const int CATEGORY_ID = 1;
        private const string BASE_URL = "https://developers.ria.com/auto";

        private readonly HttpClient client = new HttpClient();

        // --- МЕТОДИ API (FetchApiData) ---

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
                Debug.WriteLine($"Помилка при завантаженні данних с API: {ex.Message}");
                return default;
            }
        }

        // --- Завантаження даних для ComboBox'ів ---

        public async Task<List<ApiItem>> LoadBrandsAsync()
        {
            string url = $"{BASE_URL}/categories/{CATEGORY_ID}/marks?api_key={API_KEY}";
            return await FetchApiData<List<ApiItem>>(url);
        }

        public async Task<List<ApiItem>> LoadModelsAsync(int markId)
        {
            string url = $"{BASE_URL}/categories/{CATEGORY_ID}/marks/{markId}/models?api_key={API_KEY}";
            return await FetchApiData<List<ApiItem>>(url);
        }

        // --- Получение статистики цін (/average_price) ---

        public async Task<PriceStatistics> GetPricesFromApi(int markId, int modelId, int yearFrom, int yearTo, int fuelId, double volFrom, double volTo)
        {
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append($"{BASE_URL}/average_price?api_key={API_KEY}");

            urlBuilder.Append($"&marka_id={markId}");
            urlBuilder.Append($"&model_id={modelId}");

            urlBuilder.Append($"&yers={yearFrom}");
            urlBuilder.Append($"&yers={yearTo}");

            urlBuilder.Append($"&fuel_id={fuelId}");

            // Використовуємо CultureInfo.InvariantCulture для API-запитів
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
                        return null; // Недостатньо даних для статистики
                    }
                }

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<PriceStatistics>(responseBody);
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка при виконанні запиту GetPricesFromApi: {ex.Message}");
                return null;
            }
        }

        // --- Получение деталей оголошення (/info) ---

        public async Task<AutoInfoData> GetAdvertDetails(int advertId)
        {
            string url = $"{BASE_URL}/info?api_key={API_KEY}&auto_id={advertId}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<AutoInfoData>(responseBody);
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Загальна помилка в GetAdvertDetails: {ex.Message}");
                return null;
            }
        }

        // --- Завантаження фото ---

        /// <summary>
        /// Завантажує фотографію за ID оголошення.
        /// </summary>
        /// <param name="advertId">ID оголошення.</param>
        /// <returns>Кортеж з об'єктом Image (або null) та повідомленням про статус (або null).</returns>
        public async Task<(Image Image, string StatusMessage)> GetAdvertPhotoAsync(int advertId)
        {
            AutoInfoData details = await GetAdvertDetails(advertId);

            string photoUrl = details?.PhotoData?.SeoLinkF ??
                              details?.PhotoData?.SeoLinkB ??
                              details?.PhotoData?.SeoLinkM ?? "";

            if (string.IsNullOrEmpty(photoUrl))
            {
                return (null, "Фото недоступне (Посилання відсутнє в API).");
            }

            if (photoUrl.StartsWith("//"))
            {
                photoUrl = "https:" + photoUrl;
            }

            try
            {
                using (var stream = await client.GetStreamAsync(photoUrl))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        ms.Position = 0;

                        Image originalImage = Image.FromStream(ms);
                        // Повертаємо Bitmap, щоб дозволити закрити MemoryStream
                        return (new Bitmap(originalImage), null);
                    }
                }
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                return (null, "Фото видалено (404 Not Found).");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка завантаження фото: {ex.ToString()}");
                return (null, "Критична помилка завантаження фото.");
            }
        }

        // --- Бізнес-логіка ---

        /// <summary>
        /// Аналізує ціну оголошення відносно середньої.
        /// </summary>
        public string AnalyzePrice(int currentPrice, double avgPrice)
        {
            if (avgPrice == 0) return "Немає статистики";

            double deviation = (double)currentPrice / avgPrice;

            if (deviation > 1.1)
            {
                return "❌ Завищена";
            }
            else if (deviation < 0.9)
            {
                return "✅ Занижена";
            }
            else
            {
                return "⚖️ Середня";
            }
        }
    }
}