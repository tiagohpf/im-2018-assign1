using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using SpotifyAPI.Local; //Base Namespace
using SpotifyAPI.Local.Enums; //Enums
using SpotifyAPI.Local.Models; //Models for the JSON-responses

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;
        private static SpotifyLocalAPI spotify;
        public MainWindow()
        {
            InitializeComponent();

            SpotifyAPI spotifyAPI = new SpotifyAPI();
            spotify = new SpotifyLocalAPI(new SpotifyLocalAPIConfig
            {
                Port = 4381,
                HostUrl = "http://localhost"
            });
            if (!SpotifyLocalAPI.IsSpotifyRunning())
            {
                MessageBox.Show("Spotify is not running");
                return; //Make sure the spotify client is running
            }
            if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
            {
                MessageBox.Show("Spotify WebHelper is not running");
                return; //Make sure the WebHelper is running
            }

            if (!spotify.Connect())
            {
                MessageBox.Show("Spotify is not connected");
                return; //We need to call Connect before fetching infos, this will handle Auth stuff
            }

            StatusResponse status = spotify.GetStatus(); //status contains infos
            //spotify.Play();

            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            Shape _s = null;
            switch ((string)json.recognized[0].ToString())
            {
                case "PLAY":
                    spotify.Play();
                    break;
                case "PAUSE":
                    spotify.Pause();
                    break;
                case "SKIP":
                    spotify.Skip();
                    break;
                case "BACK":
                    spotify.Previous();
                    break;
                case "VDOWN":
                    float volume_down = spotify.GetSpotifyVolume();
                    if(volume_down > 0)
                    {
                        spotify.SetSpotifyVolume(volume_down - 50);
                    }
                    break;
                case "VUP":
                    float volume_up = spotify.GetSpotifyVolume();
                    if(volume_up < 100)
                    {
                        spotify.SetSpotifyVolume(volume_up + 50);
                    }
                    break;
                case "MUTE":
                    if(!spotify.IsSpotifyMuted())
                        spotify.Mute();
                    break;
                case "UNMUTE":
                    if (spotify.IsSpotifyMuted())
                        spotify.UnMute();
                    break;
            }

            /*App.Current.Dispatcher.Invoke(() =>
            {
                switch ((string)json.recognized[1].ToString())
                {
                    case "GREEN":
                        _s.Fill = Brushes.Green;
                        break;
                    case "BLUE":
                        _s.Fill = Brushes.Blue;
                        break;
                    case "RED":
                        _s.Fill = Brushes.Red;
                        break;
                }
            });*/

        }
    }
}
