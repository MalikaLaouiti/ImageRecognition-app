using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;


namespace ImageRecognition_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Add Computer Vision key and endpoint
        static string? key = Environment.GetEnvironmentVariable("VISION_KEY");
        static string? endpoint = Environment.GetEnvironmentVariable("VISION_ENDPOINT");

        public string? UserInput;

        //indice d'analyse d'image
        public bool valid = false;

        private string? selectedFileName;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Validation Button clicked!");
        }
        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG Files (*.png)|*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                // Process selected file
                selectedFileName = openFileDialog.FileName;
                MessageBox.Show($"Selected file: {selectedFileName}");
                try
                {
                    // Load the selected image file into the Image control
                    BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                    holdImage.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("browse PNG ");
            }
        }

        private void ResetImage_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                selectedFileName = null;
                holdImage.Source = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reset image: {ex.Message}");
            }

        }

        private async void ValiderButton_Click(object sender, RoutedEventArgs e)
        {
            YesButton.Background = new SolidColorBrush(Color.FromRgb(75, 152, 179));
            YesButton.Foreground = new SolidColorBrush(Colors.Black);
            NoButton.Background = new SolidColorBrush(Color.FromRgb(75, 152, 179));
            NoButton.Foreground = new SolidColorBrush(Colors.Black);

            try
            {
                if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
                {
                    MessageBox.Show("Clé ou point de terminaison non définis. Veuillez vérifier vos valeurs.");
                    return;
                }

                ComputerVisionClient client = Authenticate(endpoint, key);

                if (!string.IsNullOrEmpty(selectedFileName))
                {
                    if (!string.IsNullOrEmpty(InputTextBox.Text))
                    {

                    await AnalyzeImageLocal(client, selectedFileName, this);

                    // Update UI based on analysis if needed
                    if (valid)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            YesButton.Background = new SolidColorBrush(Color.FromRgb(20, 201, 120));
                            YesButton.Foreground = new SolidColorBrush(Colors.Black);
                            
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            NoButton.Background = new SolidColorBrush(Color.FromRgb(193, 18, 31));
                            NoButton.Foreground = new SolidColorBrush(Colors.Black);
                            
                        });
                    }
                }
                else
                {
                        MessageBox.Show("No text ");
                    }
                }
                else
                {
                    MessageBox.Show("No image selected.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur s'est produite : {ex.Message}");
            }

            
        }
        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
                new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
                { Endpoint = endpoint };
            return client;
        }

        public static async Task AnalyzeImageLocal(ComputerVisionClient client, string imagePath, MainWindow mainWindow)
        {

            // Créer une liste qui définit les fonctionnalités à extraire de l'image.
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
        {
            VisualFeatureTypes.Tags
        };

            // Ouvrir le fichier image  en tant que flux
            using (Stream imageStream = File.OpenRead(imagePath))
            {
                // Analyser l'image 
                ImageAnalysis results = await client.AnalyzeImageInStreamAsync(imageStream, visualFeatures: features);

                string input = mainWindow.InputTextBox.Text;
                // Tags de l'image et leur score de confiance

                foreach (var tag in results.Tags)
                {
                    if (tag.Name == input)
                    {
                        mainWindow.valid = true;
                        
                        return;
                    }
             
                }
                mainWindow.valid = false;
                
            }
        }
        
    }
}