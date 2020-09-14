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
        public decimal EnteredMoneyTotal => EnteredMoneyBits.Sum(); 
        public decimal TotalPrice => ShoppingCart.Select(ticket => ticket.Price*ticket.Amount).Sum(); 

        /// <summary>
        /// Adds selected ticket to shopping cart
        /// </summary>
        private ICommand _addToCartCommand;
        public ICommand AddToCartCommand
        {
            get
            {
                if (_addToCartCommand == null)
                {
                    _addToCartCommand = new RelayCommand(
                        p => this.ItemIsSelected(p),
                        p => this.AddToCart((Ticket)p));
                }
                return _addToCartCommand;
            }
        }
        
        /// <summary>
        /// Removes selected ticket from shopping cart
        /// </summary>
        private ICommand _removeFromCartCommand;
        public ICommand RemoveFromCartCommand
        {
            get
            {
                if (_removeFromCartCommand == null)
                {
                    _removeFromCartCommand = new RelayCommand(
                        p => this.ItemIsSelected(p),
                        p => this.RemoveFromCart((Ticket)p));
                }
                return _removeFromCartCommand;
            }
        }

        /// <summary>
        /// If any Ticket is in the shopping Cart, add money 
        /// </summary>
        private ICommand _addMoneyCommand;
        public ICommand AddMoneyCommand
        {
            get
            {
                if (_addMoneyCommand == null)
                {
                    _addMoneyCommand = new RelayCommand(
                        p => ShoppingCart.Any(),
                        p => this.AddInsertedMoney(decimal.Parse((string)p)));
                }
                return _addMoneyCommand;
            }
        }

        /// <summary>
        /// Eject all the entered Money from the terminal
        /// </summary>
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

        /// <summary>
        /// Command to Pay the tickets. 
        /// Button is only enabled when at least one ticket is in shopping cart and enough money was entered
        /// </summary>
        private ICommand _payCommand;
        public ICommand PayCommand
        {
            get
            {
                if (_payCommand == null)
                {
                    _payCommand = new RelayCommand(
                        p => ShoppingCart.Any() && EnteredMoneyTotal >= TotalPrice,
//                        p => true,
                        p => this.PayUp());
                }
                return _payCommand;
            }
        }

        /// <summary>
        /// CTOR - Creates a new MainWindowViewModel Object
        /// </summary>
        public MainWindowViewModel()
        {
            PossibleTickets = Definitions.GetPossibleTickets();
            PossibleMoney = Definitions.GetPossibleMoney();

            ShoppingCart = new ObservableCollection<Ticket>();
            EnteredMoneyBits = new List<decimal>();
        }

        /// <summary>
        /// Buy the Ticket and pay up
        /// Shows MessageBox with the selected tickets and calculates the change money
        /// </summary>
        private void PayUp()
        {
            var change = EnteredMoneyTotal - TotalPrice;
            if (change >= 0)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine("Folgende Fahrscheine gekauft: " + Environment.NewLine);
                ShoppingCart.ToList().ForEach(ticket => message.AppendLine($"- {ticket.Amount}x {ticket.Type}"));

                message.AppendLine();
                message.AppendLine($"Wechselgeld: {change:0.00}€");
                message.AppendLine();
                message.AppendLine(GetStringFromMoneyList(CalculateChange()));

                MessageBox.Show(message.ToString(), "Nett Geschäfte mit dir zu machen :)", MessageBoxButton.OK, MessageBoxImage.Information);

                EnteredMoneyBits.Clear();
                ShoppingCart.ToList().ForEach(ticket => ticket.Amount = 0);
                ShoppingCart.Clear();
                RefillShoppingCart();
                RaisePropertyChanged(nameof(TotalPrice));
                RaisePropertyChanged(nameof(EnteredMoneyTotal));
            }
            else
            {
                MessageBox.Show("Nicht genügend Geld eingeworfen. :(", "NEED MOAR MONEY!!!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Eject the entered money
        /// Show MessageBox with the money to be ejected
        /// </summary>
        private void EjectMoney()
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine($"Eingezahltes Geld wird zurückgegeben: {EnteredMoneyTotal:0.00}€");
            message.AppendLine();
            message.AppendLine(GetStringFromMoneyList(EnteredMoneyBits));

            MessageBox.Show(message.ToString(), "Schade, dann halt nicht. :(", MessageBoxButton.OK, MessageBoxImage.Information);

            EnteredMoneyBits.Clear();
            RaisePropertyChanged(nameof(EnteredMoneyTotal));
        }

        private void AddInsertedMoney(decimal money)
        {
            EnteredMoneyBits.Add(money);
            RaisePropertyChanged(nameof(EnteredMoneyTotal));
        }

        private void AddToCart(Ticket ticket)
        {
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

        private bool ItemIsSelected(object param) 
        {
            return param != null;
        }

        private void RemoveFromCart(Ticket param)
        {
            var ticket = param;
            ticket.Amount--;

            if (ticket.Amount == 0)
            {
                ShoppingCart.Remove(ticket);
            }
            RefillShoppingCart();
        }

        /// <summary>
        /// Needed to trigger Changed event on ObservableCollection. 
        /// Changing a property of an element in the list is not enough.
        /// </summary>
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
            
            RaisePropertyChanged(nameof(ShoppingCart));
            RaisePropertyChanged(nameof(TotalPrice));
        }

        /// <summary>
        /// Checks if a Ticket is already in the ShoppingCart and returns it
        /// </summary>
        /// <param name="ticket">Tickettype to search for</param>
        /// <returns>Ticket if found or null if not</returns>
        private Ticket IsInShoppingCart(Ticket ticket)
        {
            var found = ShoppingCart.Where(t => t.Type == ticket.Type);
            if(found.Any())
            {
                return found.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Calculate Change-List of the differerence between EnteredMoney and the TotalPrice
        /// </summary>
        /// <returns>List decimals representig Money</returns>
        private List<decimal> CalculateChange()
        {
            List<decimal> moneyList = new List<decimal>();

            var total = EnteredMoneyTotal - TotalPrice;
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

        /// <summary>
        /// Generates a string from a List of decimals. Groups and counts values 
        /// (e.g. 2x 50 Cent)
        /// </summary>
        /// <param name="moneyList">List of decimals representing money (coins and bills)</param>
        /// <returns>Formated string with the content of the given List.</returns>
        private string GetStringFromMoneyList(List<decimal> moneyList)
        {
            StringBuilder sb = new StringBuilder();
            moneyList.GroupBy(m => m).ToList()
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

    /// <summary>
    /// Class that represents a command for executing a function.
    /// Includes ability to disable the button in some conditions ("canExecute").
    /// </summary>
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
