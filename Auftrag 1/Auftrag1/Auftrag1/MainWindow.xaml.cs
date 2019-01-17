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
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Auftrag1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Person> _people = new List<Person>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_addImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            bool error = false;
            string errormessage = "";

            // Email validierung mit regEx.
            if (isValidEmail(tb_email.Text))
            {
                if (tb_firstname.Text != "")
                {

                }
            }
            else
            {
                errormessage = "Keine gültige Email";
            }
            if (error)
            {
                MessageBox.Show(errormessage, "Achtung", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public bool isValidEmail(string email)
        {
            Regex regex = new Regex(
                @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" +
                "@" +
                @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$");
            Match match = regex.Match(email);
            return match.Success;
        }
    }
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Address HomeAddress { get; set; }
        public string Email { get; set; }
        public Person(string firstname, string lastname, Address address, string email)
        {
            FirstName = firstname;
            LastName = lastname;
            HomeAddress = address;
            Email = email;
        }
    }
    public class Address
    {
        public string Street { get; set; }
        public int StreetNumber { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public Address(string street, int stNr, int zip, string city, string country)
        {
            Street = street;
            StreetNumber = stNr;
            ZipCode = zip;
            City = city;
            Country = country;
        }
    }
}
