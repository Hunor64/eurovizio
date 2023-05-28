using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace eurovizio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class Dal
    {
        public int Ev { get; set; }
        public string Eloado { get; set; }
        public string Cim { get; set; }
        public int Helyezes { get; set; }
        public int Pontszam { get; set; }
        public string Orszag { get; set; }
    }

    public partial class MainWindow : Window
    {
        private List<Dal> dalok;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }
        readonly string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=eurovizio;";
        private readonly MySqlConnection connection;

        private void LoadData()
        {
            dalok = new List<Dal>();

            string selectQuery = "SELECT ev, eloado, cim, helyezes, pontszam FROM dal;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dal dal = new Dal();
                            dal.Ev = reader.GetInt32("ev");
                            dal.Eloado = reader.GetString("eloado");
                            dal.Cim = reader.GetString("cim");
                            dal.Helyezes = reader.GetInt32("helyezes");
                            dal.Pontszam = reader.GetInt32("pontszam");
                            dalok.Add(dal);
                        }
                    }
                }
            }

            dataGrid.ItemsSource = dalok;
            if (dalok.Count > 0)
                dataGrid.SelectedIndex = 0;
        }
        private void Feladat4_Click(object sender, RoutedEventArgs e)
        {
            int magyarVersenyzoSzam = dalok.Count(d => d.Orszag == "Magyarország");
            int legjobbHelyezes = dalok.Where(d => d.Orszag == "Magyarország")
                                      .Select(d => d.Helyezes)
                                      .Min();

            MessageBox.Show($"Magyarországi versenyzők száma: {magyarVersenyzoSzam}\nLegjobb helyezés: {legjobbHelyezes}");
        }
        private void Feladat5_Click(object sender, RoutedEventArgs e)
        {
            double atlagPontszam = dalok.Where(d => d.Orszag == "Németország")
                                        .Average(d => d.Pontszam);

            MessageBox.Show($"Németország átlagos pontszáma: {atlagPontszam.ToString("0.00")}");
        }
        private void Feladat6_Click(object sender, RoutedEventArgs e)
        {
            string luckDalok = string.Join(", ",
                dalok.Where(d => d.Cim.ToLower().Contains("luck") || d.Eloado.ToLower().Contains("luck"))
                     .Select(d => $"{d.Eloado} - {d.Cim}"));

            MessageBox.Show(luckDalok);
        }
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            string filterText = filterTextBox.Text.ToLower();

            var filteredDalok = dalok.Where(d => d.Eloado.ToLower().Contains(filterText) || d.Cim.ToLower().Contains(filterText))
                                     .OrderBy(d => d.Eloado)
                                     .ThenBy(d => d.Cim)
                                     .Select(d => d.Cim);

            resultListBox.ItemsSource = filteredDalok;
        }
        private void VersenyButton_Click(object sender, RoutedEventArgs e)
        {
            Dal selectedDal = dataGrid.SelectedItem as Dal;
            if (selectedDal != null)
            {
                int ev = selectedDal.Ev;

                string selectQuery = "SELECT datum FROM verseny WHERE ev = @ev;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ev", ev);
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            DateTime datum = (DateTime)result;
                            versenyLabel.Content = $"Verseny dátuma: {datum.ToShortDateString()}";
                        }
                    }
                }
            }
        }


    }

}
