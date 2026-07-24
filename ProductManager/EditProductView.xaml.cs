using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace ProductManager
{
    public partial class EditProductView : UserControl
    {
        private readonly string connectionString =
            "Data Source=products.db";

        public EditProductView()
        {
            InitializeComponent();
            LoadAll();
        }

        private void LoadAll()
        {
            var list = new List<Product>();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string sql = "SELECT * FROM Products";

            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Product
                {
                    Code = reader["Code"].ToString(),
                    Name = reader["Name"].ToString(),
                    Category = reader["Category"].ToString(),
                    Condition = reader["Condition"]?.ToString(),
                    BuyPrice = Convert.ToDouble(reader["BuyPrice"]),
                    SellPrice = Convert.ToDouble(reader["SellPrice"]),
                    Stock = Convert.ToInt32(reader["Stock"])
                });
            }

            GridProducts.ItemsSource = list;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var keyword = TxtSearch.Text?.Trim() ?? "";

            var list = new List<Product>();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string sql = @"
                SELECT * FROM Products
                WHERE Name LIKE @kw OR Code LIKE @kw";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Product
                {
                    Code = reader["Code"].ToString(),
                    Name = reader["Name"].ToString(),
                    Category = reader["Category"].ToString(),
                    Condition = reader["Condition"]?.ToString(),
                    BuyPrice = Convert.ToDouble(reader["BuyPrice"]),
                    SellPrice = Convert.ToDouble(reader["SellPrice"]),
                    Stock = Convert.ToInt32(reader["Stock"])
                });
            }

            GridProducts.ItemsSource = list;
        }

        private void GridProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridProducts.SelectedItem is not Product p)
                return;

            TxtCode.Text = p.Code;
            TxtName.Text = p.Name;
            TxtCategory.Text = p.Category;
            TxtCondition.Text = p.Condition;
            TxtBuy.Text = p.BuyPrice.ToString();
            TxtSell.Text = p.SellPrice.ToString();
            TxtStock.Text = p.Stock.ToString();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtCode.Text))
            {
                TxtMsg.Text = " Önce Ürün Seçiniz ⚠";
                return;
            }

            if (!double.TryParse(TxtBuy.Text, out double buy) ||
                !double.TryParse(TxtSell.Text, out double sell) ||
                !int.TryParse(TxtStock.Text, out int stock))
            {
                TxtMsg.Text = " Doğru Bilgiler Giriniz ⚠";
                return;
            }

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string sql = @"
                UPDATE Products
                SET Name=@name,
                    Category=@cat,
                    Condition=@cond,
                    BuyPrice=@buy,
                    SellPrice=@sell,
                    Stock=@stock
                WHERE Code=@code";

            using var cmd = new SqliteCommand(sql, connection);

            cmd.Parameters.AddWithValue("@code", TxtCode.Text);
            cmd.Parameters.AddWithValue("@name", TxtName.Text);
            cmd.Parameters.AddWithValue("@cat", TxtCategory.Text);
            cmd.Parameters.AddWithValue("@cond", TxtCondition.Text);
            cmd.Parameters.AddWithValue("@buy", buy);
            cmd.Parameters.AddWithValue("@sell", sell);
            cmd.Parameters.AddWithValue("@stock", stock);

            cmd.ExecuteNonQuery();

            TxtMsg.Text = "Değişiklikler kaydedildi ✔";

            LoadAll();
        }
    }
}