using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.ObjectModel;
using System.Windows;
using HtmlAgilityPack;
using System.IO;
using Microsoft.Win32;

namespace WpfTestApp
{
    internal class ViewModel : INotifyPropertyChanged
    {
        private static HttpClient _httpClient = new HttpClient();
        private string _message = string.Empty;
        private Model _selectedModel, _maxUrlModel;
        private Command _gerFilePath,_startOrStop,_close;
        private bool _isRunning;
        private CancellationTokenSource _cancellationTokenSource;

        public string FileLocation { get; private set; } = "No file selected";
        public string ButtonName => _isRunning ? "Stop" : "Start";

        public ObservableCollection<Model> UrlAnchorCounters { get; } = new ObservableCollection<Model>();
        public int CurrentProgress { get; set; }
        public Model SelectedModel
        { 
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                OnPropertyChangedAsync("SelectedModel");
            }
        }
        
        public Command GetFilePath
        {
            get
            {
                return _gerFilePath ??
                    (_gerFilePath = new Command(obj =>
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "Text files (*.txt)|*.txt";

                        if (openFileDialog.ShowDialog() == true)
                        {
                            FileLocation = openFileDialog.FileName;
                            OnPropertyChangedAsync("FileLocation");

                        }

                    },obj => !_isRunning));
            }
        }
        public Command StartOrStop
        {
            get
            {
                return _startOrStop ??
                    (_startOrStop = new Command(obj =>
                    {
                        try
                        {
                            CountAnchorsAsync();
                        }
                        catch (Exception e)
                        {

                           _message+=e.Message+"\n";
                        }
                    }, obj => FileLocation != "No file selected"));
            }
        }
        public Command Close
        {
            get
            {
                return _close ??
                    (_close = new Command(window => ((Window)window).Close()));
            }
        }

        public async Task CountAnchorsAsync()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                OnPropertyChangedAsync("ButtonName");
                _maxUrlModel = null;
                _cancellationTokenSource = new();
                string[] urls = await File.ReadAllLinesAsync(FileLocation);
                UrlAnchorCounters.Clear();
                _message = string.Empty;
                CurrentProgress = 0;
                OnPropertyChangedAsync("CurrentProgress");
                Task[] tasks = new Task[urls.Length];
                
                for (int i = 0; i < urls.Length; i++)
                {
                    tasks[i] = CreateAnchorCounter(urls[i]);
                    CurrentProgress = ((i+1) * 100) / urls.Length;
                    OnPropertyChangedAsync("CurrentProgress");
                }
                await Task.WhenAll(tasks);
            }
           else _cancellationTokenSource.Cancel();
            _isRunning = false;
            OnPropertyChangedAsync("ButtonName");
            if (_message != string.Empty)
                MessageBox.Show(_message);
            _cancellationTokenSource.Dispose();


        }
        public async Task CreateAnchorCounter(string url)
        {

            if (url != string.Empty)

                try
                {
                    //await Task.Run(()=>Thread.Sleep(5000));
                    string site= await _httpClient.GetStringAsync(url,_cancellationTokenSource.Token);
                    HtmlDocument document = new();
                    document.LoadHtml(site);
                    HtmlNodeCollection htmlNodeCollection = document.DocumentNode.SelectNodes("//a[@href]");
                    int count= htmlNodeCollection==null?0:htmlNodeCollection.Count;
                    Model model = new Model(new Uri(url), count);

                        if (_maxUrlModel == null || _maxUrlModel.AnchorCount < model.AnchorCount)
                        {
                            if (_maxUrlModel != null)
                            {
                                _maxUrlModel.IsMaximal = false;
                            }
                            model.IsMaximal = true;
                            _maxUrlModel = model;
                        }

                        UrlAnchorCounters.Add(model);
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                }
                catch (Exception e) { _message += e.Message + "\n" + url + "\n"; }



        }
        public event PropertyChangedEventHandler PropertyChanged;
        public async void OnPropertyChangedAsync([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
