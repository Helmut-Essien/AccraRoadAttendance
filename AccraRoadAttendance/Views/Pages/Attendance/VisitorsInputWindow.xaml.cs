using System.Windows;

namespace AccraRoadAttendance.Views.Pages.Attendance
{
    public partial class VisitorsInputWindow : Window
    {
        public int Visitors { get; private set; }
        public int Children { get; private set; }
        public decimal OfferingAmount { get; private set; }
        public string ServiceTheme { get; private set; }

        public VisitorsInputWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(VisitorsTextBox.Text, out int visitors) &&
                int.TryParse(ChildrenTextBox.Text, out int children) &&
                decimal.TryParse(OfferingTextBox.Text, out decimal offering))
            {
                Visitors = visitors;
                Children = children;
                OfferingAmount = offering;
                ServiceTheme = ThemeTextBox.Text;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter valid numbers for Visitors, Children, and Offering Amount.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}