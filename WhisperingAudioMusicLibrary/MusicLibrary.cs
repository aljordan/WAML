using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
//using System.Threading;

namespace WhisperingAudioMusicLibrary
{
    public delegate void StatusUpdateEventHandler(object sender, StatusChangedEventArgs e);

    public class MusicLibrary
    {
        public event StatusUpdateEventHandler StatusChangedEvent;
        private string libraryRoot;
        private string libraryName;
        private string databaseName; // library name with sqlite ending.
        private Boolean okToCreateLibrary;
//        private SQLiteConnection dbConnection;
        private string sqlConnectionString;


#region MusicLibrary constructors
        /// <summary>
        /// Constructor - create a new instance of a music library
        /// </summary>
        public MusicLibrary()
        {
            okToCreateLibrary = false;
            LibraryName = "Default library";
        }

        public MusicLibrary(string name)
        {
            okToCreateLibrary = false;
            LibraryName = name;
        }

        public MusicLibrary(string name, string libraryRootFolderPath)
        {
            okToCreateLibrary = false;
            LibraryName = name;
            LibraryRoot = libraryRootFolderPath;
        }
#endregion


#region public properties 
        /// <summary>
        /// Get or set the file system folder that is the root of where music filed are stored.
        /// </summary>
        public string LibraryRoot
        {
            get { return libraryRoot; }
            set
            {
                if (Directory.Exists(value))
                {
                    libraryRoot = value;
                    okToCreateLibrary = true;
                }
                else
                {
                    okToCreateLibrary = false;
                    throw new LibraryRootNotFoundException("Invalid library root folder.");
                }
            }
        }


        /// <summary>
        /// Name for this instance of a library
        /// </summary>
        public string LibraryName
        {
            get { return libraryName; }
            set
            {
                libraryName = value;
                databaseName = libraryName + ".sqlite";
                sqlConnectionString = "Data Source=" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\" + databaseName + ";Version=3;";
            }
        }
#endregion

#region public methods

        public override string ToString() 
        {
            return LibraryName;
        }


        /// <summary>
        /// Get a list of available libraries by name.
        /// </summary>
        /// <returns>List of MusicLibraries</returns>
        public static List<MusicLibrary> GetAvailableLibraries() 
        {
            List<MusicLibrary> results = new List<MusicLibrary>();
            //foreach (string file in Directory.GetFiles("."))
            foreach (string file in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp"))
            {
                if (file.ToLower().EndsWith(".sqlite"))
                    results.Add(OpenLibrary(file.Remove(file.Length - 7).Split('\\').Last()));
            }

            return results;
        }


        /// <summary>
        /// Opens an existing music library
        /// </summary>
        /// <param name="libraryName"></param>
        /// <returns>MusicLibrary</returns>
        public static MusicLibrary OpenLibrary(string libraryName)
        {
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\" + libraryName + ".sqlite"))
            {
                MusicLibrary ml = new MusicLibrary(libraryName);
                string databaseName = libraryName + ".sqlite";
                string sqlConnectionString = "Data Source=" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\" + databaseName + ";Version=3;";

                string sql = "select directory_path from library_root";

                SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);
                dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                //SQLiteDataReader reader = command.ExecuteReader();
                //ml.LibraryRoot = reader["library_root"].ToString();
                ml.LibraryRoot = command.ExecuteScalar().ToString();

                dbConnection.Close();

                return ml;
            }
            else
                throw new LibraryNotFoundException("No library found by the name of " + libraryName);
        }


        public static void DeleteLibrary(string libraryName)
        {
            string databaseName = libraryName + ".sqlite";
            System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\" + databaseName);
        }


        /// <summary>
        /// Creates new library based on the LibraryName and LibraryRoot properties
        /// </summary>
        public void CreateLibrary()
        {
            createDatabase();
            populateDatabase();
        }

