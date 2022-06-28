using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Progress<int> progress = new();
        private CancellationTokenSource cancellationTokenSource = new();
        private ViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new(progress);
            DataContext = viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.CancelAfter(2000);
            //cancellationTokenSource.Cancel();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource = new();
            progressBar.Value = 0;
            progress.ProgressChanged += TrackProgress;
            browser.Source=new Uri("about:blank");
            await viewModel.CountAnchorsAsync(cancellationTokenSource.Token);
            progress.ProgressChanged -= TrackProgress;
        }
        private void TrackProgress(object sender, int e)
        {
            progressBar.Value = e;
        }
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            string https = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                viewModel.Urls = File.ReadAllText(openFileDialog.FileName).Split("\n");
                progressBar.Maximum = viewModel.Urls.Length;


            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object listViewSelectedItem = listView.SelectedItem;
            if (listViewSelectedItem != null)
            {

                browser.Source=new Uri(((UrlAnchorCounter)listViewSelectedItem).Url);
            }
        }
    }
}
