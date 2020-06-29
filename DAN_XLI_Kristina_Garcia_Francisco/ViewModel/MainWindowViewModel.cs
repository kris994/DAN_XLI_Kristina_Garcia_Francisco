using DAN_XLI_Kristina_Garcia_Francisco.Commands;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Input;

namespace DAN_XLI_Kristina_Garcia_Francisco.ViewModel
{
    /// <summary>
    /// Main Window view model
    /// </summary>
    class MainWindowViewModel : BaseViewModel
    {
        readonly MainWindow main;
        private readonly BackgroundWorker bgWorker = new BackgroundWorker();
        private string fileName = "";
        private readonly object locker = new object();
        private bool _isRunning = false;

        #region Constructor
        /// <summary>
        /// Constructor with Main Window param
        /// </summary>
        /// <param name="mainOpen">opens the main window</param>
        public MainWindowViewModel(MainWindow mainOpen)
        {
            main = mainOpen;
            bgWorker.DoWork += WorkerOnDoWork;
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.ProgressChanged += WorkerOnProgressChanged;
            bgWorker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
        }
        #endregion

        #region Properties
        private string document;
        public string Document
        {
            get
            {
                return document;
            }
            set
            {
                document = value;
                OnPropertyChanged("Document");
            }
        }

        private string copy;
        public string Copy
        {
            get
            {
                return copy;
            }
            set
            {
                copy = value;
                OnPropertyChanged("Copy");
            }
        }

        private string info;
        public string Info
        {
            get
            {
                return info;
            }
            set
            {
                info = value;
                OnPropertyChanged("Info");
            }
        }

        private string errInfo;
        public string ErrInfo
        {
            get
            {
                return errInfo;
            }
            set
            {
                errInfo = value;
                OnPropertyChanged("ErrInfo");
            }
        }

        private int currentProgress;
        public int CurrentProgress
        {
            get
            {
                return currentProgress;
            }
            set
            {
                if (currentProgress != value)
                {
                    currentProgress = value;
                    OnPropertyChanged("CurrentProgress");
                }
            }
        }
        #endregion

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CurrentProgress = e.ProgressPercentage;
           
            if (CurrentProgress == 100)
            {
                Info = "Finished printing";
            }
            else
            {
                Info = CurrentProgress.ToString() + "%";
            }
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            // Save all the routes to file   
            for (int i = 1; i < int.Parse(copy) + 1; i++)
            {
                Thread.Sleep(1000);

                // Check if the cancellation is requested
                if (bgWorker.CancellationPending)
                {
                    // Set Cancel property of DoWorkEventArgs object to true
                    e.Cancel = true;
                    // Reset progress percentage to ZERO and return
                    bgWorker.ReportProgress(0);
                    return;
                }    
                
                fileName = i + "." + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" +
                    DateTime.Now.Year + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute;

                using (StreamWriter streamWriter = new StreamWriter(fileName))
                {
                    streamWriter.WriteLine(document);
                }

                // Calling ReportProgress() method raises ProgressChanged event
                // To this method pass the percentage of processing that is complete
                if (i == int.Parse(copy))
                {
                    // 100% if all copies are printed
                    bgWorker.ReportProgress(100);
                }
                else
                {
                    bgWorker.ReportProgress((100 / int.Parse(copy)) * i);
                }
            }

            ErrInfo = "";
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Info = "Printing cancelled";
                ErrInfo = "";
                _isRunning = false;
            }
            else if (e.Error != null)
            {
                Info = e.Error.Message;
            }
        }

        private ICommand print;
        public ICommand Print
        {
            get
            {
                if (print == null)
                {
                    print = new RelayCommand(param => PrintExecute(), param => CanPrintExecute());
                }
                return print;
            }
        }

        private bool CanPrintExecute()
        {
            if (Document == null || Copy == null || int.Parse(Copy) == 0)
            {
                return false;
            }
            else
            {              
                return true;
            }
        }

        private void PrintExecute()
        {
            if (!bgWorker.IsBusy)
            {
                // This method will start the execution asynchronously in the background
                bgWorker.RunWorkerAsync();
                _isRunning = true;
            }
            else
            {
                ErrInfo = "Busy Printing, please wait.";
            }
        }

        private ICommand cancel;
        public ICommand Cancel
        {
            get
            {
                if (cancel == null)
                {
                    cancel = new RelayCommand(param => CancelExecute(), param => CanCancelExecute());
                }
                return cancel;
            }
        }

        private bool CanCancelExecute()
        {
            if (_isRunning == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CancelExecute()
        {
            if (bgWorker.IsBusy)
            {
                // Cancel the asynchronous operation if still in progress
                bgWorker.CancelAsync();
                _isRunning = false;
            }
        }
    }
}
