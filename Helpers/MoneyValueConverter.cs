using System;
using System.Globalization;
using System.Windows.Data;

namespace Fahrkartenautomat
{
    public class MoneyValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var amount = (decimal)value;

            if(amount < 1)
            {
                amount *= 100;
                return $"{(int)amount} ct";
            }
            return $"{amount} €";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
