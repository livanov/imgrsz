using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace ImageCompressor_v1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Thread RunningThread { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SourceButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            SourceBox.Text = dialog.SelectedPath;
        }

        private void DestinationButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            DestinationBox.Text = dialog.SelectedPath;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DestinationBox.Text = SourceBox.Text;
            DestinationBox.IsEnabled = false;
            DestinationButton.IsEnabled = false;
        }

        private void OverwriteBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            DestinationBox.IsEnabled = true;
            DestinationButton.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var srcDir = SourceBox.Text.Trim(' ', '"');
            var destDir = DestinationBox.Text.Trim(' ', '"');

            if (srcDir.Contains(destDir))
            {
                MessageBox.Show("Destination directory can't be within source directory");
            }

            if (OverwriteBox.IsChecked != null && !OverwriteBox.IsChecked.Value)
            {
                destDir = Path.Combine(destDir, new DirectoryInfo(srcDir).Name);
            }

            if (string.IsNullOrEmpty(srcDir) || string.IsNullOrEmpty(destDir))
            {
                MessageBox.Show("Missing field");
                return;
            }

            if (!Directory.Exists(srcDir))
            {
                MessageBox.Show("Source directory does not exist.");
            }

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            if ((string)StartButton.Content == "Start")
            {
                StartButton.Content = "STOP";

                RunningThread = new Thread(() =>
                {
                    var imgCompressor = new ImageCompressor(srcDir);
                    imgCompressor.Compress(destDir, true);
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        StartButton.Content = "Start";
                        MessageBox.Show("Done!", "Task completed", MessageBoxButton.OK);
                    }));
                });
                RunningThread.Start();
            }
            else
            {
                StartButton.Content = "Start";

                if (RunningThread != null && RunningThread.IsAlive)
                {
                    MessageBox.Show("Abort!", "Task aborted", MessageBoxButton.OK);
                    RunningThread.Abort();
                }
            }

        }
    }
}
