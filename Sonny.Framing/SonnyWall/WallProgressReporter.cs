// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows.Threading;
using Sonny.Application.Domain.Services;

namespace SonnyBIM
{
    public class WallProgressReporter : IProgressReporter
    {
        private WallProgressView _progressView;

        public void Show(string title)
        {
            _progressView = new WallProgressView(title);
            _progressView.Show();
            // Đảm bảo cửa sổ được vẽ lên ngay lập tức
            Dispatcher.CurrentDispatcher.Invoke(delegate { }, DispatcherPriority.ContextIdle);
        }

        public void Update(int current, int total)
        {
            _progressView?.UpdateProgress(current, total);

            Dispatcher.CurrentDispatcher.Invoke(delegate { }, DispatcherPriority.Background);
        }

        public void Close()
        {
            _progressView?.Close();
            _progressView = null;
        }
    }
}
