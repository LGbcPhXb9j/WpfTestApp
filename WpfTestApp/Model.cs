using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfTestApp
{
    class Model : IComparable<Model>, INotifyPropertyChanged
    {
        private bool _isMaximal;

        public Uri Url { get; }
        public int AnchorCount { get; }

        public bool IsMaximal
        {
            get => _isMaximal;
            set
            {
                _isMaximal = value;
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
}
