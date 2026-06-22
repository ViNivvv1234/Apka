using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;

namespace ViniParkingApp
{
    public partial class EditPage : Page
    {
        string connectionString = "Server=LAPTOP-4RM1TSLF\\SQLEXPRESS;Database=ParkingDB;Integrated Security=True;TrustServerCertificate=True;";

        public EditPage()
        {
            InitializeComponent();
            LoadActiveParkings();
            LoadArchiveParkings();
        }

   
        private void LoadActiveParkings()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT p.id_parkowania AS [Nr Biletu], poj.numer_rejestracyjny AS [Rejestracja], k.imie + ' ' + k.nazwisko AS [Klient], m.poziom AS [Poziom], p.data_wjazdu AS [Data Wjazdu] FROM PARKOWANIA p JOIN POJAZDY poj ON p.id_pojazdu = poj.id_pojazdu JOIN KLIENCI k ON poj.id_klienta = k.id_klienta JOIN MIEJSCA_PARKINGOWE m ON p.id_miejsca = m.id_miejsca WHERE p.data_wyjazdu IS NULL";
                SqlDataAdapter adapter = new SqlDataAdapter(sqlQuery, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                GridEditActive.ItemsSource = dt.DefaultView;
            }
        }

      
        private void LoadArchiveParkings()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT p.id_parkowania AS [Nr Biletu], poj.numer_rejestracyjny AS [Rejestracja], k.imie + ' ' + k.nazwisko AS [Klient], m.poziom AS [Poziom], p.data_wjazdu AS [Data Wjazdu], p.data_wyjazdu AS [Data Wyjazdu] FROM PARKOWANIA p JOIN POJAZDY poj ON p.id_pojazdu = poj.id_pojazdu JOIN KLIENCI k ON poj.id_klienta = k.id_klienta JOIN MIEJSCA_PARKINGOWE m ON p.id_miejsca = m.id_miejsca WHERE p.data_wyjazdu IS NOT NULL ORDER BY p.data_wyjazdu DESC";
                SqlDataAdapter adapter = new SqlDataAdapter(sqlQuery, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                GridEditArchive.ItemsSource = dt.DefaultView;
            }
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (GridEditActive.SelectedItem == null)
            {
                MessageBox.Show("Najpierw zaznacz pojazd na liście, który chcesz wymeldować!", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)GridEditActive.SelectedItem;
            int idParkowania = Convert.ToInt32(row["Nr Biletu"]);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "UPDATE PARKOWANIA SET data_wyjazdu = GETDATE() WHERE id_parkowania = @Id";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", idParkowania);
                command.ExecuteNonQuery();
            }

            MessageBox.Show("Pojazd odnotował wyjazd! Miejsce jest ponownie wolne.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadActiveParkings();
            LoadArchiveParkings();
        }

       
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (GridEditArchive.SelectedItem == null)
            {
                MessageBox.Show("Najpierw zaznacz wpis do usunięcia!", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Czy na pewno chcesz usunąć ten wpis z historii?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DataRowView row = (DataRowView)GridEditArchive.SelectedItem;
                int idParkowania = Convert.ToInt32(row["Nr Biletu"]);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "DELETE FROM PARKOWANIA WHERE id_parkowania = @Id";
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@Id", idParkowania);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Wpis został usunięty.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadArchiveParkings();
            }
        }
    }
}