        /// <summary>
        /// Returns all genres
        /// </summary>
        /// <returns>Typed List of strings</returns>
        public List<string> GetGenres()
        {
            return getData("genre", true);
        }

        /// <summary>
        /// Get all genres filtered by search string
        /// </summary>
        /// <param name="searchString">search string - don't include standard SQL search characters such as %</param>
        /// <returns>Typed List of strings</returns>
        public List<string> GetGenres(string searchString)
        {
            return getFilteredData("genre", searchString, true);
        }

        /// <summary>
        /// Returns all artists
        /// </summary>
        /// <returns>Typed List of strings</returns>
        public List<string> GetArtists()
        {
            return getData("album_artist", true);
        }

        /// <summary>
        /// Get all artists filtered by search string
        /// </summary>
        /// <param name="searchString">search string - don't include standard SQL search characters such as %</param>
        /// <returns>Typed string List</returns>
        public List<string> GetArtists(string searchString)
        {
            return getFilteredData("album_artist", searchString, true);
        }

        /// <summary>
        /// Returns all albums
        /// </summary>
        /// <returns>Typed string List</returns>
        public List<string> GetAlbums()
        {
            return getData("album", true);
        }

        /// <summary>
        /// Get all albums filtered by search string
        /// </summary>
        /// <param name="searchString">search string - don't include standard SQL search characters such as %</param>
        /// <returns>Typed string List</returns>
        public List<string> GetAlbums(string searchString)
        {
            return getFilteredData("album", searchString, true);
        }

        public List<string> GetAlbumsByArtist(string artist)
        {
            string artistEscaped = artist.Replace("'", "''");
            List<string> results = new List<string>();
            if (TableExists("track"))
            {
                SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);
                string sql = "select distinct album from track where album_artist = '" + artistEscaped + "' order by album";

                dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    results.Add(reader["album"].ToString());

                dbConnection.Close();
            }
            return results;

        }

        /// <summary>
        /// Returns all song titles
        /// </summary>
        /// <returns>Typed string List</returns>
        public List<string> GetSongTitles()
        {
            return getData("title", true);
        }

        /// <summary>
        /// Get all song titles filtered by search string
        /// </summary>
        /// <param name="searchString">search string - don't include standard SQL search characters such as %</param>
        /// <returns>Typed string List</returns>
        public List<string> GetSongTitles(string searchString)
        {
            return getFilteredData("title", searchString, true);
        }

        /// <summary>
        /// Returns all songs
        /// </summary>
        /// <returns>Typed Track List</returns>
        public List<Track> GetSongs()
        {
            return getTracks();
        }

        /// <summary>
        /// Get all songs filtered by search string
        /// </summary>
        /// <param name="searchString">search string - don't include standard SQL search characters such as %</param>
        /// <returns>Typed string List</returns>
        public List<Track> GetSongs(string searchString)
        {
            return getFilteredTracks(searchString);
        }


        public List<string> GetArtistsByGenre(string genre)
        {
            SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);
            List<string> results = new List<string>();
            string escapedGenre = genre.Replace("'", "''");

            string sql = "select distinct album_artist from track where genre = '" + escapedGenre + "' order by album_artist";

            dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                results.Add(reader["album_artist"].ToString());

