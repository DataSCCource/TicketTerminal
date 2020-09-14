using Fahrkartenautomat.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
        public decimal EnteredMoney => EnteredMoneyBits.Sum(); 
        public decimal TotalPrice => ShoppingCart.Select(ticket => ticket.Price*ticket.Amount).Sum(); 

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
                        p => ShoppingCart.Any(),
                        p => this.AddInsertedMoney(p));
                }
                return _addMoneyCommand;
            }
        }

        private ICommand _ejectMoneyCommand;
        public ICommand EjectMoneyCommand
        {
            get
            {
                if (_ejectMoneyCommand == null)
                {
                    _ejectMoneyCommand = new RelayCommand(
                        p => EnteredMoneyBits.Any(),
                        p => this.EjectMoney());
                }
                return _ejectMoneyCommand;
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
                StringBuilder message = new StringBuilder();
                message.AppendLine("Folgende Fahrscheine gekauft: " + Environment.NewLine);
                ShoppingCart.ToList().ForEach(ticket => message.AppendLine($"- {ticket.Amount}x {ticket.Type}"));

                message.AppendLine();
                message.AppendLine($"Wechselgeld: {change:0.00}€");
                message.AppendLine();

                message.AppendLine(GetStringFromMoneyList(CalculateChange()));
                //message.AppendLine(CalculateChange());

                MessageBox.Show(message.ToString(), "Nett Geschäfte mit dir zu machen :)", MessageBoxButton.OK, MessageBoxImage.Information);

                EnteredMoneyBits.Clear();
                ShoppingCart.ToList().ForEach(ticket => ticket.Amount = 0);
                ShoppingCart.Clear();
                RefillShoppingCart();
                RaisePropertyChanged("TotalPrice");
                RaisePropertyChanged("EnteredMoney");
            }
            else
            {
                MessageBox.Show("Nicht genügend Geld eingeworfen. :(", "NEED MOAR MONEY!!!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EjectMoney()
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine($"Eingezahltes Geld wird zurückgegeben: {EnteredMoney:0.00}€");
            message.AppendLine();
            message.AppendLine(GetStringFromMoneyList(EnteredMoneyBits));

            MessageBox.Show(message.ToString(), "Geld wird ausgegeben :)", MessageBoxButton.OK, MessageBoxImage.Information);

            EnteredMoneyBits.Clear();
            RaisePropertyChanged("EnteredMoney");
        }

        private void AddInsertedMoney(object param)
        {
            decimal money = decimal.Parse((string)param);

            EnteredMoneyBits.Add(money);
            RaisePropertyChanged("EnteredMoney");
        }

        private void AddToCart(object param)
        {
            var ticket = (Ticket)param;
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
            RefillShoppingCart();
        }

        public bool ItemIsSelected(object param) 
        {
            return param != null;
        }

        private void RemoveFromCart(object param)
        {
            var ticket = (Ticket)param;
            ticket.Amount--;

            if (ticket.Amount == 0)
            {
                ShoppingCart.Remove(ticket);
            }
            RefillShoppingCart();
        }

        // Needed to trigger Changed event on ObservableCollection. 
        // Changing a property of an element in the list is not enough.
        private void RefillShoppingCart()
        {
            var tmpList = new ObservableCollection<Ticket>();

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

        private List<decimal> CalculateChange()
        {
            List<decimal> moneyList = new List<decimal>();

            var total = EnteredMoney - TotalPrice;
            PossibleMoney.OrderByDescending(x=>x).ToList().ForEach(m =>
            {
                while (total >= m)
                {
                    total -= m;
                    moneyList.Add(m);
                }
            });
            return moneyList;
        }

        private string GetStringFromMoneyList(List<decimal> moneyList)
        {
            StringBuilder sb = new StringBuilder();
            moneyList.GroupBy(m => m)
                     .ToList()
                     .ForEach(m =>
                         sb.AppendLine($"{m.Count()}x " + (m.Key >= 1 ? $"{m.Key} €" : $"{(m.Key * 100):0} Cent"))
                     );

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
