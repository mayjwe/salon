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
using Microsoft.Win32;
using salon.Model;
using salon.Services;

namespace salon.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditService.xaml
    /// </summary>
    public partial class EditService : Page
    {
        Service _service = new Service();

        public EditService(Service service)
        {
            InitializeComponent();

            if (service != null)
            {
                _service = service;
            }
            else
            {
                _service = new Service();
            }

            DataContext = _service;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                tbTitle.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                tbCost.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                tbDuration.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                tbDiscount.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                tbDescription.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void btnEnterImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog getImageDialog = new OpenFileDialog();

            getImageDialog.Filter = "Файлы изображений: (*.png, *.jpg, *.jpeg)| *.png; *.jpg; *.jpeg";
            getImageDialog.InitialDirectory = "C:\\Users\\Администратор\\Desktop\\Учеба\\Иванов МДК.02.02 Инструментальные средства разработки программного обеспечения\\Салон красоты - услуги\\Салон красоты - услуги\\Сессия 1\\services_b_import\\Услуги салона красоты";
            if (getImageDialog.ShowDialog() == true)
            {
                _service.MainImagePath = $"Услуги салона красоты\\{getImageDialog.SafeFileName}";
                img.Source = new BitmapImage(new Uri(getImageDialog.FileName));
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var context = beauty_salonEntities.GetContext();

            if (string.IsNullOrWhiteSpace(tbCost.Text) || !decimal.TryParse(tbCost.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal cost))
            {
                MessageBox.Show("Введите корректную стоимость (например: 1000 или 1000.50)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(tbDuration.Text) || !int.TryParse(tbDuration.Text, out int duration))
            {
                MessageBox.Show("Введите корректную длительность", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            double discount = 0;
            if (!string.IsNullOrWhiteSpace(tbDiscount.Text))
            {
                if (!double.TryParse(tbDiscount.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out discount))
                {
                    MessageBox.Show("Введите корректную скидку (например: 15 или 15.5)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            _service.Title = tbTitle.Text;
            _service.Cost = cost;
            _service.DurationInSeconds = duration;
            _service.Description = tbDescription.Text;
            _service.Discount = discount;

            Validator validator = new Validator();
            var (isValid, errors) = validator.ServiceValidator(_service);

            if (!isValid)
            {
                MessageBox.Show(string.Join("\n", errors), "Ошибки валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_service.ID == 0)
                {
                    context.Service.Add(_service);
                }
                else
                {
                    var serviceInDb = context.Service.FirstOrDefault(s => s.ID == _service.ID);
                    if (serviceInDb != null)
                    {
                        serviceInDb.Title = _service.Title;
                        serviceInDb.Cost = _service.Cost;
                        serviceInDb.DurationInSeconds = _service.DurationInSeconds;
                        serviceInDb.Description = _service.Description;
                        serviceInDb.Discount = _service.Discount;
                        serviceInDb.MainImagePath = _service.MainImagePath;
                    }
                    else
                    {
                        MessageBox.Show("Услуга не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                context.SaveChanges();
                MessageBox.Show("Данные сохранены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}