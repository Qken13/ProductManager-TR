using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProductManager
{
    public partial class SaleView : UserControl
    {
        private readonly string connectionString =
            "Data Source=products.db";

        private List<Product> products =
            new List<Product>();

        private List<SaleItem> cart =
            new List<SaleItem>();

        public SaleView()
        {
            InitializeComponent();

            LoadProducts();

            GridCart.ItemsSource = cart;
        }
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Click(null, null);

                if (GridProducts.Items.Count > 0)
                {
                    GridProducts.SelectedIndex = 0;
                    GridProducts.Focus();
                }

                e.Handled = true;
            }
        }

        private void LoadProducts()
        {
            products.Clear();

            using var connection =
                new SqliteConnection(connectionString);

            connection.Open();

            string sql =
                "SELECT * FROM Products";

            using var cmd =
                new SqliteCommand(sql, connection);

            using var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                products.Add(new Product
                {
                    Code = reader["Code"].ToString(),
                    Name = reader["Name"].ToString(),
                    Category = reader["Category"].ToString(),
                    BuyPrice = Convert.ToDouble(reader["BuyPrice"]),
                    SellPrice = Convert.ToDouble(reader["SellPrice"]),
                    Stock = Convert.ToInt32(reader["Stock"])
                });
            }

            GridProducts.ItemsSource = null;
            GridProducts.ItemsSource = products;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string keyword =
                TxtSearch.Text?.Trim() ?? "";

            var result = products
                .Where(x =>
                    x.Name.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    x.Code.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            GridProducts.ItemsSource = result;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (GridProducts.SelectedItem is not Product product)
            {
                MessageBox.Show("Önce Ürün Seçiniz!");
                return;
            }

            var existing =
                cart.FirstOrDefault(
                    x => x.Code == product.Code);

            if (existing != null)
            {
                if (existing.Quantity + 1 > product.Stock)
                {
                    MessageBox.Show("Adet Stokta Bulunmamaktadır!");
                    return;
                }

                existing.Quantity++;

                RefreshCart();
                return;
            }

            cart.Add(new SaleItem
            {
                Code = product.Code,
                Name = product.Name,
                Price = product.SellPrice,
                Quantity = 1
            });

            RefreshCart();
        }
        private void GridProducts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddToCart_Click(null, null);
                e.Handled = true;
            }
        }
        private void RefreshCart()
        {
            GridCart.ItemsSource = null;
            GridCart.ItemsSource = cart;

            double total = cart.Sum(x => x.Total);

            TxtTotal.Text = $"Toplam : {total:F2}";

            TxtPaidAmount.Text = total.ToString("F2");

            TxtDiscount.Text = "İndirim : 0.00";
        }
        private void TxtPaidAmount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CompleteSale_Click(null, null);
                e.Handled = true;
            }
        }

        private void CompleteSale_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Sepet Boş!");
                return;
            }
            double total = cart.Sum(x => x.Total);

            if (!double.TryParse(TxtPaidAmount.Text, out double paidAmount))
            {
                MessageBox.Show("Alınan Para Miktarını Giriniz");
                return;
            }

            if (paidAmount > total)
            {
                MessageBox.Show("Miktar Fatura değerinden daha büyük!");
                return;
            }

            double discount = total - paidAmount;

            TxtDiscount.Text = $"İndirim : {discount:F2}";

            string paymentType =
                RbCash.IsChecked == true
                ? "Nakit"
                : "Kart";

            using var connection =
                new SqliteConnection(connectionString);

            connection.Open();

            foreach (var item in cart)
            {
                string insertSale = @"
                INSERT INTO Sales
(
    ProductCode,
    ProductName,
    Quantity,
    UnitPrice,
    Discount,
    TotalPrice,
    PaymentType,
    SaleDate
)
VALUES
(
    @code,
    @name,
    @qty,
    @price,
    @discount,
    @total,
    @payment,
    @date
)";

                using var saleCmd =
                    new SqliteCommand(
                        insertSale,
                        connection);

                saleCmd.Parameters.AddWithValue(
                    "@code",
                    item.Code);

                saleCmd.Parameters.AddWithValue(
                    "@name",
                    item.Name);

                saleCmd.Parameters.AddWithValue(
                    "@qty",
                    item.Quantity);

                saleCmd.Parameters.AddWithValue(
                    "@price",
                    item.Price);

                double itemDiscount =
    discount * item.Total / total;

                double itemPaid =
                    item.Total - itemDiscount;

                saleCmd.Parameters.AddWithValue("@discount", itemDiscount);
                saleCmd.Parameters.AddWithValue("@total", itemPaid);

                saleCmd.Parameters.AddWithValue(
                    "@payment",
                    paymentType);

                saleCmd.Parameters.AddWithValue(
                    "@date",
                    DateTime.Now.ToString(
                        "yyyy-MM-dd HH:mm:ss"));

                saleCmd.ExecuteNonQuery();

                string updateStock = @"
                UPDATE Products
                SET Stock = Stock - @qty
                WHERE Code = @code";

                using var stockCmd =
                    new SqliteCommand(
                        updateStock,
                        connection);

                stockCmd.Parameters.AddWithValue(
                    "@qty",
                    item.Quantity);

                stockCmd.Parameters.AddWithValue(
                    "@code",
                    item.Code);

                stockCmd.ExecuteNonQuery();
            }

            MessageBox.Show(
    "Satış İşlemi Başarıyla Gerçekleşmiştir");

            var main =
                Application.Current.MainWindow
                as MainWindow;

            if (main != null)
            {
                main.MainContent.Content =
                    new HomeView("home");
            }

        }
    }
}