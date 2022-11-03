using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
            public BookingModel() { }
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
        public async void AddBooking()
        {
            var parsedBookingTableNumber = int.TryParse(BookingTableNumber.Text, out int num);
            if (!parsedBookingTableNumber) return;
            DateTime date = StringToDate(cb1.Text);
            try
            {
                bookingModel = new BookingModel(DateToString(date), BookingName.Text, num);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

            if (string.IsNullOrWhiteSpace(BookingName.Text)) return;
            if (BookingTableNumbers().ToList().Count > 0) return;
            if (date.Hour == 0) { return; }
            BookingsList.Add(bookingModel);
            WriteToJsonFile();
            await ReadFromJsonFile();
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
        private async void ButtonList_Click(object sender, RoutedEventArgs e)
        {

            listBox.Visibility = Visibility.Visible;
            var jsonList = await ReadFromJsonFile();
            //listBox.ItemsSource = null;
            //listBox.ItemsSource = jsonList;

        }
        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveBooking();
        }

        public string JsonFilePath = @"C:/Users/Admin/source/repos/Labb3/JsonObjects/Bookings.json";
        public async Task<List<BookingModel>> ReadFromJsonFile()
        {
            StreamReader streamReader = new StreamReader("Bookings.Json", Encoding.UTF8);
            var json = await streamReader.ReadToEndAsync();
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            var jsonData = await JsonSerializer.DeserializeAsync<List<BookingModel>>(stream);
            return jsonData;
        }
        public void WriteToJsonFile()
        {
            var content = BookingsList;
            ToJsonFile.WriteAsync(JsonFilePath, content);
        }
        public static class ToJsonFile
        {
            private static readonly JsonSerializerOptions _options = new()
            {
                WriteIndented = true
            };

            public static void Utf8BytesWrite(string fileName, object obj)
            {
                var utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(obj, _options);
                File.WriteAllBytes(fileName, utf8Bytes);
            }
            public static async Task WriteAsync(string fileName, object obj)
            {
                var fileStream = File.Create(fileName);
                await JsonSerializer.SerializeAsync(fileStream, obj, _options);

            }

            public static async Task SimpleRead(string fileName)
            {
                FileStream openStream = File.OpenRead(fileName);
                var c = await JsonSerializer.DeserializeAsync<object>(openStream);
                byte[] byteArray = Encoding.UTF8.GetBytes(c.ToString());
                MemoryStream stream = new MemoryStream(byteArray);
                await JsonSerializer.DeserializeAsync<List<object>>(stream);
            }
        }
    }


}