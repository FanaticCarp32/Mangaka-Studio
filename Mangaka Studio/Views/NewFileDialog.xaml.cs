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
using System.Windows.Shapes;

namespace Mangaka_Studio.Views
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class NewFileDialog : Window
    {
        public int width;
        public int height;
        public NewFileDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ResolutionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResolutionComboBox.SelectedItem is ComboBoxItem item)
            {
                var resolution = item.Tag.ToString().Split('x');
                if (resolution.Length >= 2 && int.TryParse(resolution[0], out int width) && int.TryParse(resolution[1], out int height))
                {
                    WidthTextBox.Text = resolution[0];
                    this.width = width;
                    HeightTextBox.Text = resolution[1];
                    this.height = height;
                }
            }
        }

        private void CustomResolutionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ResolutionStackPanel.Visibility = Visibility.Visible;
            ResolutionComboBox.IsEnabled = false;
        }

        private void CustomResolutionCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ResolutionStackPanel.Visibility = Visibility.Collapsed;
            ResolutionComboBox.IsEnabled = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (width == 0 || height == 0)
            {
                MessageBox.Show(
                    "Неверный формат изображения",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void WidthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(WidthTextBox.Text, out int width);
            this.width = width;
        }

        private void HeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(HeightTextBox.Text, out int height);
            this.height = height;
        }
    }
}
