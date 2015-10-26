using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;

namespace WhisperingAudioMusicLibrary
{
    [Serializable()]
    public class Playlist : IEnumerable<Track>
    {
        private List<Track> playlist;
        private string name;

        // parameterless constructor for serialization
        public Playlist()
        {
            playlist = new List<Track>();
            //name = "default";
        }

        public Playlist(string playlistName)
        {
            name = playlistName;
            playlist = new List<Track>();
        }

        public Playlist(string playlistName, List<Track> songs)
        {
            name = playlistName;
            playlist = songs;
        }

        // Needed for being able to loop through collection with a foreach
        public IEnumerator<Track> GetEnumerator()
        {
            return playlist.GetEnumerator();
        }

        // Needed for being able to loop through collection with a foreach
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        //Needed for serialization of IEnumerable classes
        public void Add(object o)
        {
            playlist.Add((Track)o);
        }


        public static Playlist OpenPlaylist(string playlistName)
        {
            try
            {
                string fileName = playlistName + ".wapl";
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(Playlist));
                StreamReader file = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\" + fileName);
                Playlist result = (Playlist)reader.Deserialize(file);
                file.Close();
                result.Name = playlistName;
                return result;
            }
            catch (Exception exc)
            {
                Console.WriteLine("Playlist not found: " + exc.Message);
                return null;
            }
        }

        public void SavePlaylist()
        {
            string fileName = name + ".wapl";
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(Playlist));

            StreamWriter file = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\" + fileName);
            writer.Serialize(file, this);
            file.Close();
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<Track> Songs
        {
            get { return playlist; }
            set { playlist = value; }
        }


        /// <summary>
        /// Get a list of available Playlists.
        /// </summary>
        /// <returns>List of MusicLibraries</returns>
        public static List<Playlist> GetAvailablePlaylists()
        {
            List<Playlist> results = new List<Playlist>();
            foreach (string file in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp"))
            {
                if (file.ToLower().EndsWith(".wapl"))
                    results.Add(OpenPlaylist(file.Remove(file.Length - 5).Split('\\').Last()));
            }

            return results;
        }


        public List<Track> GetSongs()
        {
            return playlist;
        }

        public void RemoveSongs(List<Track> songs)
        {
            foreach (Track t in songs)
            {
                playlist.Remove(t);
            }
        }

        public void RemoveSong(Track song)
        {
            playlist.Remove(song);
        }

        public void AddSong(Track song)
        {
            playlist.Add(song);
        }

        public void AddSongs(List<Track> songs)
        {
            playlist.AddRange(songs);
        }

        public void ClearAllSongs()
        {
            playlist.Clear();
        }

        public override string ToString()
        {
            return name;
        }
    }
}
