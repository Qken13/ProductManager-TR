using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ProductManager
{
    public partial class SearchProductView : UserControl
    {
        private readonly string connectionString =
            "Data Source=products.db";
       
        public SearchProductView()
        {
            InitializeComponent();
            LoadAllProducts();
        }
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Click(null, null);
                e.Handled = true;
            }
        }
        private void LoadAllProducts()
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
                    Condition = reader["Condition"].ToString(),
                    BuyPrice = Convert.ToDouble(reader["BuyPrice"]),
                    SellPrice = Convert.ToDouble(reader["SellPrice"]),
                    Stock = Convert.ToInt32(reader["Stock"]),
                    IsSelected = false
                });
            }

            GridProducts.ItemsSource = list;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string keyword = TxtSearch.Text?.Trim() ?? "";

            var list = new List<Product>();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string sql =
            @"SELECT *
              FROM Products
              WHERE Name LIKE @kw
                 OR Code LIKE @kw";

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
                    BuyPrice = Convert.ToDouble(reader["BuyPrice"]),
                    SellPrice = Convert.ToDouble(reader["SellPrice"]),
                    Condition = reader["Condition"].ToString(),
                    Stock = Convert.ToInt32(reader["Stock"]),
                    IsSelected = false
                });
            }

            GridProducts.ItemsSource = list;
        }

        private void LoadAll_Click(object sender, RoutedEventArgs e)
        {
            LoadAllProducts();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var list = GridProducts.ItemsSource as List<Product>;

            if (list == null)
            {
                MessageBox.Show("Ürünle Görüntülenmedi!");
                return;
            }

            var selected = list.Where(x => x.IsSelected).ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show("Önce Ürün Seçiniz!");
                return;
            }

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            foreach (var item in selected)
            {
                string sql = "DELETE FROM Products WHERE Code=@code";

                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@code", item.Code);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Ürün Başarıyla Silinmiştir ✔");

            LoadAllProducts();
        }
    }

    public class Product : INotifyPropertyChanged
    {
        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public double BuyPrice { get; set; }
        public double SellPrice { get; set; }
        public int Stock { get; set; }
        public string Condition { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}