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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Item> items = new ObservableCollection<Item>();
        public ObservableCollection<Item> cartItems = new ObservableCollection<Item>();

        public ObservableCollection<Coupon> coupons = new ObservableCollection<Coupon>();
        public ObservableCollection<Coupon> usedCoupons = new ObservableCollection<Coupon>();
        public Coupon activeCoupons;

        public double cartTotal = 0;
        public double discountedTotal = 0;

        public MainWindow()
        {
            InitializeComponent();
            GetItems();
            GetCoupons();
        }

        public void GetItems()
        {
            using (StreamReader r = new StreamReader(@"JSON\ShoppingItems.json"))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<ObservableCollection<Item>>(json);

                ShoppingItems.ItemsSource = items;
                ShoppingCart.ItemsSource = cartItems;
            }
        }

        public void GetCoupons()
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

        public string GetTotal()
        {
            cartTotal = 0;

            foreach (var item in cartItems)
                cartTotal += item.price;

            if (activeCoupons != null)
            {
                Discounted_Stackpanel.Visibility = Visibility.Visible;
                discountedTotal = cartTotal - (cartTotal * activeCoupons.discount);
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
            string Cost;
            if (activeCoupons == null)
                 Cost = GetTotal();
            else
                 Cost = discountedTotal.ToString("C2");


            string message = $"Total Item Count: {cartItems.Count}" + Environment.NewLine +
                                   $"Total Cost: {Cost}" + Environment.NewLine +
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
                usedCoupons.Add(activeCoupons);
                activeCoupons = null;
                Discounted_Stackpanel.Visibility = Visibility.Collapsed;
                cartItems.Clear();
                CartTotal.Text = "0";
                CartCount.Text = "0";
            }
        }

        private void Coupon_Added_Click(object sender, RoutedEventArgs e)
        {
            Coupon couponToActivate = coupons.FirstOrDefault(x => x.name == Coupon_Field.Text.ToLower());

            Coupon_Field.Text = "";
            if (couponToActivate == null || usedCoupons.FirstOrDefault(x => x == couponToActivate) != null ? true : false)
            {
                MessageBox.Show("This is not a valid Coupon code.");
                return;
            }

            if (activeCoupons != null)
            {
                MessageBox.Show("A coupon has already been used during this checkout session.");
                return;
            }

            Discounted_Stackpanel.Visibility = Visibility.Visible;
            activeCoupons = couponToActivate;
            CartTotal.Text = GetTotal();
        }
    }
}
