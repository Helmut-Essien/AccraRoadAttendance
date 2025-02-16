using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AccraRoadAttendance.Views.Pages.Attendance
{
    public partial class VisitorsInputWindow : Window, INotifyPropertyChanged
    {
        private int _visitors;
        private int _children;
        private decimal _offeringAmount;
        private string _serviceTheme = string.Empty;

        public VisitorsInputWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public int Visitors
        {
            get => _visitors;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Visitors cannot be negative");
                _visitors = value;
                OnPropertyChanged(nameof(Visitors));
            }
        }

        public int Children
        {
            get => _children;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Children cannot be negative");
                _children = value;
                OnPropertyChanged(nameof(Children));
            }
        }

        public decimal OfferingAmount
        {
            get => _offeringAmount;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Offering amount cannot be negative");
                _offeringAmount = value;
                OnPropertyChanged(nameof(OfferingAmount));
            }
        }

        public string ServiceTheme
        {
            get => _serviceTheme;
            set
            {
                _serviceTheme = value?.Trim() ?? string.Empty;
                OnPropertyChanged(nameof(ServiceTheme));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void IntegerValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var fullText = textBox?.Text.Insert(textBox.CaretIndex, e.Text) ?? string.Empty;
            e.Handled = !decimal.TryParse(fullText, NumberStyles.Currency, CultureInfo.CurrentCulture, out _);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate required fields
                this.Visitors = int.Parse(VisitorsTextBox.Text);
                this.Children = int.Parse(ChildrenTextBox.Text);
                this.OfferingAmount = decimal.Parse(OfferingTextBox.Text, NumberStyles.Currency);

                // Optional field
                this.ServiceTheme = ThemeTextBox.Text;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Please fix validation errors:\n{ex.Message}",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}