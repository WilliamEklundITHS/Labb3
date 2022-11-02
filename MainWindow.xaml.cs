using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Labb3
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<DateTime> AvailableHoursList { get; set; }
        public ObservableCollection<BookingModel> BookingsList { get; set; }
        public BookingModel bookingModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            listBox.ItemsSource = null;
            cb1.ItemsSource = null;
            AvailableHoursList = new ObservableCollection<DateTime>(Enumerable.Range(8, 12).Select(x => new DateTime(DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day).AddHours(x)));
            cb1.ItemsSource = AvailableHoursList;
            BookingsList = new ObservableCollection<BookingModel>()
            { new BookingModel("2022-11-02 12:00", "Lars", 3),
             new BookingModel("2022-11-05 17:00", "Jane", 7),
                new BookingModel("2022-11-02 19:00", "Mary", 12),
                };
            listBox.ItemsSource = BookingsList;
            listBox.Visibility = Visibility.Collapsed;
        }

        public int SelectedCbItemIndex() => cb1.SelectedIndex = 0;
        public string DateToString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm");
        }
        public void DatePicker_Changed(object sender, RoutedEventArgs e)
        {
            SelectedCbItemIndex();
            var parsedDate = DateTime.Parse(dp1.Text);
            AvailableHoursList = new ObservableCollection<DateTime>(Enumerable.Range(8, 12).Select(x => new DateTime(
                parsedDate.Year,
                parsedDate.Month,
                parsedDate.Day).AddHours(x)));
            UpdateDateList();
        }

        private IEnumerable<BookingModel> CheckDuplicates()
        {
            var CheckForTableNumberDuplicates = BookingsList.Where(
                x => x.DateAndTime == bookingModel.DateAndTime &&
            x.BookingTableNumber == bookingModel.BookingTableNumber);
            if (CheckForTableNumberDuplicates.Any())
            {
                MessageBox.Show("Bordet är redan bokat");
            }
            return CheckForTableNumberDuplicates;
        }

        private void UpdateDateList()
        {
            IEnumerable<DateTime> dateList_in_bookingList = from freeHour in AvailableHoursList
                                                            join bookedHour in BookingsList
                                                            on DateToString(freeHour) equals bookedHour.DateAndTime
                                                            into matches
                                                            where matches.ToList().Count == 2
                                                            select freeHour;
            if (!dateList_in_bookingList.Any())
            {
                cb1.ItemsSource = null;
                cb1.ItemsSource = AvailableHoursList;
            }
            cb1.ItemsSource = null;
            cb1.ItemsSource = AvailableHoursList.Except(dateList_in_bookingList);
        }
        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = listBox.SelectedItems.OfType<BookingModel>().ToList();
            if (listBox.SelectedIndex is -1)
            {
                MessageBox.Show($"Markera bokningen du vill ta bort");
            }
            foreach (var booking in selectedItems) { BookingsList.Remove(booking); }
        }
        private void ButtonList_Click(object sender, RoutedEventArgs e)
        {
            listBox.Visibility = Visibility.Visible;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            var parseBookingTableNumber = int.TryParse(BookingTableNumber.Text, out int num);

            if (!parseBookingTableNumber) { return; }

            var parsedDate = DateTime.Parse(cb1.Text);
            bookingModel = new BookingModel(DateToString(parsedDate), BookingName.Text, num);

            if (string.IsNullOrWhiteSpace(BookingName.Text)) { return; }
            if (CheckDuplicates().ToList().Count > 0) { return; }

            BookingsList.Add(bookingModel);
            BookingName.Clear();
            BookingTableNumber.Clear();
            SelectedCbItemIndex();
            UpdateDateList();
        }
        public class BookingModel
        {
            public virtual string DateAndTime { get; set; }
            public virtual string Name { get; set; }
            public virtual int BookingTableNumber { get; set; }
            public BookingModel(string selectedDate, string name, int bookingTableNumber)
            {
                DateAndTime = selectedDate; Name = name; BookingTableNumber = bookingTableNumber;
            }
            public override string ToString() { return $"{DateAndTime} {Name} {BookingTableNumber}"; }
        }
    }
}