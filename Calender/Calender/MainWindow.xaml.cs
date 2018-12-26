using Calender.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
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

        SpeechSynthesizer synthesizer = new SpeechSynthesizer();


        public MainWindow()
        {
            InitializeComponent();
            //status is momenteel niet gebruikt en wordt ook niet opgelsagen in de database, dit is iets voor een uitbreiding

            DataContext = new
            {
                status = new Status(),
                herhaling = new Herhaling()
            };

            KalenderLijst = DB.SelectKalender();
            Cmonth.SelectedDate = DateTime.Today;
            CBkalender.ItemsSource = CBkalender1.ItemsSource = CBkalender2.ItemsSource = CBkalender3.ItemsSource = KalenderLijst;

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            synthesizer.Volume = 100;
            synthesizer.Rate = 1;

        }

        /// <summary>
        ///  Methode om een afspraak toe te voegen
        /// </summary>
        /// <param name="onderwerp">De titel van de afspraak</param>
        /// <param name="beschrijving">De beschrijving van de afspraak</param>
        /// <param name="kalender">De kalender waar de afspraak bij hoort</param>
        private void AddEvent(string onderwerp, string beschrijving, IKalender kalender, bool bezet)
        {
            try
            {

                if (Convert.ToDateTime(dtpStart.Value).CompareTo(Convert.ToDateTime(dtpEnd.Value)) != -1) { throw new StartDateIsBeforeEndDate(); }
                List<IAfspraak> afspraken = DB.SelectAfspraak();
                if (afspraken.Count >= 1)
                {


                    //Bij een lijst van afspraken met meerdere items gaat De afspraak meerdere keren worden ingevoerd, dit is denk ik niet de bedoeling 
                    foreach (var afspraak in afspraken)
                    {
                        if (afspraak.Bezet == true)
                        {
                            if (Convert.ToDateTime(dtpStart.Value).Ticks > afspraak.StartTime.Ticks && Convert.ToDateTime(dtpStart.Value).Ticks < afspraak.EndTime.Ticks)
                            {
                                MessageBoxResult dialogResult = MessageBox.Show("Weet je zeker dat je wilt toevoegen?", "Er is een overlapping!", MessageBoxButton.YesNo);
                                if (dialogResult == MessageBoxResult.Yes)
                                {
                                    voegToe(onderwerp, beschrijving, kalender, bezet);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            voegToe(onderwerp, beschrijving, kalender, bezet);
                            break;
                        }
                    }
                }
                else
                {
                    voegToe(onderwerp, beschrijving, kalender, bezet);
                }
            }
            catch (StartDateIsBeforeEndDate)
            {
                MessageBox.Show("De startdatum is groter of gelijk aan de eindatum!");

            }
        }
        private void voegToe(string onderwerp, string beschrijving, IKalender kalender, bool bezet)
        {
            try
            {
                Afspraak nieuweAfspraak = new Afspraak(0, Convert.ToDateTime(dtpStart.Value), Convert.ToDateTime(dtpEnd.Value), onderwerp, beschrijving, bezet);

                kalender.AfsprakenLijst.Add(nieuweAfspraak);


                DisplayList.Items.Clear();

                DB.InsertAfspraak(nieuweAfspraak, kalender);

                if (CBherhaling.SelectedValue.ToString() == "Dagelijks")
                {
                    DateTime herhalingEindDatum = (DateTime)HerhalingDatum.SelectedDate;
                    double totaalDagen = (herhalingEindDatum.Date - ((DateTime)dtpStart.Value).Date).TotalDays;
                    for (int i = 1; i <= totaalDagen; i++)
                    {
                        Afspraak nieuweHerhalingsAfspraak = new Afspraak(0, Convert.ToDateTime(dtpStart.Value).AddDays(i), Convert.ToDateTime(dtpEnd.Value).AddDays(i), onderwerp, beschrijving, bezet);
                        DB.InsertAfspraak(nieuweHerhalingsAfspraak, kalender);
                    }
                    DB.BevestigAfspraak((int)totaalDagen);

                }
                else if (CBherhaling.SelectedValue.ToString() == "Wekelijks")
                {
                    DateTime herhalingEindDatum = (DateTime)HerhalingDatum.SelectedDate;
                    double totaalDagen = (herhalingEindDatum.Date - ((DateTime)dtpStart.Value).Date).TotalDays;
                    double totaalWeken = totaalDagen / 7;
                    for (int i = 1; i <= totaalWeken; i++)
                    {
                        Afspraak nieuweHerhalingsAfspraak = new Afspraak(0, Convert.ToDateTime(dtpStart.Value).AddWeeks(i), Convert.ToDateTime(dtpEnd.Value).AddWeeks(i), onderwerp, beschrijving, bezet);
                        DB.InsertAfspraak(nieuweHerhalingsAfspraak, kalender);
                    }
                    DB.BevestigAfspraak((int)totaalWeken);
                }




                UpdateList((IKalender)CBkalender.SelectedValue);



                MaakVeldenLeeg();
                TabCalender.SelectedIndex = 1;
            }
            catch (Exceptions.NameIsEmpty ex)
            {
                MessageBox.Show("De afspraak is niet toegevoegd omdat er een veld leeg was!\n" + ex.Message);

            }
            catch (StartDateIsBeforeEndDate)
            {
                MessageBox.Show("De startdatum is groter of gelijk aan de eindatum!");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Er is een fout opgetreden bij het toevoegen van de afspraak!\n" + ex.Message);
            }


        }
        /// <summary>
        /// Als de geselecteerde datums veranderen wordt dit uitgevoerd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cmonth_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            txtDate.Text = $"{Cmonth.SelectedDates.First().ToString("dd/MM/yyyy")} - {Cmonth.SelectedDates.Last().ToString("dd/MM/yyyy")}";
            dtpStart.Value = Cmonth.SelectedDates.First();
            dtpEnd.Value = Cmonth.SelectedDates.Last();
            IKalender selected = (IKalender)CBkalender1.SelectedValue;
            if (selected == null) { DayDisplayList.Items.Clear(); return; }
            IEnumerable<IAfspraak> results = selected.AfsprakenLijst.Where(x => x.StartTime.Date >= Cmonth.SelectedDates.First().Date && x.EndTime.Date <= Cmonth.SelectedDates.Last().Date.AddDays(1));
            DayDisplayList.Items.Clear();
            foreach (Afspraak item in results)
            {
                DayDisplayList.Items.Add(item);
            }

        }
        /// <summary>
        /// Snelle manier om een afspraak in te voeren op de geselecteerde datum. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cmonth_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TabCalender.SelectedIndex = 2;
        }
        /// <summary>
        /// Het click event om een nieuwe afspraak te maken
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNieuweAfspraak_Click(object sender, RoutedEventArgs e)
        {
            bool bezet = false;
            if (CBstatus.SelectedValue.Equals("Bezet"))
                bezet = true;

            AddEvent(txtOnderwerp.Text, txtBeschrijving.Text, (IKalender)CBkalender.SelectedValue, bezet);

        }
        /// <summary>
        /// Het click event voor een nieuwe kalender toe te voegen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNieuwKalender_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Kalender nieuweKalender = new Kalender(0, txtKalenderNaam.Text, txtKalenderBeschrijving.Text, new List<IAfspraak>());
                DB.InsertKalender(nieuweKalender);
                UpdateKalenderLijst();
                MaakVeldenLeeg();
            }
            catch (Exception)
            {
                MessageBox.Show("Kon de nieuwe kalender niet toevoegen");

            }

        }
        /// <summary>
        /// Het click event om een kalender te verwijderen, update de lijst van kalenders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnVerwijderKalender_Click(object sender, RoutedEventArgs e)
        {
            IKalender result = (IKalender)CBkalender1.SelectedValue;
            DB.DeleteKalender(result);
            UpdateKalenderLijst();
            CBkalender1.SelectedIndex = 0;


        }
        /// <summary>
        /// Zorgt voor een synchronisatie van de comboboxen, als je op de eerste tab een kalender slecteerd ga je die ook zien in de andere tabs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CBkalender2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {



                IKalender kalender = (IKalender)CBkalender2.SelectedValue;
                UpdateList(kalender);
                CBkalender.SelectedIndex = CBkalender2.SelectedIndex;
                CBkalender1.SelectedIndex = CBkalender2.SelectedIndex;

                txtKalenderID.Text = kalender.Id.ToString();
                txtKalenderNaam2.Text = kalender.Naam;
                txtKalenderBeschrijving2.Text = kalender.Beschrijving;
            }
            catch (Exception ex)
            {


            }
        }
        /// <summary>
        /// Gaat de lijst van afspraken opnieuw ophalen van de database
        /// </summary>
        /// <param name="Selected">De geselecteerde kalender om afspraken van weer te geven</param>
        private void UpdateList(IKalender Selected)
        {
            DayDisplayList.Items.Clear();
            DisplayList.Items.Clear();
            if (Selected == null) return;


            foreach (Afspraak item in DB.SelectAfspraak(Selected))
            {
                DisplayList.Items.Add(item);
                DayDisplayList.Items.Add(item);
            }

        }
        /// <summary>
        /// Als er veranderd wordt van tab wordt ook de lijst van afspraken vernieuwd om altijd de laatste updates te zien
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabCalender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList((IKalender)CBkalender2.SelectedValue);
            MaakVeldenLeeg();
        }
        /// <summary>
        /// Zorgt voor een synchronisatie van de comboboxen, als je op de eerste tab een kalender slecteerd ga je die ook zien in de andere tabs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CBkalender1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList((IKalender)CBkalender2.SelectedValue);
            CBkalender.SelectedIndex = CBkalender1.SelectedIndex;
            CBkalender2.SelectedIndex = CBkalender1.SelectedIndex;


            DayDisplayList.Items.Clear();
        }
        /// <summary>
        /// Zorgt voor een synchronisatie van de comboboxen, als je op de eerste tab een kalender slecteerd ga je die ook zien in de andere tabs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CBkalender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList((IKalender)CBkalender2.SelectedValue);
            CBkalender1.SelectedIndex = CBkalender.SelectedIndex;
            CBkalender2.SelectedIndex = CBkalender.SelectedIndex;
        }

        /// <summary>
        /// Verwijdert de geselecteerde afspraak
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnVerwijder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Afspraak result = (Afspraak)DisplayList.SelectedItem;
                DB.DeleteAfspraak(result);
                UpdateList((IKalender)CBkalender2.SelectedItem);
            }
            catch
            {
                MessageBox.Show("Kon deze afspraak niet verwijderen!");
            }

        }
        /// <summary>
        /// Als er op een afspraak wordt geklikt toont dit meer informatie van die afspraak
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Afspraak afspraak = (Afspraak)DisplayList.SelectedItem;
                if (afspraak == null) return;
                txtAfspraakID.Text = afspraak.Id.ToString();
                txtAfspraakTitel.Text = afspraak.Subject;
                txtAfspraakBeschrijving.Text = afspraak.Beschrijving;
                txtAfspraakStart.Value = afspraak.StartTime;
                txtAfspraakEind.Value = afspraak.EndTime;
            }
            catch
            {
                MessageBox.Show("Kon deze afspraak niet vinden!");
            }
        }
        /// <summary>
        /// Wijzigt de afspraak
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnWijzig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int value;
                int.TryParse(txtAfspraakID.Text, out value);

                if (String.IsNullOrWhiteSpace(txtAfspraakTitel.Text) || String.IsNullOrWhiteSpace(txtAfspraakBeschrijving.Text)) { throw new NameIsEmpty(); }
                if (Convert.ToDateTime(txtAfspraakStart.Value).CompareTo(Convert.ToDateTime(txtAfspraakEind.Value)) != -1) { throw new StartDateIsBeforeEndDate(); }
                DB.UpdateAfspraak(value, (DateTime)txtAfspraakStart.Value, (DateTime)txtAfspraakEind.Value, txtAfspraakTitel.Text, txtAfspraakBeschrijving.Text, (IKalender)CBkalender3.SelectedItem);
                UpdateList((IKalender)CBkalender2.SelectedItem);
                MaakVeldenLeeg();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Selecteer een Afspraak om deze te wijzigen\n Kijk zeker na of alle velden zijn ingevuld");

            }
            catch (Exceptions.NameIsEmpty)
            {
                MessageBox.Show("Kon de afspraak niet wijzigen omdat er een veld leeg is");
            }
            catch (StartDateIsBeforeEndDate)
            {
                MessageBox.Show("De startdatum is groter of gelijk aan de eindatum!");
            }
            catch (Exception)
            {
                MessageBox.Show("Kon de afspraak niet wijzigen");
            }
        }

        /// <summary>
        /// Wijzigt de kalender
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnWijzigKalender_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int value;
                int.TryParse(txtKalenderID.Text, out value);
                if (string.IsNullOrWhiteSpace(txtKalenderBeschrijving2.Text) || string.IsNullOrWhiteSpace(txtKalenderNaam2.Text)) { throw new NameIsEmpty(); }
                DB.Updatekalender(value, txtKalenderNaam2.Text, txtKalenderBeschrijving2.Text, (IKalender)CBkalender2.SelectedValue);
                UpdateKalenderLijst();
                UpdateList((IKalender)CBkalender2.SelectedValue);
            }
            catch (Exceptions.NameIsEmpty)
            {
                MessageBox.Show("Kon de kalender niet wijzigen omdat er een veld leeg was");
            }
        }
        /// <summary>
        /// Gaat de kalenderlijst opnieuw ophalen vanuit de database
        /// </summary>
        private void UpdateKalenderLijst()
        {
            try
            {
                KalenderLijst = DB.SelectKalender();
                CBkalender.ItemsSource = CBkalender1.ItemsSource = CBkalender2.ItemsSource = CBkalender3.ItemsSource = KalenderLijst;
                CBkalender.SelectedIndex = CBkalender1.SelectedIndex = CBkalender2.SelectedIndex = CBkalender3.SelectedIndex = 0;
            }
            catch
            {
                MessageBox.Show("Kon de lijst van kalenders niet updaten");
            }
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            DB.conClose();
        }


        private void MaakVeldenLeeg()
        {
            DisplayList.UnselectAll();
            txtKalenderBeschrijving2.Text = string.Empty;
            txtKalenderNaam2.Text = string.Empty;
            txtAfspraakStart.Value = null;
            txtAfspraakEind.Value = null;
            txtAfspraakBeschrijving.Text = string.Empty;
            txtAfspraakID.Text = string.Empty;
            txtAfspraakTitel.Text = string.Empty;
            txtBeschrijving.Text = string.Empty;
            txtDate.Text = string.Empty;
            txtKalenderBeschrijving.Text = string.Empty;
            txtKalenderID.Text = string.Empty;
            txtKalenderNaam.Text = string.Empty;
            txtLocatie.Text = string.Empty;
            txtOnderwerp.Text = string.Empty;
        }

        private void BtnKopieer_Click(object sender, RoutedEventArgs e)
        {

            DB.InsertAfspraak(new Afspraak(0, (DateTime)txtAfspraakStart.Value, (DateTime)txtAfspraakEind.Value, txtAfspraakTitel.Text, txtAfspraakBeschrijving.Text, (CBstatus2.SelectedValue.ToString() == "Vrij" ? true : false)), (IKalender)CBkalender2.SelectedItem);
            UpdateList((IKalender)CBkalender2.SelectedItem);
        }

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {

            List<IAfspraak> results = DB.SelectAfspraak((IKalender)CBkalender2.SelectedItem);
            List<IAfspraak> filtered = results.Where(s => s.Subject.ToLower().Contains(TxtFilter.Text.ToLower()) || s.Beschrijving.ToLower().Contains(TxtFilter.Text.ToLower())).ToList();
            DisplayList.Items.Clear();
            if (filtered.Count == 0)
            {
                DisplayList.Items.Add("Geen items gevonden");

            }
            foreach (IAfspraak item in filtered)
            {
                DisplayList.Items.Add(item);
            }

        }

        private void DayDisplayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((bool)ChkSpraak.IsChecked)
            {
                synthesizer.SelectVoiceByHints(VoiceGender.Neutral, VoiceAge.NotSet, 0, CultureInfo.GetCultureInfo("nl-BE"));
                synthesizer.SpeakAsyncCancelAll();
                IAfspraak afspraak = (IAfspraak)DayDisplayList.SelectedValue;
                synthesizer.SpeakAsync(afspraak.Subject + afspraak.Beschrijving + afspraak.Bezet);
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

    public static class DateTimeExtensions
    {
        public static DateTime AddWeeks(this DateTime dateTime, int numberOfWeeks)
        {
            return dateTime.AddDays(numberOfWeeks * 7);
        }
    }
}
