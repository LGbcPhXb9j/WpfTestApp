﻿using System;
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
    class UrlAnchorCounter:IComparable<UrlAnchorCounter>
    {
        public string Url { get; }
        public int AnchorCount { get;}
        public bool Maximal { get; set; }
        public UrlAnchorCounter(string url,int count)
        {
            Url = url;
            AnchorCount = count;
        }

        public int CompareTo(UrlAnchorCounter? other)
        {
            return this.AnchorCount.CompareTo(other.AnchorCount);
        }
    }
    public class ProgressActuator
    {
        public int CurrentProgress { get; set; } = 0;
    }
    internal class ViewModel : INotifyPropertyChanged
    {
        int _progress;

        public string[] Urls { get; set; }
        public ObservableCollection<UrlAnchorCounter> UrlAnchorCounters { get; set; } = new ObservableCollection<UrlAnchorCounter>();

        public async Task CountAnchorsAsync(IProgress<int> progress,CancellationToken cancellationToken)
        {
            if (Urls==null)
                throw new ArgumentNullException(nameof(Urls));
            UrlAnchorCounters.Clear();
            _progress = 0;
            List<Task> tasks = new List<Task>();
            
            foreach (string url in Urls)
            {
                tasks.Add (CreateAnchorCounter(progress, cancellationToken, url));
            }
            await Task.WhenAll(tasks);
            UrlAnchorCounters.Max().Maximal=true;
        }
        public async Task CreateAnchorCounter(IProgress<int> progress, CancellationToken cancellationToken,string url) 
        {
            if(url!=string.Empty)
            {
                cancellationToken.ThrowIfCancellationRequested();
                HttpClient httpClient = new HttpClient();
                string data = await httpClient.GetStringAsync(url.Replace("\n", string.Empty).Replace(" ", string.Empty));
                Regex regex = new("<a [^>]*href=(?:'(?<href>.*?)')|(?:\"(?<href>.*?)\")", RegexOptions.IgnoreCase);
                UrlAnchorCounters.Add(new UrlAnchorCounter(url, regex.Matches(data).Count));
            }
            progress.Report(++_progress);
        }

        public event PropertyChangedEventHandler PropertyChanged; 
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}