            dbConnection.Close();
            return results;

        }


        public List<Track> GetSongsByAlbum(string albumName)
        {
            SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);
            List<Track> results = new List<Track>();
            string albumEscaped = albumName.Replace("'", "''");

            string sql = "select * from track where album = '" + albumEscaped + "' order by track_number";

            dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                results.Add(new Track((long)reader["id"], reader["title"].ToString(), reader["album_artist"].ToString(),
                    reader["composer"].ToString(), reader["album"].ToString(), reader["genre"].ToString(),
                    (short)((long)reader["track_number"]), (short)((long)reader["year"]),
                    (short)((long)reader["disc"]), reader["file_path"].ToString()));

            dbConnection.Close();
            return results;
        }


        /// <summary>
        /// Looks updates library with newly added or removed music
        /// </summary>
        public void UpdateLibrary()
        {
            //First remove deleted files from library
            OnRaiseStatusChangedEvent(new StatusChangedEventArgs("Checking for deleted files to remove from the library"));
            List<string> filesToDelete = new List<string>();
            SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);

            string sql = "select file_path from track order by file_path";

            dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string file_path = reader["file_path"].ToString();
                if (!System.IO.File.Exists(file_path))
                {
                    filesToDelete.Add(file_path);
                }
            }
            OnRaiseStatusChangedEvent(new StatusChangedEventArgs("Removing " + filesToDelete.Count + " deleted files from the library"));

            // remove missing files from the database
            foreach (string deleteFile in filesToDelete)
            {
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = "DELETE FROM track WHERE file_path = @file_path";
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@file_path", deleteFile);
                    cmd.ExecuteNonQuery();
                    OnRaiseStatusChangedEvent(new StatusChangedEventArgs("Removing " + deleteFile + " from the library"));
                }
            }

            // Now look for new files
            OnRaiseStatusChangedEvent(new StatusChangedEventArgs("Looking for new files to add to the library"));
            foreach (string file in Directory.EnumerateFiles(libraryRoot, "*.*", SearchOption.AllDirectories))
            {
                if (file.ToLower().EndsWith(".aiff") || file.ToLower().EndsWith(".aif") || file.ToLower().EndsWith(".mp3") || file.ToLower().EndsWith(".flac") || file.ToLower().EndsWith(".wav"))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                    {
                        cmd.CommandText = "SELECT count(*) FROM track WHERE file_path = @file_path";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@file_path", file);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count == 0)
                        {
                            try
                            {
                                TagLib.File songInfo = TagLib.File.Create(file);

                                cmd.CommandText = "INSERT INTO track(title, album_artist, composer, album, genre, year, track_number, disc, file_path) VALUES(@title, @album_artist, @composer, @album, @genre, @year, @track_number, @disc, @file_path)";
                                cmd.Prepare();
                                try
                                {
                                    string title = songInfo.Tag.Title.Trim().Replace("\0", String.Empty);
                                    if (title.Trim().Equals(string.Empty))
                                        title = GetTitleFromFilePath(file);
                                    cmd.Parameters.AddWithValue("@title", title);
                                }
                                catch (Exception) { cmd.Parameters.AddWithValue("@title", GetTitleFromFilePath(file)); }

                                string artist = String.Empty;
                                try
                                {
                                    artist = songInfo.Tag.FirstArtist.Trim().Replace("\0", String.Empty);
                                }
                                catch (Exception) { }
                                if (String.IsNullOrEmpty(artist))
                                {
                                    try
                                    {
                                        artist = songInfo.Tag.FirstAlbumArtist.Trim().Replace("\0", String.Empty);
                                    }
                                    catch (Exception) { artist = ""; }
                                }
                                if (artist.Trim().Equals(string.Empty))
                                    artist = GetContainingFolderNameFromFilePath(file, 2);

                                cmd.Parameters.AddWithValue("@album_artist", artist);

                                try { cmd.Parameters.AddWithValue("@composer", songInfo.Tag.FirstComposer.Trim().Replace("\0", String.Empty)); }
                                catch (Exception) { cmd.Parameters.AddWithValue("@composer", ""); }

                                try
                                {
                                    string album = songInfo.Tag.Album.Trim().Replace("\0", String.Empty);
                                    if (album.Trim().Equals(string.Empty))
                                        album = GetContainingFolderNameFromFilePath(file, 1);
                                    cmd.Parameters.AddWithValue("@album", album);
                                }
                                catch (Exception) { cmd.Parameters.AddWithValue("@album", GetContainingFolderNameFromFilePath(file, 1)); }

                                try
                                {
                                    string genre = songInfo.Tag.FirstGenre.Trim().Replace("\0", String.Empty);
                                    if (genre.Trim().Equals(string.Empty))
                                        genre = "No Genre";
                                    cmd.Parameters.AddWithValue("@genre", genre);
                                }
                                catch (Exception) { cmd.Parameters.AddWithValue("@genre", "No Genre"); }

                                cmd.Parameters.AddWithValue("@year", songInfo.Tag.Year);
                                cmd.Parameters.AddWithValue("@track_number", songInfo.Tag.Track);
                                cmd.Parameters.AddWithValue("@disc", songInfo.Tag.Disc);
                                cmd.Parameters.AddWithValue("@file_path", file);
                                cmd.ExecuteNonQuery();
                                OnRaiseStatusChangedEvent(new StatusChangedEventArgs("Processed " + file));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                }
            }

            dbConnection.Close();
            OnRaiseStatusChangedEvent(new StatusChangedEventArgs("Finished updating music library."));
        }

#endregion



#region private methods

        /// <summary>
        /// Returns all songs in the library
        /// </summary>
        /// <returns>Typed List of Tracks</returns>
        private List<Track> getTracks()
        {
            SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);
            List<Track> results = new List<Track>();

            string sql = "select * from track order by album_artist, track_number";

            dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                results.Add(new Track((long)reader["id"],reader["title"].ToString(),reader["album_artist"].ToString(),
                    reader["composer"].ToString(),reader["album"].ToString(),reader["genre"].ToString(),
                    (short)((long)reader["track_number"]), (short)((long)reader["year"]), 
                    (short)((long)reader["disc"]), reader["file_path"].ToString()));

            dbConnection.Close();
            return results;
        }

        /// <summary>
        /// Returns songs filtered by title search string.
        /// </summary>
        /// <returns>Typed List of Tracks</returns>
        private List<Track> getFilteredTracks(string searchString)
        {
            SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);
            List<Track> results = new List<Track>();

            string sql = "select * from track where title like '%" + searchString + "%' order by album_artist, album, track_number";

            dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                results.Add(new Track((long)reader["id"], reader["title"].ToString(), reader["album_artist"].ToString(),
                    reader["composer"].ToString(), reader["album"].ToString(), reader["genre"].ToString(),
                    (short)((long)reader["track_number"]), (short)((long)reader["year"]), 
                    (short)((long)reader["disc"]), reader["file_path"].ToString()));

            dbConnection.Close();
            return results;
        }

        /// <summary>
        /// Returns List of all data matching criteria
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="searchString"></param>
        /// <param name="uniqueEntriesOnly"></param>
        /// <returns></returns>
        private List<string> getFilteredData(string fieldName, string searchString, bool uniqueEntriesOnly)
        {
            List<string> results = new List<string>();
            string distinct;
            if (TableExists("track"))
            {
                SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);

                distinct = uniqueEntriesOnly ? "distinct " : "";

                string sql = "select " + distinct + fieldName + " from track where " + fieldName + " like '%" + searchString + "%' order by " + fieldName;

                dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    results.Add(reader[fieldName].ToString());

                dbConnection.Close();
            }
            return results;
        }

        
        private List<string> getData(string fieldName, bool uniqueEntriesOnly)
        {
            List<string> results = new List<string>();
            string distinct;
            if (TableExists("track"))
            {
                SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);

                distinct = uniqueEntriesOnly ? "distinct " : "";

                string sql = "select " + distinct + fieldName + " from track order by " + fieldName; 

                dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    results.Add(reader[fieldName].ToString());

                dbConnection.Close();
            }
            return results;
        }

        private void createDatabase()
        {
            SQLiteConnection dbConnection;
            if (okToCreateLibrary)
            {
                string[] commands = new string[] {DatabaseCommands.CreateTrackTable, DatabaseCommands.CreateLibraryRootTable, 
                    DatabaseCommands.CreateTrackTitleIndex, DatabaseCommands.CreateTrackAlbumIndex, 
                    DatabaseCommands.CreateTrackGenreIndex};

                //SQLiteConnection.CreateFile(databaseName);
                SQLiteConnection.CreateFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\" + databaseName);
                dbConnection = new SQLiteConnection(sqlConnectionString);
                dbConnection.Open();

                foreach (string commandText in commands)
                {
                    SQLiteCommand command = new SQLiteCommand(commandText, dbConnection);
                    command.ExecuteNonQuery();
                }
                dbConnection.Close();
            }
        }


        /// <summary>
        /// Need this because when library is first created the app will try to load genres and artists before the 
        /// tables are finished being created
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private bool TableExists(string tableName)
        {
            SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);
            dbConnection.Open();
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(*) FROM sqlite_master WHERE name=@TableName";
            var p1 = cmd.CreateParameter();
            p1.ParameterName = "TableName";
            p1.Value = tableName;
            cmd.Parameters.Add(p1);

            var result = cmd.ExecuteScalar();
            dbConnection.Close();

            return ((long)result) == 1;
        }



        /// <summary>
        /// Loops though all files returned from recursiveDirectorySearch and adds them to a single database table. 
        /// </summary>
        private void populateDatabase()
        {
            SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);

            System.Collections.Specialized.StringCollection errors = new System.Collections.Specialized.StringCollection();
            dbConnection.Open();

            SQLiteCommand command = new SQLiteCommand(dbConnection);
            command.CommandText = "INSERT INTO library_root(directory_path) VALUES (@directory_path)";
            command.Prepare();
            try { command.Parameters.AddWithValue("@directory_path", LibraryRoot); }
            catch (Exception) { command.Parameters.AddWithValue("@directory_path", ""); }
            command.ExecuteNonQuery();

 
        	foreach (string file in Directory.EnumerateFiles(libraryRoot, "*.*", SearchOption.AllDirectories))            
            {
                //// Below test was to see what was in strings that were appearing the same
                //// but were in fact different.
                //TagLib.File songInfo = TagLib.File.Create(song);
                //string genre = songInfo.Tag.FirstGenre.Trim();
                //genre = genre.Replace("\0",String.Empty);
                //if (genre != null && genre.Contains("Fusion"))
                //{
                //    byte[] contents = GetBytes(genre);
                //    Console.WriteLine("");
                //}

                if (file.ToLower().EndsWith(".aiff") || file.ToLower().EndsWith(".aif") || file.ToLower().EndsWith(".mp3") || file.ToLower().EndsWith(".flac") || file.ToLower().EndsWith(".wav"))
                {
                    try
                    {
                        TagLib.File songInfo = TagLib.File.Create(file);

                        using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                        {
                            cmd.CommandText = "INSERT INTO track(title, album_artist, composer, album, genre, year, track_number, disc, file_path) VALUES(@title, @album_artist, @composer, @album, @genre, @year, @track_number, @disc, @file_path)";
                            cmd.Prepare();

                            try 
                            {
                                string title = songInfo.Tag.Title.Trim().Replace("\0", String.Empty);
                                if (title.Trim().Equals(string.Empty))
                                    title = GetTitleFromFilePath(file);
                                cmd.Parameters.AddWithValue("@title", title); 
                            }
                            catch (Exception) { cmd.Parameters.AddWithValue("@title", GetTitleFromFilePath(file)); }
                            
                            string artist = String.Empty;
                            try
                            { 
                                artist = songInfo.Tag.FirstArtist.Trim().Replace("\0", String.Empty); 
                            } 
                            catch (Exception) {}
                            if (String.IsNullOrEmpty(artist))
                            {
                                try 
                                {
                                    artist = songInfo.Tag.FirstAlbumArtist.Trim().Replace("\0", String.Empty);
                                }
                                catch (Exception) { artist = ""; }
                            }
                            if (artist.Trim().Equals(string.Empty))
                                artist = GetContainingFolderNameFromFilePath(file, 2);

                            cmd.Parameters.AddWithValue("@album_artist", artist);

                            try { cmd.Parameters.AddWithValue("@composer", songInfo.Tag.FirstComposer.Trim().Replace("\0", String.Empty)); }
                            catch (Exception) { cmd.Parameters.AddWithValue("@composer", ""); }

                            try 
                            {
                                string album = songInfo.Tag.Album.Trim().Replace("\0", String.Empty);
                                if (album.Trim().Equals(string.Empty))
                                    album = GetContainingFolderNameFromFilePath(file, 1);
                                cmd.Parameters.AddWithValue("@album", songInfo.Tag.Album.Trim().Replace("\0", String.Empty)); 
                            }
                            catch (Exception) { cmd.Parameters.AddWithValue("@album", GetContainingFolderNameFromFilePath(file,1)); }
                            
                            try 
                            {
                                string genre = songInfo.Tag.FirstGenre.Trim().Replace("\0", String.Empty);
                                if (genre.Trim().Equals(string.Empty))
                                    genre = "No Genre";
                                cmd.Parameters.AddWithValue("@genre", genre); 
                            }
                            catch (Exception) { cmd.Parameters.AddWithValue("@genre", "No Genre"); }

                            cmd.Parameters.AddWithValue("@year", songInfo.Tag.Year);
                            cmd.Parameters.AddWithValue("@track_number", songInfo.Tag.Track);
                            cmd.Parameters.AddWithValue("@disc", songInfo.Tag.Disc);
                            cmd.Parameters.AddWithValue("@file_path", file);
                            cmd.ExecuteNonQuery();
                        }
                        OnRaiseStatusChangedEvent(new StatusChangedEventArgs("Processed " + file));
                    }
                    catch (Exception e)
                    {
                        errors.Add(file + ": " + e.Message);
                    }
                }
            }

            Console.WriteLine("\nThe following files had errors:");
            foreach (string error in errors)
                Console.WriteLine(error);

            dbConnection.Close();

            OnRaiseStatusChangedEvent(new StatusChangedEventArgs("Finished processing music library."));

        }

        private string GetTitleFromFilePath(string filePath)
        {
            string fileName = filePath.Split('\\').Last();
            int fileEndStart = fileName.LastIndexOf('.');
            return fileName.Substring(0, fileEndStart);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">Full file path.</param>
        /// <param name="offsetFromFileName">Folder offset from ending file.  0 will return file, 
        /// 1 returns foldername that the file is in, etcetera</param>
        /// <returns>string</returns>
        private string GetContainingFolderNameFromFilePath(string filePath, int offsetFromFileName)
        {
            string[] folders = filePath.Split('\\');
            if (folders.Length > 1 + offsetFromFileName)
                return folders[folders.Length - (offsetFromFileName + 1)];
            else
                return "No Name";
        }


        protected virtual void OnRaiseStatusChangedEvent(StatusChangedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            StatusUpdateEventHandler handler = StatusChangedEvent;
            // Raise the event
            if (handler != null)
                handler(this, e);
        }

#endregion

    }
}


