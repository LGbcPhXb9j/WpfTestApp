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
        ViewModel viewModel=new();
        CancellationTokenSource cancellationTokenSource = new();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Value = 0;
            Progress<int> progress = new();
            progress.ProgressChanged += RefreshProgress;
            await viewModel.CountAnchorsAsync(progress,cancellationTokenSource.Token);
        }
        private void RefreshProgress(object sender, int e)
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
                viewModel.Urls= File.ReadAllText(openFileDialog.FileName).Split("\n");
                progressBar.Maximum = viewModel.Urls.Length;


            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            browser.Navigate(((UrlAnchorCounter)listView.SelectedItem).Url);
        }
    }
}
