using System;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//id integer primary key not null, title text, album_artist text, composer text, album text, genre text, 
//year integer, track_number integer, disc integer, file_path text not null
namespace WhisperingAudioMusicLibrary
{
    [Serializable()]
    public class Track
    {

        #region Private variables
        private long id;
        private string title;
        private string artist;
        private string composer;
        private string album;
        private string genre;
        private short trackNumber;
        private short year;
        private short discNumber;
        private string filePath;

        #endregion

        #region Class Constructors

        // parameterless constructor for serialization
        public Track()
        {
        }

        public Track(long id, string title, string artist, string composer, string album,
            string genre, short trackNumber, short year, short discNumber, string filePath)
        {
            this.id = id;
            this.title = title;
            this.artist = artist;
            this.composer = composer;
            this.album = album;
            this.genre = genre;
            this.trackNumber = trackNumber;
            this.year = year;
            this.discNumber = discNumber;
            this.filePath = filePath;
        }

        //TODO: author method to write tags back to database and file.

        #endregion

        #region Public Properties
        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Artist
        {
            get { return artist; }
            set { artist = value; }
        }

        public string Composer
        {
            get { return composer; }
            set { composer = value; }
        }

        public string Album
        {
            get { return album; }
            set { album = value; }
        }

        public string Genre
        {
            get { return genre; }
            set { genre = value; }
        }

        public short DiscNumber
        {
            get { return discNumber; }
            set { discNumber = value; }
        }

        public short Year
        {
            get { return year; }
            set { year = value; }
        }

        public short TrackNumber
        {
            get { return trackNumber; }
            set { trackNumber = value; }
        }

        public string FilePath
        {
            get { return filePath; }
            // we only want file path set from the constructor at this
            //set { filePath = value; }

            // had to disregard the above because it is needed for serialization
            set { filePath = value; }
        }

        public override string ToString()
        {
            return Title;
        }

        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Track t = obj as Track;
            if ((Object)t == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Title == t.Title) && (Artist == t.Artist)
                && (Album == t.Album) && (Genre == t.Genre);
        }

        #endregion
    }
}