///// <summary>
///// Test method to convert strings to byte arrays so I could figure out why genres and artists were
///// looking the same but showing up double when selcting distinct.
///// Issue was null character at the end of the array, so it is now being replaced by an empty string.
///// </summary>
///// <param name="str"></param>
///// <returns></returns>
//private static byte[] GetBytes(string str)
//{
//    byte[] bytes = new byte[str.Length * sizeof(char)];
//    System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
//    return bytes;
//}


///// <summary>
///// Starting at a given folder, loops through all sub-directories and returns all aiff, aif, mp3, and flac files.
///// Keeping this around in case I want to support .Net earlier than version 4.5
///// </summary>
///// <param name="initialDirectory"></param>
///// <param name="files"></param>
//private void recursiveDirectorySearch(string initialDirectory, System.Collections.Specialized.StringCollection files)
//{
//    try
//    {
//        foreach (string file in Directory.GetFiles(initialDirectory))
//        {
//            if (file.ToLower().EndsWith(".aiff") || file.ToLower().EndsWith(".aif") || file.ToLower().EndsWith(".mp3") || file.ToLower().EndsWith(".flac"))
//                files.Add(file);
//        }

//        foreach (string d in Directory.GetDirectories(initialDirectory))
//        {
//            recursiveDirectorySearch(d, files);
//        }
//    }
//    catch (System.Exception excpt)
//    {
//        Console.WriteLine(excpt.Message);
//    }
//}

