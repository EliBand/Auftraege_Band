using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO.Packaging;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
//using System.Drawing;

namespace Auftrag1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path_images = @"..\\..\\Images\\";
        string path_saveFiles = @"..\\..\\SaveFiles\\";
        string path_pdfSaveFiles = @"..\\..\\PDFSaveFiles\\";
        List<Person> _people = new List<Person>();
        Person currentPerson = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_addImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            string absolute_path = Path.Combine(Directory.GetCurrentDirectory(), path_images).ToString();
            file.InitialDirectory = Path.GetFullPath((new Uri(absolute_path)).LocalPath);

            file.DefaultExt = ".png";

            if (file.ShowDialog() == true)
            {
                currentPerson.Picture = 
                image.Source = new BitmapImage(new Uri(file.FileName));
            }
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            save(false);
        }
        private void btn_saveAsPDF_Click(object sender, RoutedEventArgs e)
        {
            save(true);
        }
        private void save(bool asPDF)
        {
            bool error = false;
            string errormessage = "";

            // Email validierung mit regEx.
            if (isValid(tb_email.Text, RegexType.Email))
            {
                if (isValid(tb_address_zip.Text, RegexType.Nr_4Digits))
                {
                    if (tb_firstname.Text == "" || tb_lastname.Text == "")
                    {
                        errormessage = "Namen bitte komplet angeben";
                    }
                    else if (tb_address_street.Text == "" || tb_address_snr.Text == "" || tb_address_city.Text == "" || tb_address_country.Text == "")
                    {
                        errormessage = "Adresse bitte komplet angeben";
                    }
                    else
                    {
                        // Save as Json file
                        Address currentAddress = new Address(tb_address_street.Text, tb_address_snr.Text, tb_address_zip.Text, tb_address_city.Text, tb_address_country.Text);
                        currentPerson = new Person(tb_firstname.Text, tb_lastname.Text, currentAddress, tb_email.Text, image.Source);
                        string json = JsonConvert.SerializeObject(currentPerson);
                        string filename = currentPerson.FirstName + currentPerson.LastName;

                        string file = path_saveFiles + filename + ".txt";
                        File.WriteAllText(file, json);

                        // Save as PDF
                        if (asPDF)
                        {
                            
                            saveSnapshotToPDF(filename + "Image");
                            buildPDF(filename, currentPerson);
                        }
                    }
                }
                else
                {
                    errormessage = "Keine gültige Postleizahl";
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
        public void saveSnapshotToPDF(string filename)
        {
            Document pdf = new Document(PageSize.A4, 25, 25, 30, 30);

            MemoryStream lMemoryStream = new MemoryStream();
            Package package = Package.Open(lMemoryStream, FileMode.Create);
            FileStream fs = new FileStream(path_pdfSaveFiles + filename + ".pdf", FileMode.Create);
            PdfWriter writer = PdfWriter.GetInstance(pdf, fs);

            writer.Open();
            pdf.Open();
            pdf.AddTitle(filename);

            // save the card part of the WPF window as an image
            string filepath = path_images + filename + ".png";
            Extensions.SnapShotPNG(grd_card, filepath, 1);

            // add previously created image to pdf file
            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(filepath);
            pdf.Add(image);
            
            pdf.Close();
        }
        public void buildPDF(string filename, Person data)
        {
            // Create a pdf file
            Document pdf = new Document(PageSize.A4, 25, 25, 30, 30);

            MemoryStream lMemoryStream = new MemoryStream();
            Package package = Package.Open(lMemoryStream, FileMode.Create);
            FileStream fs = new FileStream(path_pdfSaveFiles + filename + ".pdf", FileMode.Create);
            PdfWriter writer = PdfWriter.GetInstance(pdf, fs);

            writer.Open();
            pdf.Open();
            pdf.AddTitle(filename);

            // Fonts
            BaseFont f_a = BaseFont.CreateFont("c:\\windows\\fonts\\Arial.ttf", BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

            PdfContentByte cb = writer.DirectContent;
            cb.BeginText();
            int fontsize = 20;
            int gap = 3;
            cb.SetFontAndSize(f_a, fontsize);

            string picPath = data.Picture.ToString();
            int x = 30;
            int y = 820;

            PropertyInfo[] properties = typeof(Person).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                y -= fontsize - gap;
                if (property.PropertyType == data.Address.GetType())
                {
                    PropertyInfo[] address = typeof(Address).GetProperties();
                    foreach (PropertyInfo ap in address)
                    {
                        y -= fontsize - gap;
                        cb.SetTextMatrix(x, y);
                        cb.ShowText(ap.GetValue(data.Address).ToString());
                    }
                }
                else if (property.PropertyType == typeof(ImageSource))
                {
                    // load Image if exists
                    if (true)
                    {
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(picPath);
                        img.ScaleAbsolute(130, 130);
                        img.SetAbsolutePosition(400, 680);
                        cb.AddImage(img);
                    }
                }
                else
                {
                    cb.SetTextMatrix(x, y);
                    cb.ShowText(property.GetValue(data).ToString());
                }
            }
            cb.EndText();
            pdf.Close();
        }

        public enum RegexType
        {
            Email,
            Nr_4Digits
        }
        public bool isValid(string input, RegexType type)
        {
            string regexString = "";
            switch (type)
            {
                case RegexType.Email:
                    regexString =
                        @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" +
                        "@" +
                        @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
                    break;
                case RegexType.Nr_4Digits:
                    regexString = @"\d{4}";
                    break;
                default:
                    break;
            }
            Regex regex = new Regex(regexString);
            Match match = regex.Match(input);
            return match.Success;
        }

        private void btn_load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();

            string absolute_path = Path.Combine(Directory.GetCurrentDirectory(), path_saveFiles).ToString();
            file.InitialDirectory = Path.GetFullPath((new Uri(absolute_path)).LocalPath);
            if (file.ShowDialog() == true)
            {
                if (file.FileName.EndsWith(".txt"))
                {
                    try
                    {
                        currentPerson = JsonConvert.DeserializeObject<Person>(File.ReadAllText(file.FileName));
                        fillFormFromPerson(currentPerson);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    MessageBox.Show("Die gewählte Datei entspricht nicht dem erwarteten Format.");
                }
            }
        }
        public void fillFormFromPerson(Person person)
        {
            tb_firstname.Text = person.FirstName;
            tb_lastname.Text = person.LastName;
            tb_email.Text = person.Email;
            tb_address_street.Text = person.Address.Street;
            tb_address_snr.Text = person.Address.StreetNumber;
            tb_address_zip.Text = person.Address.ZipCode;
            tb_address_city.Text = person.Address.City;
            tb_address_country.Text = person.Address.Country;
            image.Source = person.Picture;
        }

        private void btn_new_Click(object sender, RoutedEventArgs e)
        {
            emptyForm();
        }
        public void emptyForm()
        {
            tb_firstname.Text = 
            tb_lastname.Text = 
            tb_email.Text = 
            tb_address_street.Text = 
            tb_address_snr.Text = 
            tb_address_zip.Text = 
            tb_address_city.Text = 
            tb_address_country.Text = "";
            image.Source = new BitmapImage();
        }
    }
    public static class Extensions
    {
        public static void SnapShotPNG(this UIElement source, string filePath, int zoom)
        {
            try
            {
                double actualHeight = source.RenderSize.Height;
                double actualWidth = source.RenderSize.Width;
                
                double renderHeight = actualHeight * zoom;
                double renderWidth = actualWidth * zoom;

                RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
                VisualBrush sourceBrush = new VisualBrush(source);

                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();

                using (drawingContext)
                {
                    drawingContext.PushTransform(new ScaleTransform(zoom, zoom));
                    drawingContext.DrawRectangle(sourceBrush, null, new Rect(new System.Windows.Point(0, 0), new System.Windows.Point(actualWidth, actualHeight)));
                }
                renderTarget.Render(drawingVisual);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    encoder.Save(stream);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        /// <summary>
         /// Resize the image to the specified width and height.
         /// </summary>
         /// <param name="image">The image to resize.</param>
         /// <param name="width">The width to resize to.</param>
         /// <param name="height">The height to resize to.</param>
         /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public ImageSource Picture { get; set; }
        public Address Address { get; set; }
        public Person()
        {

        }
        public Person(string firstname, string lastname, Address address, string email, ImageSource picture)
        {
            FirstName = firstname;
            LastName = lastname;
            Address = address;
            Email = email;
            Picture = picture;
        }
    }
    public class Address
    {
        public string Street { get; set; }
        public string StreetNumber { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public Address(string street, string stNr, string zip, string city, string country)
        {
            Street = street;
            StreetNumber = stNr;
            ZipCode = zip;
            City = city;
            Country = country;
        }
    }
    public class TextInputToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Always test MultiValueConverter inputs for non-null 
            // (to avoid crash bugs for views in the designer) 
            if (values[0] is bool && values[1] is bool)
            {
                bool hasText = !(bool)values[0];
                bool hasFocus = (bool)values[1];
                if (hasFocus || hasText)
                    return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
