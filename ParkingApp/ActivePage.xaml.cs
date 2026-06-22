using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient; // Jeśli masz błąd z tym usingiem, daj znać!

namespace ViniParkingApp
{
    public partial class ActivePage : Page
    {
        string connectionString = "Server=LAPTOP-4RM1TSLF\\SQLEXPRESS;Database=ParkingDB;Integrated Security=True;TrustServerCertificate=True;";

        public ActivePage()
        {
            InitializeComponent();
            LoadActiveParkings(); 
        }

        private void LoadActiveParkings()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                  
                    string sqlQuery = @"
                        SELECT 
                            p.id_parkowania AS [Nr Biletu],
                            poj.numer_rejestracyjny AS [Rejestracja],
                            k.imie + ' ' + k.nazwisko AS [Klient],
                            m.poziom AS [Poziom],
                            p.data_wjazdu AS [Data Wjazdu]
                        FROM PARKOWANIA p
                        JOIN POJAZDY poj ON p.id_pojazdu = poj.id_pojazdu
                        JOIN KLIENCI k ON poj.id_klienta = k.id_klienta
                        JOIN MIEJSCA_PARKINGOWE m ON p.id_miejsca = m.id_miejsca
                        WHERE p.data_wyjazdu IS NULL";

                    SqlCommand command = new SqlCommand(sqlQuery, connection);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    GridActiveParkings.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd połączenia z bazą: " + ex.Message);
            }
        }
    }
}