using Fahrkartenautomat.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Fahrkartenautomat.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Ticket> PossibleTickets { get; }
        public ObservableCollection<Ticket> ShoppingCart { get; }
        public List<decimal> PossibleMoney { get; }
        private List<decimal> EnteredMoneyBits { get; }
        public decimal EnteredMoney 
        { 
            get { return EnteredMoneyBits.Sum(); } 
        }

        public decimal TotalPrice 
        { 
            get { return ShoppingCart.Select(ticket => ticket.Price*ticket.Amount).Sum(); } 
        }

        private ICommand _addToCartCommand;
        public ICommand AddToCartCommand
        {
            get
            {
                if (_addToCartCommand == null)
                {
                    _addToCartCommand = new RelayCommand(
                        p => this.ItemIsSelected(p),
                        p => this.AddToCart(p));
                }
                return _addToCartCommand;
            }
        }
        
        private ICommand _removeFromCartCommand;
        public ICommand RemoveFromCartCommand
        {
            get
            {
                if (_removeFromCartCommand == null)
                {
                    _removeFromCartCommand = new RelayCommand(
                        p => this.ItemIsSelected(p),
                        p => this.RemoveFromCart(p));
                }
                return _removeFromCartCommand;
            }
        }

        private ICommand _addMoneyCommand;
        public ICommand AddMoneyCommand
        {
            get
            {
                if (_addMoneyCommand == null)
                {
                    _addMoneyCommand = new RelayCommand(
                        p => true,
                        p => this.AddInsertedMoney(p));
                }
                return _addMoneyCommand;
            }
        }

        private ICommand _payCommand;
        public ICommand PayCommand
        {
            get
            {
                if (_payCommand == null)
                {
                    _payCommand = new RelayCommand(
                        p => ShoppingCart.Any() && EnteredMoney >= TotalPrice,
//                        p => true,
                        p => this.PayUp());
                }
                return _payCommand;
            }
        }

        public MainWindowViewModel()
        {
            PossibleTickets = Definitions.GetPossibleTickets();
            PossibleMoney = Definitions.GetPossibleMoney();

            ShoppingCart = new ObservableCollection<Ticket>();
            EnteredMoneyBits = new List<decimal>();
        }

        private void PayUp()
        {
            var change = EnteredMoney - TotalPrice;
            if (change >= 0)
            {
                string message = "Folgende Fahrscheine gekauft: " + Environment.NewLine + Environment.NewLine;
                ShoppingCart.ToList().ForEach(ticket => message += $"- {ticket.Amount}x {ticket.Type}{Environment.NewLine}");

                message += Environment.NewLine;
                message += $"Wechselgeld: {change:0.00}€";
                message += Environment.NewLine;
                message += CalculateChange();

                MessageBox.Show(message, "Nett Geschäfte mit dir zu machen :)", MessageBoxButton.OK, MessageBoxImage.Information);


                EnteredMoneyBits.Clear();
                ShoppingCart.Clear();
                RefillShoppinCart();
                RaisePropertyChanged("TotalPrice");
                RaisePropertyChanged("EnteredMoney");
            }
            else
            {
                MessageBox.Show("Nicht genügend Geld eingeworfen. :(", "NEED MOAR MONEY!!!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddInsertedMoney(object param)
        {
            decimal money = decimal.Parse((string)param);

            EnteredMoneyBits.Add(money);
            RaisePropertyChanged("EnteredMoney");
        }

        private void AddToCart(object p)
        {
            var ticket = (Ticket)p;
            var found = IsInShoppingCart((ticket));
            if(found == null)
            {
                ShoppingCart.Add(ticket);
                ticket.Amount++;
            }
            else
            {
                found.Amount++;
            }
            RefillShoppinCart();
        }

        public bool ItemIsSelected(object p) 
        {
            return p != null;
        }

        private void RemoveFromCart(object p)
        {
            var ticket = (Ticket)p;
            ticket.Amount--;

            if (ticket.Amount == 0)
            {
                ShoppingCart.Remove(ticket);
            }
            RefillShoppinCart();
        }

        private void RefillShoppinCart()
        {
            ObservableCollection<Ticket> tmpList = new ObservableCollection<Ticket>();

            foreach (var ticket in ShoppingCart)
            {
                tmpList.Add(ticket);
            }
            ShoppingCart.Clear();

            foreach (var ticket in tmpList)
            {
                ShoppingCart.Add(ticket);
            }
            tmpList.Clear();

            RaisePropertyChanged("ShoppingCart");
            RaisePropertyChanged("TotalPrice");
        }


        private Ticket IsInShoppingCart(Ticket ticket)
        {
            var found = ShoppingCart.Where(t => t.Type == ticket.Type);
            if(found.Any())
            {
                return found.FirstOrDefault();
            }
            return null;
        }

        private string CalculateChange()
        {
            StringBuilder sb = new StringBuilder();
            PossibleMoney.Sort();
            PossibleMoney.Reverse();
            var total = EnteredMoney - TotalPrice;

            PossibleMoney.ForEach(m =>
            {
                int amount = 0;
                while (total >= m)
                {
                    amount++;
                    total -= m;
                }
                if (amount > 0)
                {
                    sb.AppendLine($"{amount}x " + (m >= 1 ? $"{m} €" : $"{(m * 100):0} Cent"));
                }
            });
            return sb.ToString();
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public RelayCommand(Predicate<object> canExecute, Action<object> execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
