// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows;
using System.Windows.Input;

namespace SonnyBIM
{
    public partial class FramingFromCadWindow : Window
    {
        private FramingFromCADViewModel _viewModel;

        public FramingFromCadWindow(FramingFromCADViewModel viewModel)
        {
            InitializeComponent();
            _viewModel= viewModel;
            DataContext = viewModel;

            viewModel.MainWindow = this;
        }

        #region Copy for All xaml.cs
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Gửi message đến hệ thống để bắt đầu di chuyển window từ non-client area (titlebar)
                SendMessage(new System.Runtime.InteropServices.HandleRef(this, new System.Windows.Interop.WindowInteropHelper(this).Handle),
                    0xA1, new IntPtr(2), IntPtr.Zero);
            }
            catch (Exception)
            {
                // Bỏ qua lỗi nếu có
            }
        }

        // Phương thức P/Invoke để gọi Windows API
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(System.Runtime.InteropServices.HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MaximizeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MaximizeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore;
            }
        }

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
            else if (e.Key == Key.F1)
            {
                BrowserHelper.OpenUrlWithBrowserOrCopyToClipboard("https://alphabimvn.com/en/all_plugins/");

            }
        }

        #endregion Copy for All xaml.cs

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Height = MainGrid.ActualHeight + 43;
            MinHeight = MainGrid.ActualHeight + 43;
        }

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

        // private void btnHelp_Click(object sender, RoutedEventArgs e)
        // {
        //     BrowserHelper.OpenUrlWithBrowserOrCopyToClipboard("https://youtu.be/MPgzN9JDJ5Q");
        // }
    }
}