///// <summary>
///// Old method that works with early versions of .net framework
///// Loops though all files returned from recursiveDirectorySearch and adds them to a single database table. 
///// </summary>
//private void populateDatabase()
//{
//    System.Collections.Specialized.StringCollection errors = new System.Collections.Specialized.StringCollection();
//    dbConnection = new SQLiteConnection(sqlConnectionString);
//    dbConnection.Open();

//    System.Collections.Specialized.StringCollection musicFiles = new System.Collections.Specialized.StringCollection();
//    recursiveDirectorySearch(libraryRoot, musicFiles);
//    foreach (string song in musicFiles)
//    {
//        //TagLib.File songInfo = TagLib.File.Create(song);
//        //string genre = songInfo.Tag.FirstGenre.Trim();
//        //genre = genre.Replace("\0",String.Empty);
//        //if (genre != null && genre.Contains("Fusion"))
//        //{
//        //    byte[] contents = GetBytes(genre);
//        //    Console.WriteLine("");
//        //}
//        try
//        {
//            TagLib.File songInfo = TagLib.File.Create(song);

//            using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
//            {
//                cmd.CommandText = "INSERT INTO track(title, album_artist, composer, album, genre, year, track_number, disc, file_path) VALUES(@title, @album_artist, @composer, @album, @genre, @year, @track_number, @disc, @file_path)";
//                cmd.Prepare();
//                try { cmd.Parameters.AddWithValue("@title", songInfo.Tag.Title.Trim()); }
//                catch (Exception e) { cmd.Parameters.AddWithValue("@title", ""); };
//                try { cmd.Parameters.AddWithValue("@album_artist", songInfo.Tag.FirstAlbumArtist.Trim().Replace("\0", String.Empty)); }
//                catch (Exception e) { cmd.Parameters.AddWithValue("@album_artist", ""); };
//                try { cmd.Parameters.AddWithValue("@composer", songInfo.Tag.FirstComposer.Trim().Replace("\0", String.Empty)); }
//                catch (Exception e) { cmd.Parameters.AddWithValue("@composer", ""); };
//                try { cmd.Parameters.AddWithValue("@album", songInfo.Tag.Album.Trim()); }
//                catch (Exception e) { cmd.Parameters.AddWithValue("@album", ""); };
//                try { cmd.Parameters.AddWithValue("@genre", songInfo.Tag.FirstGenre.Trim().Replace("\0", String.Empty)); }
//                catch (Exception e) { cmd.Parameters.AddWithValue("@genre", ""); };
//                cmd.Parameters.AddWithValue("@year", songInfo.Tag.Year);
//                cmd.Parameters.AddWithValue("@track_number", songInfo.Tag.Track);
//                cmd.Parameters.AddWithValue("@disc", songInfo.Tag.Disc);
//                cmd.Parameters.AddWithValue("@file_path", song);
//                cmd.ExecuteNonQuery();
//            }
//            Console.WriteLine("Processed " + song);
//        }
//        catch (Exception e)
//        {
//            errors.Add(song + ": " + e.Message);
//        }
//    }

//    Console.WriteLine("\nThe following files had errors:");
//    foreach (string error in errors)
//        Console.WriteLine(error);

//    dbConnection.Close();
//}

