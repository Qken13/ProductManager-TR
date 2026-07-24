using System.ComponentModel;

namespace ProductManager
{
    public class SaleItem : INotifyPropertyChanged
    {
        private int quantity = 1;

        public string Code { get; set; }
        public string Name { get; set; }

        public double Price { get; set; }

        public int Quantity
        {
            get => quantity;
            set
            {
                quantity = value;
                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(Quantity)));

                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(Total)));
            }
        }


        public double Total => Price * Quantity;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}