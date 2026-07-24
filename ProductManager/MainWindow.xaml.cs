using System.Windows;

namespace ProductManager
{
    public partial class MainWindow : Window
    {
        Database db = new Database();

        public MainWindow()
        {
            InitializeComponent();

            MainContent.Content = new HomeView("home");

            BtnHome.Click += (s, e) =>
                MainContent.Content = new HomeView("home");

            BtnAdd.Click += (s, e) =>
                MainContent.Content = new AddProductView();

            BtnSearch.Click += (s, e) =>
                MainContent.Content = new SearchProductView();

            BtnSell.Click += (s, e) =>
                MainContent.Content = new SaleView();

            BtnEdit.Click += (s, e) =>
                MainContent.Content = new EditProductView();

            BtnReport.Click += (s, e) =>
                MainContent.Content = new ReportView();
        }
    }
}