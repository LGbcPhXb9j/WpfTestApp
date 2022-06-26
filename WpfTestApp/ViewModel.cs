using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows;

namespace WpfTestApp
{
    class UrlAnchorCounter : IComparable<UrlAnchorCounter>
    {
        public string Url { get; }
        public int AnchorCount { get; }
        public bool IsMaximal { get; set; }
        public UrlAnchorCounter(string url, int count)
        {
            Url = url;
            AnchorCount = count;
        }

        public int CompareTo(UrlAnchorCounter other)
        {
            return this.AnchorCount.CompareTo(other.AnchorCount);
        }
    }
    internal class ViewModel
    {
        private int _currentCycle;
        private HttpClient _httpClient = new HttpClient();
        private IProgress<int> _progress;
        private string _message = string.Empty;

        public string[] Urls { get; set; }
        public ObservableCollection<UrlAnchorCounter> UrlAnchorCounters { get; } = new ObservableCollection<UrlAnchorCounter>();

        public ViewModel(IProgress<int> progress)
        {
            _progress = progress;
        }
        public async Task CountAnchorsAsync(CancellationToken cancellationToken)
        {
            if (Urls == null)
                throw new ArgumentNullException(nameof(Urls));
            UrlAnchorCounters.Clear();
            _message = string.Empty;
            _progress.Report(_currentCycle = 0);
            try
            {
                foreach (string url in Urls)
                {
                    await CreateAnchorCounterAsync(url);
                    cancellationToken.ThrowIfCancellationRequested();
                }

            }
            catch (OperationCanceledException e)
            {

                _message += e.Message + "\n";
            }

            UrlAnchorCounters.Max().IsMaximal = true;

            if (_message != string.Empty)
            {
                MessageBox.Show(_message);

            }
        }
        public async Task CreateAnchorCounterAsync(string url)
        {

            if (url != string.Empty)

                try
                {
                    string data = await _httpClient.GetStringAsync(url.Replace("\n", string.Empty).Replace(" ", string.Empty));
                    Regex regex = new("<a [^>]*href=(?:'(?<href>.*?)')|(?:\"(?<href>.*?)\")", RegexOptions.IgnoreCase);
                    UrlAnchorCounters.Add(new UrlAnchorCounter(url, regex.Matches(data).Count));
                }
                catch (Exception e) { _message += e.Message + "\n" + url + "\n"; }

            _progress.Report(++_currentCycle);

        }
    }
}
