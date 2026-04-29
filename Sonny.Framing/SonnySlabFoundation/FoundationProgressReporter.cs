// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows.Threading;
using Sonny.Application.Domain.Services;

namespace SonnyBIM
{
    public class FoundationProgressReporter : IProgressReporter
    {
        private FoundationProgressView _progressView;

        public void Show(string title)
        {
            _progressView = new FoundationProgressView(title);
            _progressView.Show();
            // Đảm bảo cửa sổ được vẽ lên ngay lập tức
            Dispatcher.CurrentDispatcher.Invoke(delegate { }, DispatcherPriority.ContextIdle);
        }

        public void Update(int current, int total) => _progressView?.UpdateProgress(current, total);

        public void Close()
        {
            _progressView?.Close();
            _progressView = null;
        }
    }
}
