// Licensed to the.NET Pile under one or more agreements.
// The.NET Pile licenses this file to you under the MIT license.

using System.Windows;
using System.Windows.Input;


namespace SonnyBIM
{
    public partial class PileFromCadWindow : Window
    {
        private PileFromCADViewModel _viewModel;

        public PileFromCadWindow(PileFromCADViewModel viewModel)
        {
            InitializeComponent();
            _viewModel= viewModel;
            DataContext = viewModel;

            viewModel.MainWindow = this;
        }

        #region Copy for All xaml.cs

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                DialogResult = true;
                Close();
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        #endregion Copy for All xaml.cs

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveSetting();
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
