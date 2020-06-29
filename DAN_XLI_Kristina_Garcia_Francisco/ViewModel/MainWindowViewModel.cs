using DAN_XLI_Kristina_Garcia_Francisco.Commands;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace DAN_XLI_Kristina_Garcia_Francisco.ViewModel
{
    /// <summary>
    /// Main Window view model
    /// </summary>
    class MainWindowViewModel : BaseViewModel
    {
        /// <summary>
        /// Main window
        /// </summary>
        readonly MainWindow main;
        /// <summary>
        /// Background worker
        /// </summary>
        private readonly BackgroundWorker bgWorker = new BackgroundWorker();
        /// <summary>
        /// Check if its currently printing
        /// </summary>
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
        /// <summary>
        /// The text field document property
        /// </summary>
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

        /// <summary>
        /// The text field copy amount property
        /// </summary>
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

        /// <summary>
        /// The label field general info property
        /// </summary>
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

        /// <summary>
        /// The label field error info property
        /// </summary>
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

        /// <summary>
        /// The progress bar property
        /// </summary>
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

        /// <summary>
        /// Updates the progress bar and prints the value
        /// </summary>
        /// <param name="sender">objecy sender</param>
        /// <param name="e">progress changed event</param>
        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CurrentProgress = e.ProgressPercentage;         
            Info = CurrentProgress.ToString() + "%";
        }

        /// <summary>
        /// Method that the background worker executes
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">do work event</param>
        private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = "";

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

                // Appends files that are made in the same time, since they have the same name
                using (StreamWriter streamWriter = new StreamWriter(fileName, append:true))
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
                    // Round to nearest integer value
                    bgWorker.ReportProgress(Convert.ToInt32(Math.Round(100 / double.Parse(copy))) * i);
                }
            }

            ErrInfo = "";
            _isRunning = false;

            // Cancel the asynchronous operation if still in progress
            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();               
            }
        }

        /// <summary>
        /// Print the appropriate message depending how the worker finished.
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">worker completed event</param>
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
                _isRunning = false;
            }
            else if (CurrentProgress == 100)
            {
                Info = "Finished printing";
                _isRunning = false;
                MessageBox.Show("Finished printing "+ Copy + " documents.");
            }
        }

        #region ICommand

        /// <summary>
        /// Print button
        /// </summary>
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

        /// <summary>
        /// Check if the button has the needed requirements to execute
        /// </summary>
        /// <returns>return true if it has the needed requirements to execute</returns>
        private bool CanPrintExecute()
        {
            if (string.IsNullOrWhiteSpace(Document) || string.IsNullOrWhiteSpace(Copy) 
                || int.Parse(Copy) == 0)
            {
                return false;
            }
            else
            {              
                return true;
            }
        }

        /// <summary>
        /// Start the worker once the button is pressed
        /// </summary>
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

        /// <summary>
        /// Cancel button
        /// </summary>
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

        /// <summary>
        /// Check if the button has the needed requirements to execute
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Stop the worker from executing
        /// </summary>
        private void CancelExecute()
        {
            if (bgWorker.IsBusy)
            {
                // Cancel the asynchronous operation if still in progress
                bgWorker.CancelAsync();
                _isRunning = false;
            }
        }
        #endregion
    }
}
