using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ProductManager
{
    public partial class ReportView : UserControl
    {
        private readonly string connectionString =
            "Data Source=products.db";

        public ReportView()
        {
            InitializeComponent();

            DpFrom.SelectedDate = DateTime.Today.AddDays(-7);
            DpTo.SelectedDate = DateTime.Today;

            LoadReport();
        }

        private void LoadReport_Click(object sender, RoutedEventArgs e)
        {
            LoadReport();
        }

        private void LoadReport()
        {
            if (DpFrom.SelectedDate == null || DpTo.SelectedDate == null)
                return;

            string fromDate =
                DpFrom.SelectedDate.Value.ToString("yyyy-MM-dd");

            string toDate =
                DpTo.SelectedDate.Value.ToString("yyyy-MM-dd");

            var list = new List<ReportItem>();

            double totalSales = 0;
            int totalItems = 0;
            double cashTotal = 0;
            double cardTotal = 0;

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string sql = @"
                SELECT *
                FROM Sales
                WHERE date(SaleDate)
                BETWEEN @fromDate
                AND @toDate
                AND IFNULL(IsReturn,0)=0
                ORDER BY SaleDate DESC";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@fromDate", fromDate);
            cmd.Parameters.AddWithValue("@toDate", toDate);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string payment = reader["PaymentType"].ToString();

                double total = Convert.ToDouble(reader["TotalPrice"]);
                int qty = Convert.ToInt32(reader["Quantity"]);

                totalSales += total;
                totalItems += qty;

                if (payment == "Nakit")
                    cashTotal += total;
                else
                    cardTotal += total;

                DateTime saleDate =
                    DateTime.Parse(reader["SaleDate"].ToString());

                list.Add(new ReportItem
                {
                    Id = Convert.ToInt32(reader["Id"]),

                    Date = saleDate.ToString("yyyy-MM-dd"),

                    Time = saleDate.ToString("HH:mm"),
                    ProductCode = reader["ProductCode"].ToString(),
                    ProductName = reader["ProductName"].ToString(),
                    Quantity = qty,
                    TotalPrice = total,
                    PaymentType = payment
                });
            }

            GridReport.ItemsSource = list;

            TxtTotalSales.Text = $"{totalSales:F2} ₺";
            TxtTotalItems.Text = totalItems.ToString();
            TxtCash.Text = $"{cashTotal:F2} ₺";
            TxtCard.Text = $"{cardTotal:F2} ₺";
        }

        private void ReturnSale_Click(object sender, RoutedEventArgs e)
        {
            if (GridReport.SelectedItem is not ReportItem item)
            {
                MessageBox.Show("Önce İşlem Seçiniz!");
                return;
            }

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // 🔥 SADECE TEK SATIŞI İADE ET
            string updateSale = @"
                UPDATE Sales
                SET IsReturn = 1
                WHERE Id = @id";

            using (var cmd = new SqliteCommand(updateSale, connection))
            {
                cmd.Parameters.AddWithValue("@id", item.Id);
                cmd.ExecuteNonQuery();
            }

            // 🔥 STOK GERİ EKLE
            string updateStock = @"
                UPDATE Products
                SET Stock = Stock + @qty
                WHERE Code = @code";

            using (var cmd = new SqliteCommand(updateStock, connection))
            {
                cmd.Parameters.AddWithValue("@qty", item.Quantity);
                cmd.Parameters.AddWithValue("@code", item.ProductCode);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("İşlem Başarıyla İptal Edildi");

            LoadReport();
        }
    }
}