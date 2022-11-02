using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Labb3
{
    public partial class MainWindow : Window
    {
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
        public ObservableCollection<DateTime> AvailableHoursList { get; set; }
        public ObservableCollection<BookingModel> BookingsList { get; set; }
        public BookingModel bookingModel { get; set; }

        private int SelectedCbItemIndex() => cb1.SelectedIndex = 0;
        public string DateToString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm");
        }
        private ObservableCollection<DateTime> UpdateAvailableHoursList(DateTime date)
        {
            AvailableHoursList = new ObservableCollection<DateTime>(Enumerable.Range(8, 12).Select(x => new DateTime(date.Year,
                date.Month,
                date.Day).AddHours(x)));
            return AvailableHoursList;
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            listBox.ItemsSource = null;
            cb1.ItemsSource = null;
            UpdateAvailableHoursList(DateTime.Now);
            BookingsList = new ObservableCollection<BookingModel>()
            { new BookingModel("2022-11-02 12:00", "Lars", 3),
             new BookingModel("2022-11-05 17:00", "Jane", 7),
                new BookingModel("2022-11-02 19:00", "Mary", 12),
                };

            cb1.ItemsSource = AvailableHoursList;
            listBox.ItemsSource = BookingsList;
            listBox.Visibility = Visibility.Collapsed;
        }
        private void ValidateBookingTableNumber()
        {
            IEnumerable<BookingModel> BookingTableNumbers()
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
            if (BookingTableNumbers().ToList().Count > 0) return;
        }

        private void LimitAvailableHoursList()
        {
            IEnumerable<DateTime> dateList_in_bookingList = from freeHour in AvailableHoursList
                                                            join bookedHour in BookingsList
                                                            on DateToString(freeHour) equals bookedHour.DateAndTime
                                                            into matches
                                                            where matches.ToList().Count == 5
                                                            select freeHour;
            if (!dateList_in_bookingList.Any())
            {
                cb1.ItemsSource = null;
                cb1.ItemsSource = AvailableHoursList;
            }
            cb1.ItemsSource = null;
            cb1.ItemsSource = AvailableHoursList.Except(dateList_in_bookingList);
        }
        private void RemoveBooking()
        {
            var selectedItems = listBox.SelectedItems.OfType<BookingModel>().ToList();
            if (listBox.SelectedIndex is -1)
            {
                MessageBox.Show($"Markera bokningen du vill ta bort");
            }
            foreach (var booking in selectedItems) { BookingsList.Remove(booking); }
        }
        public void AddBooking()
        {
            var parseBookingTableNumber = int.TryParse(BookingTableNumber.Text, out int num);

            if (!parseBookingTableNumber) return;

            var parsedDate = DateTime.Parse(cb1.Text);
            bookingModel = new BookingModel(DateToString(parsedDate), BookingName.Text, num);

            if (string.IsNullOrWhiteSpace(BookingName.Text)) return;

            ValidateBookingTableNumber();
            BookingsList.Add(bookingModel);
            BookingName.Clear();
            BookingTableNumber.Clear();
            SelectedCbItemIndex();
            LimitAvailableHoursList();
        }


        public void DatePicker_Changed(object sender, RoutedEventArgs e)
        {
            SelectedCbItemIndex();
            var parsedDate = DateTime.Parse(dp1.Text);
            UpdateAvailableHoursList(parsedDate);
            LimitAvailableHoursList();
        }
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            AddBooking();
        }
        private void ButtonList_Click(object sender, RoutedEventArgs e)
        {
            listBox.Visibility = Visibility.Visible;
        }
        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveBooking();
        }


    }
}