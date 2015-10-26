namespace WhisperingAudioMusicLibrary
{
    internal static class DatabaseCommands
    {
        public const string CreateTrackTable = @"CREATE TABLE track(id integer primary key not null, title text, album_artist text, composer text, album text, genre text, year integer, track_number integer, disc integer, file_path text not null);";
        public const string CreateTrackTitleIndex = @"CREATE INDEX track_title_idx ON track (title);";
        public const string CreateTrackAlbumIndex = @"CREATE INDEX track_album_idx ON track (album);";
        public const string CreateTrackGenreIndex = @"CREATE INDEX track_genre_idx ON track (genre);";
        public const string CreateLibraryRootTable = @"CREATE TABLE library_root(id integer primary key not null, directory_path text not null);";
    }
}