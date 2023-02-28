using System;
using System.Collections.Generic;
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
using Template_4335.Windows;

namespace Template_4335
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Zagidullin_4335_Click(object sender, RoutedEventArgs e)
        {
            Zagidullin_4335 zg = new Zagidullin_4335();
            zg.Show();
        }

        private void Gazizullin_4335_Click(object sender, RoutedEventArgs e)
        {
            Gazizullin_4335 gz = new Gazizullin_4335();
            gz.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
         
        }
    }
}
