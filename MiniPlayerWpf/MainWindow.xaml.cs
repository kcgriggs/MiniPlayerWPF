using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MiniPlayerWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MusicLib musicLib;
        private MediaPlayer mediaPlayer;
        private ObservableCollection<String> songIds;

        // Custom commands
        public static RoutedCommand Add = new RoutedCommand();
        public static RoutedCommand Update = new RoutedCommand();


        public MainWindow()
        {
            InitializeComponent();

            mediaPlayer = new MediaPlayer();
            musicLib = new MusicLib();

            musicLib.PrintAllTables();
            songIds = new ObservableCollection<string>(musicLib.SongIds);

            Console.WriteLine("Total songs = " + songIds.Count());    

            // Bind the song IDs to the combo box
            songIdComboBox.ItemsSource = songIds;
            
            // Select the first item
            if (songIdComboBox.Items.Count > 0)
            {
                songIdComboBox.SelectedItem = songIdComboBox.Items[0];
            }

           
        }

        
        private Song GetSongDetails(string filename)
        {
            Song song = null;

            try
            {
                // PM> Install-Package taglib
                // http://stackoverflow.com/questions/1750464/how-to-read-and-write-id3-tags-to-an-mp3-in-c
                TagLib.File file = TagLib.File.Create(filename);

                song = new Song
                {
                    Title = file.Tag.Title,
                    Artist = file.Tag.AlbumArtists.Length > 0 ? file.Tag.AlbumArtists[0] : "",
                    Album = file.Tag.Album,
                    Genre = file.Tag.Genres.Length > 0 ? file.Tag.Genres[0] : "",
                    Length = file.Properties.Duration.Minutes + ":" + file.Properties.Duration.Seconds,
                    Filename = filename
                };

                return song;
            }
            catch (TagLib.UnsupportedFormatException)
            {
                DisplayError("You did not select a valid song file.");
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            return song;
        }

      

        private void showDataButton_Click(object sender, RoutedEventArgs e)
        {
            musicLib.PrintAllTables();
        }
        
        private void songIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Display the selected song
            if (songIdComboBox.SelectedItem != null)
            {
                Console.WriteLine("Load song " + songIdComboBox.SelectedItem);
                int songId = Convert.ToInt32(songIdComboBox.SelectedItem);

                // Only one row should be selected
                Song song = musicLib.GetSong(songId);
                titleTextBox.Text = song.Title;
                    artistTextBox.Text =song.Artist;
                    albumTextBox.Text = song.Album;
                    genreTextBox.Text = song.Genre;
                    lengthTextBox.Text = song.Length;
                    filenameTextBox.Text = song.Filename;                    
            }
        }

        private void OpenCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.FileName = "";
            openFileDialog.DefaultExt = "*.wma;*.wav;*mp3";
            openFileDialog.Filter = "Media files|*.mp3;*.m4a;*.wma;*.wav|MP3 (*.mp3)|*.mp3|M4A (*.m4a)|*.m4a|Windows Media Audio (*.wma)|*.wma|Wave files (*.wav)|*.wav|All files|*.*";

            // Show open file dialog box
            bool? result = openFileDialog.ShowDialog();

            // Load the selected song
            if (result == true)
            {
                songIdComboBox.IsEnabled = false;
                Song s = GetSongDetails(openFileDialog.FileName);
                if (s != null)
                {
                    titleTextBox.Text = s.Title;
                    artistTextBox.Text = s.Artist;
                    albumTextBox.Text = s.Album;
                    genreTextBox.Text = s.Genre;
                    lengthTextBox.Text = s.Length;
                    filenameTextBox.Text = s.Filename;
                    mediaPlayer.Open(new Uri(s.Filename));
                    addButton.IsEnabled = true;
                }
            }
        }

        private void AddCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Console.WriteLine("Adding song");

            // Add the selected file to the song table
            Song song = new Song
            {
                Title = titleTextBox.Text,
                Artist = artistTextBox.Text,
                Album = albumTextBox.Text,
                Genre = genreTextBox.Text,
                Length = lengthTextBox.Text,
                Filename = filenameTextBox.Text
            };
            string id = musicLib.AddSong(song).ToString();
            // Add the song ID to the combo box
            songIdComboBox.IsEnabled = true;
            (songIdComboBox.ItemsSource as ObservableCollection<string>).Add(id);
            songIdComboBox.SelectedIndex = songIdComboBox.Items.Count - 1;
            // There is at least one song that can be deleted
            deleteButton.IsEnabled = true;

        }

        private void UpdateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int songId = Convert.ToInt32(songIdComboBox.SelectedItem);
            Console.WriteLine("Updating song " + songId);

            Song updatedSong = new Song();
            updatedSong.Title = titleTextBox.Text;
            updatedSong.Artist = artistTextBox.Text;
            updatedSong.Album = albumTextBox.Text;
            updatedSong.Genre = genreTextBox.Text;
            updatedSong.Length = lengthTextBox.Text;
            updatedSong.Filename = filenameTextBox.Text;

            musicLib.UpdateSong(songId, updatedSong);
        }

        private void UpdateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = songIds != null && songIds.Count() > 0;
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {            
            if (MessageBox.Show("Are you sure you want to delete this song?", "MiniPlayer",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                int songId = Convert.ToInt32(songIdComboBox.SelectedItem);
                Console.WriteLine("Deleting song " + songId);

                // Search the primary key for the selected song and delete it from 
                // the song table
                musicLib.DeleteSong(songId);

                // Remove from playlist_song every occurance of songId.
                // Add rows to a separate list before deleting because we'll get an exception
                // if we try to delete more than one row while looping through table.Rows

               

                // Remove the song from the combo box and select the next item
                songIds.Remove(songIdComboBox.SelectedItem.ToString());
                if (songIdComboBox.Items.Count > 0)
                    songIdComboBox.SelectedItem = songIdComboBox.Items[0];
                else
                {
                    // No more songs to display
                    titleTextBox.Text = "";
                    artistTextBox.Text = "";
                    albumTextBox.Text = "";
                    genreTextBox.Text = "";
                    lengthTextBox.Text = "";
                    filenameTextBox.Text = "";
                }
            }
        }
        
        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = songIds != null && songIds.Count() > 0;
        }

        private void PlayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Open(new Uri(filenameTextBox.Text));
            mediaPlayer.Play();
        }

        private void StopCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Stop();            
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            musicLib.Save();
        }

        private void DisplayError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "MiniPlayer", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
