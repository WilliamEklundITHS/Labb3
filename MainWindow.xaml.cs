using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Labb3
{
    public partial class MainViewModel : Window
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

        private void SetSelectedHourIndex()
        {
            foreach (var item in BookingsList)
            {
                cb1.SelectedIndex = AvailableHoursList.IndexOf(StringToDate(item.DateAndTime));
            }
            if (cb1.SelectedIndex == -1)
            {
                cb1.SelectedIndex = 0;
            }
        }

        public string DateToString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm");
        }
        public DateTime StringToDate(string str)
        {
            bool SuccessfulParse = DateTime.TryParse(str, out DateTime date);
            if (!SuccessfulParse)
            {
                MessageBox.Show("Välj en tid.");
            }
            return date;
        }
        private ObservableCollection<DateTime> UpdateDateList(DateTime date)
        {
            AvailableHoursList = new ObservableCollection<DateTime>(Enumerable.Range(12, 10).Select(x => new DateTime(date.Year,
                date.Month,
                date.Day).AddHours(x)));
            return AvailableHoursList;
        }

        public MainViewModel()
        {
            InitializeComponent();
            DataContext = this;
            listBox.ItemsSource = null;
            cb1.ItemsSource = null;
            UpdateDateList(DateTime.Now);
            BookingsList = new ObservableCollection<BookingModel>()
            { new BookingModel("2022-11-02 12:00", "Lars", 3),
             new BookingModel("2022-11-05 17:00", "Jane", 7),
                new BookingModel("2022-11-02 19:00", "Mary", 12),
                };

            cb1.ItemsSource = AvailableHoursList;
            listBox.ItemsSource = BookingsList;
            listBox.Visibility = Visibility.Collapsed;
        }



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

        private void UpdateAvailableHoursList()
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
            SetSelectedHourIndex();
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
            var parsedBookingTableNumber = int.TryParse(BookingTableNumber.Text, out int num);
            if (!parsedBookingTableNumber) return;
            DateTime date = StringToDate(cb1.Text);
            try { bookingModel = new BookingModel(DateToString(date), BookingName.Text, num); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

            if (string.IsNullOrWhiteSpace(BookingName.Text)) return;
            if (BookingTableNumbers().ToList().Count > 0) return;
            if (date.Hour == 0) { return; }
            BookingsList.Add(bookingModel);
            BookingName.Clear();
            BookingTableNumber.Clear();
            UpdateAvailableHoursList();
        }
        public void DatePicker_Changed(object sender, RoutedEventArgs e)
        {
            var parsedDate = StringToDate(dp1.Text);
            UpdateDateList(parsedDate);
            UpdateAvailableHoursList();

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