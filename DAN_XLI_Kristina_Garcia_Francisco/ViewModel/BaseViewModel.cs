using System.ComponentModel;

namespace DAN_XLI_Kristina_Garcia_Francisco.ViewModel
{
    /// <summary>
    /// Base view model that implements the interface INotifyPropertyChanged
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
