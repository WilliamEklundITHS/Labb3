using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Labb3
{

    public partial class MainViewModel : Window
    {
        public class BookingModel
        {
            public virtual string DateAndTime { get; init; }
            public virtual string Name { get; init; }
            public virtual int BookingTableNumber { get; init; }
            public BookingModel() { }
            public BookingModel(string selectedDate, string name, int bookingTableNumber)
            {
                DateAndTime = selectedDate; Name = name; BookingTableNumber = bookingTableNumber;
            }
            public override string ToString() { return $"{DateAndTime} {Name} {BookingTableNumber}"; }
        }

        public ObservableCollection<DateTime> AvailableHoursList { get; set; }
        public ObservableCollection<BookingModel> BookingsList { get; set; }
        public BookingModel Booking { get; set; }

        private void SetSelectedHourIndex()
        {

            foreach (var item in BookingsList)
            {
                cb1.SelectedIndex = AvailableHoursList.IndexOf(DateAndStringHelper.StringToDate(item.DateAndTime));
            }
            if (cb1.SelectedIndex == -1) cb1.SelectedIndex = 0;
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
            listBox.Loaded += ListBox_Loaded;
            DataContext = this;
            listBox.ItemsSource = null;
            BookingsList = new()
            {
                new BookingModel("2022-11-02 12:00", "Lars", 3),
                new BookingModel("2022-11-05 17:00", "Jane", 7),
                new BookingModel("2022-11-02 19:00", "Mary", 12),
            };
            listBox.Visibility = Visibility.Collapsed;
            listBox.ItemsSource = BookingsList;
            cb1.ItemsSource = null;
            UpdateDateList(DateTime.Now);
            cb1.ItemsSource = AvailableHoursList;
        }

        IEnumerable<BookingModel> BookingTableNumbers()
        {
            var CheckForTableNumberDuplicates = BookingsList.Where(
                x => x.DateAndTime == Booking.DateAndTime &&
            x.BookingTableNumber == Booking.BookingTableNumber);
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
                                                            on DateAndStringHelper.DateToString(freeHour) equals bookedHour.DateAndTime
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
        private async void RemoveBooking()
        {
            var selectedItems = listBox.SelectedItems.OfType<BookingModel>().ToList();
            if (listBox.SelectedIndex is -1)
            {
                MessageBox.Show($"Markera bokningen du vill ta bort");
            }
            foreach (var booking in selectedItems) BookingsList.Remove(booking);
            await ToJsonFile.UpdateAsync(GetCurrentDirectory(), BookingsList, listBox);
            UpdateAvailableHoursList();
        }
        private async void AddBooking()
        {
            var parsedBookingTableNumber = int.TryParse(BookingTableNumber.Text, out int num);
            if (!parsedBookingTableNumber || num > 40) return;
            DateTime date = DateAndStringHelper.StringToDate(cb1.Text);

            Booking = new BookingModel(DateAndStringHelper.DateToString(date), BookingName.Text, num);

            if (string.IsNullOrWhiteSpace(BookingName.Text)) return;
            if (BookingTableNumbers().ToList().Count > 0) return;
            if (date.Hour == 0) { return; }
            BookingsList.Add(Booking);
            try
            {
                await ToJsonFile.WriteAsync(GetCurrentDirectory(), BookingsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("Något gick fel :/");
            }
            listBox.ItemsSource = null;
            listBox.ItemsSource = BookingsList;
            BookingName.Clear();
            BookingTableNumber.Clear();
            UpdateAvailableHoursList();
        }
        private void DatePicker_Changed(object sender, RoutedEventArgs e)
        {
            var parsedDate = DateAndStringHelper.StringToDate(dp1.Text);
            UpdateDateList(parsedDate);
            UpdateAvailableHoursList();

        }
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            AddBooking();
        }
        private async void ButtonList_Click(object sender, RoutedEventArgs e)
        {
            await ToJsonFile.ReadAsync<BookingModel>(GetCurrentDirectory());
            listBox.Visibility = Visibility.Visible;
        }
        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveBooking();
        }
        public string GetCurrentDirectory()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string file = "TEST.json";
            string path = Path.Combine(dir, file); return path;
        }
        private async void ListBox_Loaded(object? sender, EventArgs e)
        {
            try
            {
                BookingsList = await ToJsonFile.ReadAsync<BookingModel>(GetCurrentDirectory());

                if (!BookingsList.Any())
                {
                    BookingsList = new()
                    {
                        new BookingModel("2022-11-02 12:00", "Lars", 3),
                        new BookingModel("2022-11-05 17:00", "Jane", 7),
                        new BookingModel("2022-11-02 19:00", "Mary", 12),
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            await ToJsonFile.UpdateAsync(GetCurrentDirectory(), BookingsList, listBox);
        }
    }
    public static class DateAndStringHelper
    {
        public static string DateToString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm");
        }
        public static DateTime StringToDate(string str)
        {
            bool SuccessfulParse = DateTime.TryParse(str, out DateTime date);
            if (!SuccessfulParse)
            {
                MessageBox.Show("Välj en tid.");
            }
            return date;
        }

    }
    public static class ToJsonFile
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true
        };
        public static async Task WriteAsync(string fileName, object obj)
        {

            await using (FileStream fileStream = File.Create(fileName))
            {
                await JsonSerializer.SerializeAsync(fileStream, obj, _options);
            };
        }
        public static async Task<ObservableCollection<T>> ReadAsync<T>(string fileName)
        {
            StreamReader streamReader = new StreamReader(fileName, Encoding.UTF8);
            var json = await streamReader.ReadToEndAsync();
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            ObservableCollection<T>? jsonData = await JsonSerializer.DeserializeAsync<ObservableCollection<T>>(stream);
            streamReader.Close();
            stream.Close();

            return jsonData;
        }
        public static async Task UpdateAsync<T>(string fileName, ObservableCollection<T> list, ListBox listBox)
        {
            await WriteAsync(fileName, list);
            listBox.ItemsSource = null;
            listBox.ItemsSource = list;

        }
    }
}


