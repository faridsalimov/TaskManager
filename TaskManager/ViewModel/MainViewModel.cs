using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using TaskManager.Commands;

namespace TaskManager.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public RelayCommand EndCommand { get; set; }
        public RelayCommand CreateCommand { get; set; }
        public RelayCommand AddBlacklistCommand { get; set; }

        private ObservableCollection<Process> processes = new ObservableCollection<Process>();
        public ObservableCollection<Process> Processes
        {
            get { return processes; }
            set { processes = value; OnPropertyChanged(); }
        }

        private Process selectedProcess;
        public Process SelectedProcess
        {
            get { return selectedProcess; }
            set { selectedProcess = value; OnPropertyChanged(); }
        }

        private DispatcherTimer _dispatcherTimer;

        private string newProcess;
        public string NewProcess
        {
            get { return newProcess; }
            set { newProcess = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> blacklistProcesses = new ObservableCollection<string>();
        public ObservableCollection<string> BlacklistProcesses
        {
            get { return blacklistProcesses; }
            set { blacklistProcesses = value; OnPropertyChanged(); }
        }

        private string blacklistProcess;
        public string BlacklistProcess
        {
            get { return blacklistProcess; }
            set { blacklistProcess = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            UpdateProcesses(null, null);
            DispatcherTimerSetup();

            EndCommand = new RelayCommand((obj) =>
            {
                if (SelectedProcess != null)
                {
                    try
                    {
                        SelectedProcess.Kill();
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });

            CreateCommand = new RelayCommand((obj) =>
            {
                string process = NewProcess;
                if (!process.EndsWith(".exe")) { process += ".exe"; }

                try
                {
                    Process.Start(process);
                    NewProcess = String.Empty;
                }

                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to start process named {process}.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            AddBlacklistCommand = new RelayCommand((obj) =>
            {
                if (BlacklistProcess != null)
                {
                    BlacklistProcesses.Add(BlacklistProcess);
                }

                else
                {
                    MessageBox.Show("Blacklist textbox section looks blank.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        public void DispatcherTimerSetup()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += UpdateProcesses;
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(5);
            _dispatcherTimer.Start();
        }

        public void UpdateProcesses(object sender, EventArgs e)
        {
            Processes = new ObservableCollection<Process>(Process.GetProcesses().OrderBy(p => p.ProcessName));
            Processes.ToList().Where(p => BlacklistProcesses.Contains(p.ProcessName)).ToList().ForEach(pr => pr.Kill());
        }
    }
}
