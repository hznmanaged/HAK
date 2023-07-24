using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Horizon.AppointmentKeeper
{
    /// <summary>
    /// Interaction logic for TimePicker.xaml
    /// </summary>
    public partial class TimePicker : Window, INotifyPropertyChanged
    {
        private DateTime _selectedTime;
        public DateTime SelectedTime {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                OnPropertyChanged();
            }
    }

        public TimePicker(DateTime initialTime)
        {
            this.DataContext = this;
            this._selectedTime = initialTime;
            InitializeComponent();
            DatePickerControl.SelectedDate = initialTime;
            //TimePickerControl.DateTime = initialTime;
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.SelectedTime = DatePickerControl.SelectedDate.GetValueOrDefault().Date;
            //this.SelectedTime = this.SelectedTime.Add(TimePickerControl.DateTime.TimeOfDay);
            this.Close();
        }
    }
}
