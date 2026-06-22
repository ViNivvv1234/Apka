using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;

namespace ViniParkingApp
{
    public partial class AddPage : Page
    {
        string connectionString = "Server=LAPTOP-4RM1TSLF\\SQLEXPRESS;Database=ParkingDB;Integrated Security=True;TrustServerCertificate=True;";

        public AddPage()
        {
            InitializeComponent();
            LoadAvailableSpots(); 
        }

  
        private void LoadAvailableSpots()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT id_miejsca, 'Miejsce nr ' + CAST(id_miejsca AS VARCHAR) + ' (Poziom ' + CAST(poziom AS VARCHAR) + ')' AS Opis FROM MIEJSCA_PARKINGOWE WHERE status = 'wolne'";

                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    CmbMiejsca.ItemsSource = dt.DefaultView;
                    CmbMiejsca.DisplayMemberPath = "Opis"; 
                    CmbMiejsca.SelectedValuePath = "id_miejsca";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd ładowania miejsc: " + ex.Message);
            }
        }

     
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtImie.Text) || string.IsNullOrWhiteSpace(TxtNazwisko.Text) ||
                string.IsNullOrWhiteSpace(TxtRejestracja.Text) || CmbMiejsca.SelectedValue == null)
            {
                MessageBox.Show("Wypełnij wszystkie pola i wybierz miejsce!", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                   
                    string sqlQuery = @"
                        DECLARE @IdKlienta INT;
                        DECLARE @IdPojazdu INT;

                        -- KROK 1: Dodajemy klienta i pobieramy jego wygenerowane ID
                        INSERT INTO KLIENCI (imie, nazwisko) VALUES (@Imie, @Nazwisko);
                        SET @IdKlienta = SCOPE_IDENTITY();

                        -- KROK 2: Sprawdzamy czy auto jest już w bazie, jak nie, to je dodajemy
                        IF NOT EXISTS (SELECT 1 FROM POJAZDY WHERE numer_rejestracyjny = @Rejestracja)
                        BEGIN
                            INSERT INTO POJAZDY (id_klienta, numer_rejestracyjny) VALUES (@IdKlienta, @Rejestracja);
                            SET @IdPojazdu = SCOPE_IDENTITY();
                        END
                        ELSE
                        BEGIN
                            SELECT @IdPojazdu = id_pojazdu FROM POJAZDY WHERE numer_rejestracyjny = @Rejestracja;
                        END

                        -- KROK 3: Rozpoczynamy parkowanie (data wjazdu ustawia się automatycznie na teraz)
                        INSERT INTO PARKOWANIA (id_pojazdu, id_miejsca, data_wjazdu) 
                        VALUES (@IdPojazdu, @IdMiejsca, GETDATE());
                    ";

                    SqlCommand command = new SqlCommand(sqlQuery, connection);

                    command.Parameters.AddWithValue("@Imie", TxtImie.Text);
                    command.Parameters.AddWithValue("@Nazwisko", TxtNazwisko.Text);
                    command.Parameters.AddWithValue("@Rejestracja", TxtRejestracja.Text);
                    command.Parameters.AddWithValue("@IdMiejsca", CmbMiejsca.SelectedValue);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Pojazd został zaparkowany pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                    TxtImie.Clear();
                    TxtNazwisko.Clear();
                    TxtRejestracja.Clear();
                    LoadAvailableSpots();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd zapisu: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}