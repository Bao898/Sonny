// Licensed to the.NET Pile under one or more agreements.
// The.NET Pile licenses this file to you under the MIT license.

using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;


namespace SonnyBIM
{
    public partial class AdjustStrBeamWindow : Window
    {
        private readonly AdjustStrBeamViewModel _viewModel;
        public AdjustStrBeamWindow(AdjustStrBeamViewModel viewModel)
        {
            InitializeComponent();
            _viewModel= viewModel;
            DataContext = viewModel;

            viewModel.MainWindow = this;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryAccept();
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            TryAccept();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TryAccept()
        {
            // CHANGED: validate cả BeamBeamGapColumnMm + BeamBeamPerpendicularGapMm.
            if (_viewModel.BeamGapWallMm < 0
                || _viewModel.BeamGapColumnMm < 0
                || _viewModel.BeamBeamGapColumnMm < 0
                || _viewModel.BeamBeamPerpendicularGapMm < 0)
            {
                MessageBox.Show(
                    "Gap must be greater than or equal to 0 mm.",
                    "Adjust Structural Beam",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
