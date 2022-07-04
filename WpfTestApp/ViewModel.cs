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
using HtmlAgilityPack;
using System.IO;

namespace WpfTestApp
{
    class Model : IComparable<Model>, INotifyPropertyChanged
    {
        private bool isMaximal;

        public Uri Url { get; }
        public int AnchorCount { get; }

        public bool IsMaximal
        {
            get => isMaximal;
            set
            {
                isMaximal = value;
                OnPropertyChanged("IsMaximal");
            }
        }
        public Model(Uri url, int count)
        {
            Url = url;
            OnPropertyChanged("Url");
            AnchorCount = count;
        }

        public int CompareTo(Model other)
        {
            return this.AnchorCount.CompareTo(other.AnchorCount);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    internal class ViewModel: INotifyPropertyChanged
    {
        private int _currentCycle;
        private HttpClient _httpClient = new HttpClient();
        private IProgress<int> _progress;
        private string _message = string.Empty;
        private Model _selectedModel, _maxUrlModel;

        public string[] Urls { get; set; }
        public ObservableCollection<Model> UrlAnchorCounters { get; } = new ObservableCollection<Model>();
        public Model SelectedModel
        { 
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                OnPropertyChanged("SelectedModel");
            }
        }


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
                    Stream stream = await _httpClient.GetStreamAsync(url);
                    HtmlDocument document = new();
                    document.Load(stream);
                    Model model = new Model(new Uri(url), document.DocumentNode.SelectNodes("//a[@href]").Count);

                    if (_maxUrlModel == null || _maxUrlModel.AnchorCount <model.AnchorCount)
                    {
                        if (_maxUrlModel != null)
                        {
                            _maxUrlModel.IsMaximal = false;
                        }
                        model.IsMaximal=true;
                        _maxUrlModel=model;
                    }

                    UrlAnchorCounters.Add(model);

                }
                catch (Exception e) { _message += e.Message + "\n" + url + "\n"; }

            _progress.Report(++_currentCycle);

        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
