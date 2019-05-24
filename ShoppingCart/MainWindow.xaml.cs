using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using FontAwesome.WPF;
using ShoppingCart.Models;

namespace ShoppingCart
{
    public partial class MainWindow : Window
    {
        ObservableCollection<Item> items;
        ObservableCollection<Item> cartItems;

        ObservableCollection<Coupon> coupons;
        ObservableCollection<Coupon> usedCoupons;
        Coupon activeCoupon;

        public double cartTotal = 0;
        public double discountedTotal = 0;

        public MainWindow()
        {
            InitializeComponent();

            items = new ObservableCollection<Item>();
            cartItems = new ObservableCollection<Item>();
            coupons = new ObservableCollection<Coupon>();
            usedCoupons = new ObservableCollection<Coupon>();

            GetItems();
            GetCoupons();
        }

        private void GetItems()
        {
            using (StreamReader r = new StreamReader(@"JSON\ShoppingItems.json"))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<ObservableCollection<Item>>(json);

                ShoppingItems.ItemsSource = items;
                ShoppingCart.ItemsSource = cartItems;
            }
        }

        private void GetCoupons()
        {
            using (StreamReader r = new StreamReader(@"JSON\CouponCodes.json"))
            {
                string json = r.ReadToEnd();
                coupons = JsonConvert.DeserializeObject<ObservableCollection<Coupon>>(json);
            }
        }

        private void ShoppingItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Item selectedItem = (Item)((ListBox)sender).SelectedItem;
            if (selectedItem == null)
                return;

            cartItems.Add(selectedItem);
            CartTotal.Text = GetTotal();
            CartCount.Text = cartItems.Count.ToString();
            ShoppingItems.SelectedItem = null;
        }

        private string GetTotal()
        {
            cartTotal = 0;

            foreach (var item in cartItems)
                cartTotal += item.price;

            if (activeCoupon != null)
            {
                Discounted_Stackpanel.Visibility = Visibility.Visible;
                discountedTotal = cartTotal - (cartTotal * activeCoupon.discount);
                DiscountTotal.Text = discountedTotal.ToString("C2");
            }

            return cartTotal.ToString("C2");
        }

        private void Item_Remove_Btn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Item selectedItem = (Item)((ImageAwesome)sender).DataContext;
            cartItems.Remove(selectedItem);
            CartTotal.Text = GetTotal();
            CartCount.Text = cartItems.Count.ToString();
        }

        private void CheckOut_Click(object sender, RoutedEventArgs e)
        {
            string cost;
            if (activeCoupon == null)
                 cost = GetTotal();
            else
                 cost = discountedTotal.ToString("C2");


            string message = $"Total Item Count: {cartItems.Count}" + Environment.NewLine +
                                   $"Total Cost: {cost}" + Environment.NewLine +
                                   Environment.NewLine +
                                   $"Would you like to place your order?";
            

            string caption = "Form Closing";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                e.Handled = true;

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Your order has been placed.");
                usedCoupons.Add(activeCoupon);
                activeCoupon = null;
                Discounted_Stackpanel.Visibility = Visibility.Collapsed;
                cartItems.Clear();
                CartTotal.Text = "0";
                CartCount.Text = "0";
            }
        }

    }
}
