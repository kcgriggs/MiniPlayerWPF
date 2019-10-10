using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace MiniPlayerWpf
{
    public class MusicLib
    {
        private DataSet musicDataSet;
        public MusicLib()
        {
            try
            {
                musicDataSet = new DataSet();
                musicDataSet.ReadXmlSchema("music.xsd");
                musicDataSet.ReadXml("music.xml");
            }
            catch (Exception e)
            {
                DisplayError("Error loading file: " + e.Message);
            }
        }

        public EnumerableRowCollection<string> SongIds
        {
            get
            {
                var ids = from row in musicDataSet.Tables["song"].AsEnumerable()
                          orderby row["id"]
                          select row["id"].ToString();
                return ids;
            }
        }

        public int AddSong(Song song)
        {
            DataTable table = musicDataSet.Tables["song"];
            bool exists = table.Rows.Find(song.Id) != null;

            if (exists) return -1;
           
            DataRow row = table.NewRow();
            row["title"] = song.Title;
            row["artist"] = song.Artist;
            row["album"] = song.Album;
            row["filename"] = song.Filename;
            row["length"] = song.Length;
            row["genre"] = song.Genre;
            table.Rows.Add(row);

            // Get the song's newly assigned ID
            song.Id = Convert.ToInt32(row["id"]);
            return song.Id;
        }

        public Song GetSong(int songId)
        {
            DataTable table = musicDataSet.Tables["song"];
            DataRow row = table.Rows.Find(songId);
            Song result = new Song();
            if (row != null)
            {
                result.Id = (int)row["id"];
                result.Title = row["title"].ToString();
                result.Artist = row["artist"].ToString();
                result.Album = row["album"].ToString();
                result.Genre = row["genre"].ToString();
                result.Length = row["length"].ToString();
                result.Filename = row["filename"].ToString();
                return result;
            }
            else return null;
        }

        public bool UpdateSong(int songId, Song song)
        {
            DataTable table = musicDataSet.Tables["song"];
            DataRow row = table.Rows.Find(songId);
            if (row != null)
            {
                row["title"] = song.Title;
                row["artist"] = song.Artist;
                row["album"] = song.Album;
                row["genre"].ToString();
                row["length"].ToString();
                row["filename"].ToString();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DeleteSong(int songId)
        {
            DataTable table = musicDataSet.Tables["song"];
            DataRow row = table.Rows.Find(songId);
            if(row != null)
            {
                table.Rows.Remove(row);
                List<DataRow> rows = new List<DataRow>();
                table = musicDataSet.Tables["playlist_song"];
                foreach (DataRow row2 in table.Rows)
                    if (row2["song_id"].ToString() == songId.ToString())
                        rows.Add(row2);

                foreach (DataRow row2 in rows)
                    row.Delete();

                return true;
            }
            else
            {
                return false;
            }
           
        }
        public void Save()
        {
            musicDataSet.WriteXml("music.xml");
        }
        public void PrintAllTables()
        {
            foreach (DataTable table in musicDataSet.Tables)
            {
                Console.WriteLine("Table name = " + table.TableName);
                foreach (DataRow row in table.Rows)
                {
                    Console.WriteLine("Row:");
                    int i = 0;
                    foreach (Object item in row.ItemArray)
                    {
                        Console.WriteLine(" " + table.Columns[i].Caption + "=" + item);
                        i++;
                    }
                }
                Console.WriteLine();
            }
        }

        private void DisplayError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "MiniPlayer", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
