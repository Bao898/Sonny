// Licensed to the.NET Pile under one or more agreements.
// The.NET Pile licenses this file to you under the MIT license.

using System.Windows;
using System.Windows.Threading;
using Sonny.Application.Presentation.Extensions;

namespace SonnyBIM
{
    public partial class PileCoordinateProgressView : Window
    {
        private readonly string _title;

        public PileCoordinateProgressView(string title)
        {
            _title = title;
            InitializeComponent();
            this.Title = title; // Gán tiêu đề ngay lập tức
            this.SetOwnerByRevit();
        }

        public void UpdateProgress(int current, int total) =>
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                if (total <= 0)
                {
                    ProgressBar.Maximum = 100;
                    ProgressBar.Value = 0;
                    PercentText.Text = "0%";
                    return;
                }
                ProgressBar.Maximum = total;
                ProgressBar.Value = current;
                var percent = current * 100.0 / total;
                PercentText.Text = $"{percent:0}%";

                // ProgressBar.Maximum = total;
                // ProgressBar.Value = current;
                // Title = $"{_title} ({current} / {total})";
                // ProgressText.Text = $"{current} / {total}";
            }, DispatcherPriority.Background);
    }
}
