using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using salon.Model;
using System.Data.Entity; 

namespace salon.Pages
{
    public partial class ListServices : Page
    {
        private bool adminMode = false;

        public ListServices()
        {
            InitializeComponent();

            cbSorting.ItemsSource = new string[]
            {
                "Без сортировки",
                "Стоимость по возрастанию",
                "Стоимость по убыванию"
            };

            cbFilter.ItemsSource = new string[]
            {
                "Все диапазоны",
                "0% - 5%",
                "5% - 15%",
                "15% - 30%",
                "30% - 70%",
                "70% - 100%"
            };

            cbSorting.SelectedIndex = 0;
            cbFilter.SelectedIndex = 0;

            LoadServices();
        }

        private void LoadServices()
        {
            var services = beauty_salonEntities.GetContext().Service.ToList();
            LViewServices.ItemsSource = services;
            txtAllAmount.Text = services.Count.ToString();
            UpdateData();
        }

        private void UpdateData()
        {
            var result = beauty_salonEntities.GetContext().Service.ToList();

            switch (cbSorting.SelectedIndex)
            {
                case 1:
                    result = result.OrderBy(s => s.Cost).ToList();
                    break;
                case 2:
                    result = result.OrderByDescending(s => s.Cost).ToList();
                    break;
            }

            switch (cbFilter.SelectedIndex)
            {
                case 1:
                    result = result.Where(s => s.Discount >= 0 && s.Discount < 0.05).ToList();
                    break;
                case 2:
                    result = result.Where(s => s.Discount >= 0.05 && s.Discount < 0.15).ToList();
                    break;
                case 3:
                    result = result.Where(s => s.Discount >= 0.15 && s.Discount < 0.30).ToList();
                    break;
                case 4:
                    result = result.Where(s => s.Discount >= 0.30 && s.Discount < 0.70).ToList();
                    break;
                case 5:
                    result = result.Where(s => s.Discount >= 0.70 && s.Discount <= 1.00).ToList();
                    break;
            }

            if (!string.IsNullOrWhiteSpace(tbSearch.Text))
            {
                result = result.Where(p => p.Title.ToLower().Contains(tbSearch.Text.ToLower())).ToList();
            }

            txtResultAmount.Text = result.Count.ToString();
            LViewServices.ItemsSource = result;
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateData();
        }

        private void cbSorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void tbAdminCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbAdminCode.Text == "0000" && !adminMode)
            {
                adminMode = true;
                tbAdminCode.Clear();
                MessageBox.Show("Режим администратора активирован", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                btnToRequests.Visibility = Visibility.Visible;
                btnAddService.Visibility = Visibility.Visible;
            }
        }

        private void btnToRequests_Click(object sender, RoutedEventArgs e) { }

        private void btnAddService_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditService(null));
        }

        private void LViewServices_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (adminMode && LViewServices.SelectedItem is Service selectedService)
            {
                var context = beauty_salonEntities.GetContext();
                var freshService = context.Service.FirstOrDefault(s => s.ID == selectedService.ID);
                NavigationService.Navigate(new EditService(freshService));
            }
        }

        private void btnEditService_Click(object sender, RoutedEventArgs e)
        {
            if (adminMode)
            {
                var button = sender as Button;
                if (button?.DataContext is Service selectedService)
                {
                    var context = beauty_salonEntities.GetContext();
                    var freshService = context.Service.FirstOrDefault(s => s.ID == selectedService.ID);
                    NavigationService.Navigate(new EditService(freshService));
                }
            }
        }

        private void btnDeleteService_Click(object sender, RoutedEventArgs e)
        {
            if (!adminMode)
            {
                MessageBox.Show("Для удаления услуг необходимо войти в режим администратора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = sender as Button;
            if (button == null) return;

            var service = button.DataContext as Service;
            if (service == null) return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить услугу \"{service.Title}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var context = beauty_salonEntities.GetContext();

                    var serviceToDelete = context.Service
                        .Include("ServicePhoto")
                        .Include("ClientService")
                        .FirstOrDefault(s => s.ID == service.ID);

                    if (serviceToDelete == null)
                    {
                        MessageBox.Show("Услуга не найдена в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    bool hasClientRecords = serviceToDelete.ClientService != null && serviceToDelete.ClientService.Any();

                    if (hasClientRecords)
                    {
                        MessageBox.Show("Невозможно удалить услугу, так как с ней связаны записи клиентов.\n\nУдаление запрещено при наличии любых записей (прошлых или будущих).",
                            "Ошибка удаления",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    if (serviceToDelete.ServicePhoto != null && serviceToDelete.ServicePhoto.Any())
                    {
                        context.ServicePhoto.RemoveRange(serviceToDelete.ServicePhoto);
                    }

                    context.Service.Remove(serviceToDelete);
                    context.SaveChanges();

                    MessageBox.Show("Услуга успешно удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadServices();
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException dbEx)
                {
                    MessageBox.Show($"Ошибка при удалении услуги: возможно, существуют связанные записи в базе данных.\n\n{dbEx.InnerException?.Message}",
                        "Ошибка базы данных",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении услуги: {ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}