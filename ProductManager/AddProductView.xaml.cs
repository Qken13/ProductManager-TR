using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProductManager
{
    public partial class AddProductView : UserControl
    {
        private readonly string connectionString =
            "Data Source=products.db";

        public AddProductView()
        {
            InitializeComponent();
        }
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            var element = Keyboard.FocusedElement as UIElement;

            if (element == TxtCondition)
            {
                Save_Click(null, null);
                return;
            }

            element?.MoveFocus(
                new TraversalRequest(FocusNavigationDirection.Next));

            e.Handled = true;
        }
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string code = TxtCode.Text;
                string name = TxtName.Text;
                string category = TxtCategory.Text;

                string condition =
                    string.IsNullOrWhiteSpace(TxtCondition.Text)
                    ? "-"
                    : TxtCondition.Text.Trim();

                if (!double.TryParse(TxtBuy.Text, out double buy) ||
                    !double.TryParse(TxtSell.Text, out double sell) ||
                    !int.TryParse(TxtStock.Text, out int stock))
                {
                    TxtMsg.Text = "LÜTFEN DOĞRU BİGİLER GİRİNİZ";
                    return;
                }

                using var connection =
                    new SqliteConnection(connectionString);

                connection.Open();

                string checkSql =
                    "SELECT Stock FROM Products WHERE Code=@code AND Name=@name";

                var checkCmd =
                    new SqliteCommand(checkSql, connection);

                checkCmd.Parameters.AddWithValue("@code", code);
                checkCmd.Parameters.AddWithValue("@name", name);

                var result = checkCmd.ExecuteScalar();

                if (result != null)
                {
                    int currentStock =
                        Convert.ToInt32(result);

                    int newStock =
                        currentStock + stock;

                    string updateSql = @"
                    UPDATE Products
                    SET Stock=@stock,
                        Condition=@condition
                    WHERE Code=@code
                    AND Name=@name";

                    var updateCmd =
                        new SqliteCommand(updateSql, connection);

                    updateCmd.Parameters.AddWithValue(
                        "@stock",
                        newStock);

                    updateCmd.Parameters.AddWithValue(
                        "@condition",
                        condition);

                    updateCmd.Parameters.AddWithValue(
                        "@code",
                        code);

                    updateCmd.Parameters.AddWithValue(
                        "@name",
                        name);

                    updateCmd.ExecuteNonQuery();

                    TxtMsg.Text =
                        "ADET GÜNCELLENDİ ✔";
                }
                else
                {
                    string insertSql = @"
                    INSERT INTO Products
                    (
                        Code,
                        Name,
                        Category,
                        Condition,
                        BuyPrice,
                        SellPrice,
                        Stock,
                        CreatedDate
                    )
                    VALUES
                    (
                        @code,
                        @name,
                        @cat,
                        @condition,
                        @buy,
                        @sell,
                        @stock,
                        @date
                    )";

                    var insertCmd =
                        new SqliteCommand(insertSql, connection);

                    insertCmd.Parameters.AddWithValue(
                        "@code",
                        code);

                    insertCmd.Parameters.AddWithValue(
                        "@name",
                        name);

                    insertCmd.Parameters.AddWithValue(
                        "@cat",
                        category);

                    insertCmd.Parameters.AddWithValue(
                        "@condition",
                        condition);

                    insertCmd.Parameters.AddWithValue(
                        "@buy",
                        buy);

                    insertCmd.Parameters.AddWithValue(
                        "@sell",
                        sell);

                    insertCmd.Parameters.AddWithValue(
                        "@stock",
                        stock);

                    insertCmd.Parameters.AddWithValue(
                        "@date",
                        DateTime.Now.ToString(
                            "yyyy-MM-dd HH:mm:ss"));

                    insertCmd.ExecuteNonQuery();

                    TxtMsg.Text =
                        "Ürün Başarıyla Eklendi ✔";
                }

                await Task.Delay(800);

                var mainWindow =
                    Application.Current.MainWindow
                    as MainWindow;

                mainWindow.MainContent.Content =
                    new HomeView("home");
            }
            catch (Exception ex)
            {
                TxtMsg.Text =
                    "Hata: " + ex.Message;
            }
        }
    }
}