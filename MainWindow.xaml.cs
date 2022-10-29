using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Labb3.MainWindow.BookingModel;
namespace Labb3
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<DateTime> AvailableBookingsList { get; set; }
        public ObservableCollection<BookingModel> BookingsList { get; set; }
        public BookingModel bookingModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; listBox.DataContext = this;
            listBox.ItemsSource = null; cb1.ItemsSource = null;
            var jim = new DateTime(2022, 11, 10, 8, 0, 0);
            AvailableBookingsList = new ObservableCollection<DateTime>(Enumerable.Range(8, 12).Select(x => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddHours(x)));
            cb1.ItemsSource = AvailableBookingsList; BookingsList = new ObservableCollection<BookingModel>()
            { new BookingModel("2022-11-02 12:00", "Greg", 3),
                new BookingModel("2022-11-02 19:00", "Mary", 12),
                new BookingModel("2022-11-05 17:00", "Jane", 7) }; listBox.ItemsSource = BookingsList;
        }
        public int SetSelectedCbItemIndex() => cb1.SelectedIndex = 0;
        public void DatePicker_Changed(object sender, RoutedEventArgs e)
        {
            var parsedDate = DateTime.Parse(dp1.Text);
            AvailableBookingsList = new ObservableCollection<DateTime>(Enumerable.Range(8, 12).Select(x => new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day).AddHours(x)));
            UpdateDateList();
        }
        private void UpdateDateList()
        {
            var UpdateAvailableDatesList = AvailableBookingsList.AsEnumerable();

            foreach (var item in BookingsList)
            {
                UpdateAvailableDatesList = UpdateAvailableDatesList.Where(x => x.ToString("yyyy-MM-dd HH:mm") != item.DateAndTime);

                if (UpdateAvailableDatesList.Any())
                {
                    cb1.ItemsSource = null; cb1.ItemsSource = AvailableBookingsList;

                    SetSelectedCbItemIndex();

                }
                cb1.ItemsSource = null;

                cb1.ItemsSource = UpdateAvailableDatesList.ToList();

                SetSelectedCbItemIndex();

            }
        }
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {

            var parseBookingTableNumber = int.TryParse(BookingTableNumber.Text, out int num);

            if (!parseBookingTableNumber) { return; }

            var parsedDate = DateTime.Parse(cb1.Text);

            string FormattedDate = parsedDate.ToString("yyyy-MM-dd HH:mm");

            bookingModel = new BookingModel(FormattedDate, BookingName.Text, num);

            if (string.IsNullOrWhiteSpace(BookingName.Text)) { return; }

            BookingsList.Add(bookingModel);

            BookingName.Clear();

            BookingTableNumber.Clear();

            UpdateDateList();

            SetSelectedCbItemIndex();

        }
        public class BookingModel
        {

            public virtual string DateAndTime { get; set; }

            public virtual string Name { get; set; }

            public virtual int BookingTableNumber { get; set; }
            public BookingModel()
            {

                DateAndTime = string.Empty; Name = string.Empty; BookingTableNumber = 0;
            }
            public BookingModel(string selectedDate, string name, int bookingTableNumber)
            {

                DateAndTime = selectedDate; Name = name; BookingTableNumber = bookingTableNumber;
            }

            public override string ToString() { return $"{DateAndTime} {Name} {BookingTableNumber}"; }

        }
    }
}