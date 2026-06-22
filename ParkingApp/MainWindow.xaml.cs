using System.Windows;

namespace ViniParkingApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

   
        private void BtnActive_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new ActivePage();
        }

        private void BtnArchive_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new ArchivePage();
        }

      
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new AddPage();
        }

       
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new EditPage();
        }
    }
}