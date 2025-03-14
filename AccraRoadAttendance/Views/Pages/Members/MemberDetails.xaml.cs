﻿using AccraRoadAttendance.Models;
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
    public partial class MemberDetails : UserControl
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

        public MemberDetails()
        {
            InitializeComponent();
            DataContext = this; // Bind to the UserControl itself
        }
    }
}
