using AccraRoadAttendance.Models;
using AccraRoadAttendance.Services;
using AccraRoadAttendance.Views.Pages.Reports;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AccraRoadAttendance.Views.Pages.Members
{
    /// <summary>
    /// Interaction logic for MemberDetails.xaml
    /// </summary>
    public partial class MemberDetails : UserControl, IParameterReceiver
    {
        // DependencyProperty for Member
        public static readonly DependencyProperty MemberProperty = DependencyProperty.Register(
            nameof(Member),
            typeof(Member),
            typeof(MemberDetails),
            new PropertyMetadata(null));

        public Member Member
        {
            get { return (Member)GetValue(MemberProperty); }
            set { SetValue(MemberProperty, value); }
        }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is Member member)
            {
                Member = member; // Set the Member property when receiving the parameter
            }
        }

        public MemberDetails()
        {
            InitializeComponent();
            DataContext = this; // Bind to the UserControl itself
        }

        private void PrintToPdf_Click(object sender, RoutedEventArgs e)
        {
            if (Member == null)
            {
                MessageBox.Show("No member data to print.");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"{Member.FullName}_Details_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var generator = new ReportGenerator();
                    generator.GenerateMemberDetailsReport(Member, saveFileDialog.FileName);
                    MessageBox.Show($"PDF saved to {saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating PDF: {ex.Message}");
                }
            }
        }
    }
}
