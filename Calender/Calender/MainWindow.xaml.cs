using Calender.Exceptions;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using System.Timers;

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

        private StreamReader sr;
        private StreamWriter sw;


        public MainWindow()
        {
            InitializeComponent();
            //status is momenteel niet gebruikt en wordt ook niet opgelsagen in de database, dit is iets voor een uitbreiding

            DataContext = new
            {
                status = new Status(),
                herhaling = new Herhaling(),

            };

            KalenderLijst = DB.SelectKalender();
            Cmonth.SelectedDate = DateTime.Today;
            CBkalender.ItemsSource = CBkalender1.ItemsSource = CBkalender2.ItemsSource = CBkalender3.ItemsSource = importcalender.ItemsSource = KalenderLijst;

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            Image.Source = ImageSourceForBitmap(Properties.Resources.download);

            synthesizer.Volume = 100;
            synthesizer.Rate = 1;


            HerhalingDatum.DisplayDateStart = DateTime.Today;
            HerhalingDatum.BlackoutDates.AddDatesInPast();

            calcNextNotif();

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
                bool overlapping = false;
                if (afspraken.Count >= 1)
                {


                    foreach (var afspraak in afspraken)
                    {
                        if (afspraak.Bezet == true || bezet)
                        {
                            if (Convert.ToDateTime(dtpStart.Value).Ticks > afspraak.StartTime.Ticks && Convert.ToDateTime(dtpStart.Value).Ticks < afspraak.EndTime.Ticks)
                            {
                                overlapping = true;

                            }
                        }
                    }

                    if (overlapping)
                    {
                        MessageBoxResult dialogResult = MessageBox.Show("Weet je zeker dat je wilt toevoegen?", "Er is een overlapping!", MessageBoxButton.YesNo);
                        if (dialogResult == MessageBoxResult.Yes)
                        {
                            voegToe(onderwerp, beschrijving, kalender, bezet);
                            //break;
                        }
                    }
                    else
                    {
                        voegToe(onderwerp, beschrijving, kalender, bezet);
                    }

                    /*
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
                                   // break;
                                }
                            }
                        }
                        else
                        {
                            voegToe(onderwerp, beschrijving, kalender, bezet);
                           // break;
                        }
                    }*/
                }
                else
                {
                    voegToe(onderwerp, beschrijving, kalender, bezet);
                }
            }
            catch (StartDateIsBeforeEndDate)
            {
                notify("De startdatum is groter of gelijk aan de eindatum!",4);

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
                notify("De afspraak is niet toegevoegd omdat er een veld leeg was!\n" + ex.Message,4);

            }
            catch (StartDateIsBeforeEndDate)
            {
                notify("De startdatum is groter of gelijk aan de eindatum!",4);

            }
            catch (Exception ex)
            {
               notify("Er is een fout opgetreden bij het toevoegen van de afspraak!\n" + ex.Message,4);
            }


        }
        /// <summary>
        /// Als de geselecteerde datums veranderen wordt dit uitgevoerd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cmonth_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            try
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
            catch (Exception)
            {


            }


        }

        internal static void notify(string v)
        {
            throw new NotImplementedException();
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
                notify("kalender is succesvol toegevoegd!", 1);
            }
            catch (Exception)
            {
                notify("Kon de nieuwe kalender niet toevoegen", 4);

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
               notify(ex.Message,4);

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

            IList<IAfspraak> lijst = DB.SelectAfspraak(Selected);
            lijst = lijst.OrderBy(x => x.StartTime).ToList();
            foreach (Afspraak item in lijst)
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
            DayDisplayList.Items.Clear();
            UpdateList((IKalender)CBkalender2.SelectedValue);
            CBkalender.SelectedIndex = CBkalender1.SelectedIndex;
            CBkalender2.SelectedIndex = CBkalender1.SelectedIndex;



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
                notify("Kon deze afspraak niet verwijderen!",4);
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
                if (afspraak.Bezet)
                {
                    CBstatus2.SelectedIndex = 1;
                }
                else
                {
                    CBstatus2.SelectedIndex = 0;
                }
            }
            catch
            {
               notify("Kon deze afspraak niet vinden!",4);
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
               notify("Selecteer een Afspraak om deze te wijzigen\n Kijk zeker na of alle velden zijn ingevuld",4);

            }
            catch (Exceptions.NameIsEmpty)
            {
                notify("Kon de afspraak niet wijzigen omdat er een veld leeg is", 4);
            }
            catch (StartDateIsBeforeEndDate)
            {
                notify("De startdatum is groter of gelijk aan de eindatum!", 4);
            }
            catch (Exception)
            {
                notify("Kon de afspraak niet wijzigen", 4);
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
                notify("Kon de kalender niet wijzigen omdat er een veld leeg was", 4);
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
                notify("Kon de lijst van kalenders niet updaten", 4);
            }
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            DB.conClose();
        }

        /// <summary>
        /// Methode om alle velden opleeg te maken. 
        /// </summary>
        private void MaakVeldenLeeg()
        {
            DisplayList.UnselectAll();
            DayDisplayList.UnselectAll();
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
        /// <summary>
        /// Kopieert een afspraak
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnKopieer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DB.InsertAfspraak(new Afspraak(0, (DateTime)txtAfspraakStart.Value, (DateTime)txtAfspraakEind.Value, txtAfspraakTitel.Text, txtAfspraakBeschrijving.Text, (CBstatus2.SelectedValue.ToString() == "Vrij" ? true : false)), (IKalender)CBkalender2.SelectedItem);
                UpdateList((IKalender)CBkalender2.SelectedItem);
                notify("Afspraak is gekopieerd!", 1);
            }
            catch (Exception ex)
            {

               notify("Kon de afspraak niet kopiëren!\n" + ex.Message, 4);
            }

        }

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayList.IsEnabled = true;
            List<IAfspraak> results = DB.SelectAfspraak((IKalender)CBkalender2.SelectedItem);
            List<IAfspraak> filtered = results.Where(s => s.Subject.ToLower().Contains(TxtFilter.Text.ToLower()) || s.Beschrijving.ToLower().Contains(TxtFilter.Text.ToLower())).ToList();
            DisplayList.Items.Clear();
            if (filtered.Count == 0)
            {
                DisplayList.Items.Add("Geen items gevonden");
                DisplayList.IsEnabled = false;
            }
            foreach (IAfspraak item in filtered)
            {
                DisplayList.IsEnabled = true;
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
                synthesizer.SpeakAsync(afspraak.Subject + afspraak.Beschrijving);
            }

        }

        private void WriteItemsToFile(List<IAfspraak> items, FileStream file)
        {

            try
            {
                ImpExplist.Items.Clear();
                if (items == null) { return; }
                sw = new StreamWriter(file);
                sw.WriteLine("De waarde True of False duidt de status vrij(false) of bezet(true) aan");
                foreach (object item in items)
                {
                    //sw.WriteLine(item + ",");
                    IAfspraak afspraak = (IAfspraak)item;
                    sw.WriteLine($"{afspraak.StartTime};{afspraak.EndTime};{afspraak.Subject};{afspraak.Beschrijving};{afspraak.Locatie};{afspraak.Bezet}");
                }

                sw.Close();
                string message = "Alle afspraken geëxporteerd!\nEr waren : " + items.Count + " afspraken";
                notify(message, 1);//success
                ImpExplist.Items.Clear();
            }
            catch (Exception ex)
            {
                notify(ex.Message, 4);//error
                
                sw.Close();

            }
        }

        private void ReadItemsFromFile(FileStream file)
        {
            try
            {
                ImpExplist.Items.Clear();
                sr = new StreamReader(file);
                sr.ReadLine();     // eerste rij moet niet worden ingelezen
                string currentLine;
                while ((currentLine = sr.ReadLine()) != null)
                {
                    string[] values = currentLine.Split(';');
                    ImpExplist.Items.Add(new Afspraak(0, Convert.ToDateTime(values[0]), Convert.ToDateTime(values[1]), values[2], values[3], Convert.ToBoolean(values[5])));
                }
                MessageBoxResult dialogResult = MessageBox.Show($"Wil je {ImpExplist.Items.Count} afspraken importeren in kalender {importcalender.SelectedValue.ToString()}?", "Importeren?", MessageBoxButton.YesNo);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    foreach (Afspraak item in ImpExplist.Items)
                    {
                        DB.InsertAfspraak(item, (IKalender)importcalender.SelectedValue);
                    }



                }
                if (dialogResult == MessageBoxResult.No)
                {
                    notify("Er zijn geen afspraken geïmpoorteerd", 3);//warning
                }
                ImpExplist.Items.Clear();
                sr.Close();
                UpdateList((IKalender)importcalender.SelectedValue);
            }
            catch (NullReferenceException ex)
            {
               notify("Er ging iets fout :\n" + ex.Message, 4);//error
                sr.Close();
            }
            catch (Exception ex)
            {
                notify("Er ging iets fout bij het importeren, is het csv bestand goed geformatteerd?\n" + ex.Message, 4);
                sr.Close();
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
                if (saveFileDialog.ShowDialog() == true)
                {
                    WriteItemsToFile(DB.SelectAfspraak((IKalender)importcalender.SelectedValue), new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write));
                }
            }
            catch (Exception ex)
            {
                notify(ex.Message, 4);
            }

        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CSV Files (*.csv)|*.csv";
                if (openFileDialog.ShowDialog() == true)
                {

                    ReadItemsFromFile(new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read));
                }
            }
            catch (Exception)
            {
                notify("Kon het bestand niet openen!", 4);
            }
        }
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        private ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                ImageSource newSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(handle);
                return newSource;
            }
            catch (Exception)
            {
                DeleteObject(handle);
                return null;
            }
        }

        private void BtnLeegKalender_Click(object sender, RoutedEventArgs e)
        {
            DB.DeleteAfspraak((IKalender)CBkalender1.SelectedValue);
            UpdateList((IKalender)CBkalender1.SelectedValue);
        }

        private void CBherhaling_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CBherhaling.SelectedValue.ToString() != "Geen")
            {

                HerhalingDatum.Visibility = LBLEind.Visibility = Visibility.Visible;
            }
            else
            {

                HerhalingDatum.Visibility = LBLEind.Visibility = Visibility.Hidden;
            }

        }

        public static void notify(string message, int method)
        {
            Notifier notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new PrimaryScreenPositionProvider(
                    corner: Corner.BottomRight,
                    offsetX: 10,
                    offsetY: 10);
                /* * */
                
                cfg.DisplayOptions.TopMost = true; // set the option to show notifications over other windows
                cfg.DisplayOptions.Width = 300; // set the notifications width

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(20),
                maximumNotificationCount: MaximumNotificationCount.FromCount(3));
            });

            switch (method)
            {
                case 1:
                    notifier.ShowSuccess(message);
                    break;
                case 2:
                    notifier.ShowInformation(message);
                    break;
                case 3:
                    notifier.ShowWarning(message);
                    break;
                case 4:
                    notifier.ShowError(message);
                    break;
            }


        }

        private void calcNextNotif()
        {

            /* We berekenen wanneer de volgende afspraak is. Notificatie één uur vroeger
             * We gebruiken hiervoor de klasse Timer en triggeren de Elapsed event 
             * */
            if (DB.SelectAfspraak().Count > 0)
            {
                TimeSpan day = new TimeSpan(24, 00, 00);
                TimeSpan now = TimeSpan.Parse(DateTime.Now.ToString("HH:MM"));
                List<IAfspraak> afspraken = DB.SelectAfspraak();
                bool gevonden = false;
                foreach(var afspraak in afspraken)
                {
                    if (afspraak.StartTime >= DateTime.Now && gevonden == false)
                    {
                        DateTime nextAfspraak = afspraken.First().StartTime;
                        TimeSpan activationTime = TimeSpan.Parse(nextAfspraak.AddHours(-1).ToString("HH:mm"));

                        TimeSpan timeLeftUntilFirstRun = ((day - now) + activationTime);

                        Timer execute = new Timer();
                        execute.Interval = timeLeftUntilFirstRun.TotalMilliseconds;
                        execute.Elapsed += notifNextAfspraak;
                        execute.Start();
                        execute.AutoReset = true;
                        gevonden = true;
                    }
                }
                gevonden = false;
            }
        }

        private void notifNextAfspraak(object sender, ElapsedEventArgs e)
        {
            IAfspraak afspraak = DB.SelectAfspraak().First();
            notify(afspraak.Subject + " " + afspraak.Beschrijving, 1);
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
