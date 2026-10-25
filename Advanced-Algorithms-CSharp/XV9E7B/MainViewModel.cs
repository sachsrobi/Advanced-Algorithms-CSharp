using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace XV9E7B
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private int _coffeeAmount;
        private int _milkAmount;
        private int _price;
        private Brush _coffeeColor;

        public event PropertyChangedEventHandler PropertyChanged;

        // Coffee Amount Slider Value
        public int CoffeeAmount
        {
            get => _coffeeAmount;
            set
            {
                _coffeeAmount = value;
                OnPropertyChanged();
                UpdatePriceAndColor();
            }
        }

        // Milk Amount Slider Value
        public int MilkAmount
        {
            get => _milkAmount;
            set
            {
                _milkAmount = value;
                OnPropertyChanged();
                UpdatePriceAndColor();
            }
        }

        // Price of the Coffee
        public int Price
        {
            get => _price;
            private set
            {
                _price = value;
                OnPropertyChanged();
            }
        }

        // Coffee Color
        public Brush CoffeeColor
        {
            get => _coffeeColor;
            private set
            {
                _coffeeColor = value;
                OnPropertyChanged();
            }
        }

        // Orders List
        public ObservableCollection<CoffeeOrder> Orders { get; } = new();

        // Add coffee to the orders
        public void AddToOrders()
        {
            if (CoffeeAmount == 0 && MilkAmount == 0)
                return; // Skip adding invalid orders

            Orders.Add(new CoffeeOrder { Price = $"{Price} Ft", Color = CoffeeColor });
            OnPropertyChanged(nameof(SumPrice)); // Notify the UI to update the total price

            CoffeeAmount = 0;
            MilkAmount = 0;
        }

        public int SumPrice
        {
            get
            {
                return Orders.Sum(order => int.Parse(order.Price.Split(' ')[0]));
            }
        }

        public int TotalAmount => CoffeeAmount + MilkAmount;

        private void UpdatePriceAndColor()
        {
            OnPropertyChanged(nameof(TotalAmount)); 

            Price = CoffeeAmount * 200 + MilkAmount * 100;

            if (CoffeeAmount == 0 && MilkAmount > 0)
                CoffeeColor = new SolidColorBrush(Colors.White); // Milk only
            else if (MilkAmount == 0 && CoffeeAmount > 0)
                CoffeeColor = new SolidColorBrush(Colors.Black); // Coffee only
            else if (CoffeeAmount > MilkAmount)
                CoffeeColor = new SolidColorBrush(Colors.SaddleBrown); // More coffee
            else if (MilkAmount > CoffeeAmount)
                CoffeeColor = new SolidColorBrush(Colors.BurlyWood); // More milk
            else
                CoffeeColor = new SolidColorBrush(Colors.Chocolate); // Equal ratio
        }


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CoffeeOrder
    {
        public string Price { get; set; }
        public Brush Color { get; set; }
    }
}
