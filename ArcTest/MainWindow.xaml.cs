using Esri.ArcGISRuntime.UI;
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
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;


namespace ArcTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Constants for OAuth-related values ...
        // URL of the server to authenticate with (ArcGIS Online)
        private const string ArcGISOnlineUrl = "http://www.arcgis.com/sharing/rest";
        // Client ID for the app registered with the server (Portal Maps)
        private const string AppClientId = "jurkLK12eJ3n4bS9";
        // Redirect URL after a successful authorization (configured for the Portal Maps application)
        private const string OAuthRedirectUrl = "http://myapps.portalmapapp";
       
        private string[] _basemapNames = new string[]
        {
        "Topographic",
        "Streets",
        "Imagery",
        "Imagery with Labels",
        "Ocean",
        "National Geographic",
        "Light Gray"
        };

       

        // ChallengeHandler function that will be called whenever access to a secured resource is attempted
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;
            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (Exception ex)
            {
                // Exception will be reported in calling function
                throw (ex);
            }
            return credential;
        }
        public MainWindow()
        {
            InitializeComponent();
            UpdateAuthenticationManager();
            // Fill the basemap combo box with basemap names
            BasemapListBox.ItemsSource = _basemapNames;
            // Select the first basemap in the list by default
            BasemapListBox.SelectedIndex = 0;

            BasemapListBox.SelectionChanged += BasemapListBox_SelectionChanged;

            SaveMapButton.Click += SaveMapButton_Click;
        }

        private async Task LoginToArcGISOnlineAsync()
        {
            // Create an OAuth credential request
            CredentialRequestInfo loginInfo = new CredentialRequestInfo();
            // Indicate the portal (url) to authenticate with (ArcGIS Online)
            loginInfo.ServiceUri = new Uri(ArcGISOnlineUrl);
            // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
            await AuthenticationManager.Current.GetCredentialAsync(loginInfo, false);
        }

        private async void SaveMapButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the current map
            Map myMap = MyMapView.Map;

          
           
         
            try
            {
                await LoginToArcGISOnlineAsync();
                if (myMap.Item == null)
                {
                    // Get the ArcGIS Online portal
                    ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync();

                    // Get information for the new portal item
                    var title = TitleTextBox.Text;
                    var description = DescriptionTextBox.Text;
                    var tags = TagsTextBox.Text.Split(',');

                    // Show the progress bar so the user knows it's working
                    SaveProgressBar.Visibility = Visibility.Visible;

                    // Save the current map as a portal item in the user's default folder
                    await myMap.SaveAsAsync(agsOnline, null, title, description, tags, null);
                    MessageBox.Show("Saved '" + title + "' to ArcGIS Online!", "Map Saved");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving map to ArcGIS Online: " + ex.Message);
            }
            finally
            {
                // Hide the progress bar
                SaveProgressBar.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateAuthenticationManager()
        {
            // Define the server information for ArcGIS Online
            ServerInfo portalServerInfo = new ServerInfo();
            // ArcGIS Online URI
            portalServerInfo.ServerUri = new Uri(ArcGISOnlineUrl);
            // Type of token authentication to use
            portalServerInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit;
            // Define the OAuth information
            OAuthClientInfo oAuthInfo = new OAuthClientInfo
            {
                ClientId = AppClientId,
                RedirectUri = new Uri(OAuthRedirectUrl)
            };
            portalServerInfo.OAuthClientInfo = oAuthInfo;
            // Get a reference to the (singleton) AuthenticationManager for the app
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;
            // Register the ArcGIS Online server information with the AuthenticationManager
            thisAuthenticationManager.RegisterServer(portalServerInfo);
            // Use the OAuthAuthorize class in this project to create a new web view to show the login UI
            thisAuthenticationManager.OAuthAuthorizeHandler = new OAuthAuthorize();
            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        private void BasemapListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the name of the selected basemap
            var basemapName = BasemapListBox.SelectedValue.ToString();
            // Set the basemap for the map according to the user's choice in the list box
            Map myMap = MyMapView.Map;
            switch (basemapName)
            {
                case "Topographic":
                    // Set the basemap to Topographic
                    myMap.Basemap = Basemap.CreateTopographic();
                    break;
                case "Streets":
                    // Set the basemap to Streets
                    myMap.Basemap = Basemap.CreateStreets();
                    break;
                case "Imagery":
                    // Set the basemap to Imagery
                    myMap.Basemap = Basemap.CreateImagery();
                    break;
                case "Imagery with Labels":
                    // Set the basemap to Imagery with labels
                    myMap.Basemap = Basemap.CreateImageryWithLabels();
                    break;
                case "Ocean":
                    // Set the basemap to Oceans
                    myMap.Basemap = Basemap.CreateOceans();
                    break;
                case "National Geographic":
                    // Set the basemap to National Geographic
                    myMap.Basemap = Basemap.CreateNationalGeographic();
                    break;
                case "Light Gray":
                    // Set the basemap to Light Gray Canvas
                    myMap.Basemap = Basemap.CreateLightGrayCanvas();
                    break;
                default:
                    break;
            }
        }

        // Map initialization logic is contained in MapViewModel.cs
    }
}
