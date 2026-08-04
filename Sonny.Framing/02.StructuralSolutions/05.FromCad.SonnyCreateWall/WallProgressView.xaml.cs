// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows;
using System.Windows.Threading;
using Sonny.Application.Presentation.Extensions;

namespace SonnyBIM
{
    public partial class WallProgressView : Window
    {
        private readonly string _title;

        public WallProgressView(string title)
        {
            _title = title;
            InitializeComponent();
            this.Title = title; // Gán tiêu đề ngay lập tức
            this.SetOwnerByRevit();
        }

        public void UpdateProgress(int current, int total) =>
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Maximum = total;
                ProgressBar.Value = current;
                Title = $"{_title} ({current} / {total})";
            }, DispatcherPriority.Background);
    }
}
