using AccraRoadAttendance.Data;
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
using AccraRoadAttendance.Models;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.ComponentModel;

namespace AccraRoadAttendance.Views.Pages.Members
{
   
    /// <summary>
    /// Interaction logic for AddMembers.xaml
    /// </summary>
    public partial class AddMembers : UserControl, INotifyPropertyChanged
    {
        private readonly AttendanceDbContext _context;
        public AddMembers(AttendanceDbContext context)
        {
            InitializeComponent();
            _context = context;
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void SaveMember_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Member saved successfully!");
            // Add logic to save member to the database
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Action canceled!");
            // Add logic to navigate back or clear the form
        }

        private string _selectedPicturePath;
        public string SelectedPicturePath
        {
            get => _selectedPicturePath;
            set
            {
                _selectedPicturePath = value;
                OnPropertyChanged(nameof(SelectedPicturePath));
            }
        }


        private void UploadPicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select a Picture"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedPicturePath = openFileDialog.FileName;

                // Load the selected image into the ImagePreview control
                ImagePreview.Source = new BitmapImage(new Uri(SelectedPicturePath));
            }
        }
    }
}
