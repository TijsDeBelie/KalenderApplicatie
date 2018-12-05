using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Calender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<IKalender> KalenderLijst = new ObservableCollection<IKalender>();
        Sqlconnect DB = new Sqlconnect();
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new Status();

            KalenderLijst = DB.SelectKalender();

            Cmonth.SelectedDate = DateTime.Today;

            //KalenderLijst.Add(new Kalender("Werk", DB.SelectAfspraak()));
            //KalenderLijst.Add(new Kalender("Thuis", DB.SelectAfspraak()));
            
            //DB.InsertKalender(KalenderLijst.First());

            CBkalender.ItemsSource = CBkalender1.ItemsSource = CBkalender2.ItemsSource = KalenderLijst;
     
            //TestData();
            //UpdateList(KalenderLijst.First());

        }


        public void AddEvent(string onderwerp, string beschrijving, IKalender kalender)
        {
            List<DateTime> dates = new List<DateTime>();

            foreach (DateTime item in Cmonth.SelectedDates)
            {
                dates.Add(item);
            }
            dates.Sort((x, y) => DateTime.Compare(x, y));

            Afspraak nieuweAfspraak = new Afspraak(dates.First().AddHours(Convert.ToDateTime(dtpStart.Text).Hour).AddMinutes(Convert.ToDateTime(dtpStart.Text).Minute), dates.Last().AddHours(Convert.ToDateTime(dtpEnd.Text).Hour).AddMinutes(Convert.ToDateTime(dtpEnd.Text).Minute), onderwerp, beschrijving);
            
            kalender.AfsprakenLijst.Add(nieuweAfspraak);
            dates.Clear();

            DisplayList.Items.Clear();
            UpdateList((IKalender)CBkalender.SelectedValue);
            DB.InsertAfspraak(nieuweAfspraak);
        }

        private void Cmonth_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            txtDate.Text = Cmonth.SelectedDate.Value.ToString("dd/MM/yyyy");
            IKalender selected = (IKalender)CBkalender1.SelectedValue;
            if (selected == null) { DayDisplayList.Items.Clear(); return; }
            IEnumerable<IAfspraak> results = selected.AfsprakenLijst.Where(x => x.StartTime.ToString("dd/MM/yyyy") == Cmonth.SelectedDate.Value.ToString("dd/MM/yyyy"));

            DayDisplayList.Items.Clear();
            foreach (Afspraak item in results)
            {
                DayDisplayList.Items.Add(item.ToString());
            }

        }

        private void Cmonth_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TabCalender.SelectedIndex = 2;
        }

        private void BtnNieuweAfspraak_Click(object sender, RoutedEventArgs e)
        {
            DisplayList.Items.Add(CBkalender.SelectedValue.ToString());
            //IEnumerable<IKalender> results = KalenderLijst.Where(x => x.Naam == CBkalender.SelectedValue.ToString());

            AddEvent(txtOnderwerp.Text, txtBeschrijving.Text, (IKalender)CBkalender.SelectedValue);
            //AddEvent(txtOnderwerp.Text);
            TabCalender.SelectedIndex = 1;
        }

        private void BtnNieuwKalender_Click(object sender, RoutedEventArgs e)
        {
            Kalender nieuweKalender = new Kalender(0,txtKalenderNaam.Text, new List<Afspraak>());
            DB.InsertKalender(nieuweKalender);
            KalenderLijst.Add(nieuweKalender);
        }

        private void BtnVerwijderKalender_Click(object sender, RoutedEventArgs e)
        {
            IKalender result = (IKalender)CBkalender1.SelectedValue;
            DB.DeleteKalender(result);
            KalenderLijst.Remove(result);
            CBkalender1.SelectedIndex = 0;

            
        }

        private void CBkalender2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList((IKalender)CBkalender2.SelectedValue);
            CBkalender.SelectedIndex = CBkalender2.SelectedIndex;
            CBkalender1.SelectedIndex = CBkalender2.SelectedIndex;
        }

        public void UpdateList(IKalender Selected)
        {
            DayDisplayList.Items.Clear();
            DisplayList.Items.Clear();
            if (Selected == null) return;
            foreach (Afspraak item in Selected.AfsprakenLijst)
            {
                DisplayList.Items.Add(item.ToString());
                DayDisplayList.Items.Add(item.ToString());
            }

        }

        private void TabCalender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList((IKalender)CBkalender2.SelectedValue);
        }

        private void CBkalender1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList((IKalender)CBkalender2.SelectedValue);
            CBkalender.SelectedIndex = CBkalender1.SelectedIndex;
            CBkalender2.SelectedIndex = CBkalender1.SelectedIndex;


            DayDisplayList.Items.Clear();
        }

        private void CBkalender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList((IKalender)CBkalender2.SelectedValue);
            CBkalender1.SelectedIndex = CBkalender.SelectedIndex;
            CBkalender2.SelectedIndex = CBkalender.SelectedIndex;
        }

        private void TestData()
        {
            for (int i = 0; i < 100; i++)
            {
                KalenderLijst.First().VoegAfspraakToe(new Afspraak(DateTime.Now, DateTime.Now, $"test{i}", "dit is een test", new Persoon("Tijs", $"De Belie{i}")));
            }

        }

        
    }

    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
