using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace ProductManager
{
    public class Database
    {
        private readonly string connectionString =
            "Data Source=products.db";

        public Database()
        {
            CreateTable();
            System.Diagnostics.Debug.WriteLine("DB CREATED / CHECKED");
        }

        private void CreateTable()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // ======================
            // PRODUCTS TABLE
            // ======================
            string productsTable = @"
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT,
                Name TEXT,
                Category TEXT,
                Condition TEXT,
                BuyPrice REAL,
                SellPrice REAL,
                Stock INTEGER,
                CreatedDate TEXT
            );";

            new SqliteCommand(productsTable, connection)
                .ExecuteNonQuery();

            
            try
            {
                string alterTable =
                    "ALTER TABLE Products ADD COLUMN Condition TEXT";

                new SqliteCommand(alterTable, connection)
                    .ExecuteNonQuery();
            }
            catch
            {
                
            }

            // ======================
            // SALES TABLE
            // ======================
            string salesTable = @"
CREATE TABLE IF NOT EXISTS Sales (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductCode TEXT,
    ProductName TEXT,
    Quantity INTEGER,
    UnitPrice REAL,
    Discount REAL DEFAULT 0,
    TotalPrice REAL,
    PaymentType TEXT,
    SaleDate TEXT,
    IsReturn INTEGER DEFAULT 0
);";
            
            try
            {
                string alterSales =
                    "ALTER TABLE Sales ADD COLUMN Discount REAL DEFAULT 0";

                new SqliteCommand(alterSales, connection)
                    .ExecuteNonQuery();
            }
            catch
            {
                
            }

            new SqliteCommand(salesTable, connection)
                .ExecuteNonQuery();

            // ======================
            // INDEX
            // ======================
            string index = @"
            CREATE INDEX IF NOT EXISTS idx_sales_date
            ON Sales(SaleDate);";

            new SqliteCommand(index, connection)
                .ExecuteNonQuery();
        }

        // ======================
        // ADD PRODUCT
        // ======================
        public void AddProduct(
            string code,
            string name,
            string category,
            string condition,
            double buy,
            double sell,
            int stock)
        {
            using var connection =
                new SqliteConnection(connectionString);

            connection.Open();

            string sql = @"
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
                @category,
                @condition,
                @buy,
                @sell,
                @stock,
                @date
            )";

            var cmd =
                new SqliteCommand(sql, connection);

            cmd.Parameters.AddWithValue("@code", code);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@category", category);

            cmd.Parameters.AddWithValue(
                "@condition",
                string.IsNullOrWhiteSpace(condition)
                    ? "-"
                    : condition);

            cmd.Parameters.AddWithValue("@buy", buy);
            cmd.Parameters.AddWithValue("@sell", sell);
            cmd.Parameters.AddWithValue("@stock", stock);

            cmd.Parameters.AddWithValue(
                "@date",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            cmd.ExecuteNonQuery();
        }

        // ======================
        // GET PRODUCTS
        // ======================
        public List<string> GetProducts()
        {
            var list = new List<string>();

            using var connection =
                new SqliteConnection(connectionString);

            connection.Open();

            string sql =
                "SELECT Name FROM Products";

            var cmd =
                new SqliteCommand(sql, connection);

            var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(reader.GetString(0));
            }

            return list;
        }
    }
}
