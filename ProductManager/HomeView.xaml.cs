using Microsoft.Data.Sqlite;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;

namespace ProductManager
{
    public partial class HomeView : UserControl
    {
        private readonly string connectionString =
            "Data Source=products.db";

        private DispatcherTimer timer;

        public HomeView(string mode = "home")
        {
            InitializeComponent();

            switch (mode)
            {
                case "add":
                    TitleText.Text = "Ürün Ekle";
                    break;

                case "search":
                    TitleText.Text = "Ürün Ara";
                    break;

                case "sell":
                    TitleText.Text = "Ürün Sat";
                    break;

                case "edit":
                    TitleText.Text = "Ürün Güncelle";
                    break;

                case "delete":
                    TitleText.Text = "Ürün Sil";
                    break;

                case "report":
                    TitleText.Text = "Rapor";
                    break;

                default:
                    TitleText.Text = "Kontrol paneli";
                    break;
            }

            _ = LoadDashboardAsync();

            timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromMinutes(1);

            timer.Tick += async (s, e) =>
            {
                await LoadDashboardAsync();
            };

            timer.Start();
        }

        private async Task LoadDashboardAsync()
        {
            using var connection =
                new SqliteConnection(connectionString);

            connection.Open();

            // Ürün sayısı
            string productSql =
                "SELECT COUNT(*) FROM Products";

            using var cmd1 =
                new SqliteCommand(productSql, connection);

            int productCount =
                Convert.ToInt32(cmd1.ExecuteScalar());

            // Bugünkü satışlar
            string salesSql = @"
                SELECT IFNULL(SUM(TotalPrice),0)
                FROM Sales
                WHERE date(SaleDate)=date('now')
                AND IFNULL(IsReturn,0)=0";

            using var cmd2 =
                new SqliteCommand(salesSql, connection);

            double todaySales =
                Convert.ToDouble(cmd2.ExecuteScalar());

            // Stok değeri (TL)
            string stockValueSql = @"
                SELECT IFNULL(SUM(BuyPrice * Stock),0)
                FROM Products";

            using var cmd3 =
                new SqliteCommand(stockValueSql, connection);

            double stockValueUsd =
                Convert.ToDouble(cmd3.ExecuteScalar());

            // Dolar kuru
            double usdTry =
                await GetDollarRateAsync();

            System.Diagnostics.Debug.WriteLine(
                $"Kur güncellendi: {usdTry} - {DateTime.Now:HH:mm:ss}");

            // İstersen test için aç
            // MessageBox.Show($"Kur çekildi: {usdTry}");

            TxtProducts.Text =
                productCount.ToString();

            TxtSales.Text =
                $"{todaySales:F2} ₺";

            if (usdTry > 0)
            {
                TxtDollarRate.Text =
                    $"1$ = {usdTry:F2} ₺";

                TxtLastUpdate.Text =
                    $"Son Güncelleme: {DateTime.Now:HH:mm:ss}";
            }
            else
            {
                TxtDollarRate.Text =
                    "İnternet yok";
            }

            TxtStockValue.Text =
                $"{stockValueUsd:F2} $";
        }

        private async Task<double> GetDollarRateAsync()
        {
            try
            {
                using var client = new HttpClient();

                client.DefaultRequestHeaders.Add(
                    "User-Agent",
                    "Mozilla/5.0");

                string json = await client.GetStringAsync(
                    "https://fxapi.app/api/USD/TRY.json");

                using JsonDocument doc = JsonDocument.Parse(json);

                return doc.RootElement
                          .GetProperty("rate")
                          .GetDouble();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return 0;
            }
        }
    }
}