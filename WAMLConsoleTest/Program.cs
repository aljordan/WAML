using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading.Tasks;
using WhisperingAudioMusicLibrary;

namespace WAMLConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MusicLibrary library;
            Console.WriteLine("Listing available libraries:");
            List<string> libraries = MusicLibrary.GetAvailableLibraries();
            foreach (string lib in libraries)
                Console.WriteLine(lib);
            Console.WriteLine("Finished listing available libraries. Press Enter to continue");
            Console.ReadLine();

            if (libraries.Count > 0)
            {
                Console.WriteLine("Opening first available library:");
                library = MusicLibrary.OpenLibrary(libraries[0]);
            } 
            else 
            {
                library = new MusicLibrary("Small Library", @"C:\Users\aljordan\Music");
            }

            //library.CreateLibrary();
            //Console.WriteLine("Finished processing.  Press Enter to continue.");
            //Console.ReadLine();

            Console.WriteLine("Listing all genres:");
            List<string> genres = library.GetGenres();
            foreach (string s in genres)
                Console.WriteLine(s);
            
            Console.WriteLine("\nPress return to continue");
            Console.ReadLine();

            Console.WriteLine("Listing genres filtered by the word rock:");
            List<string> filteredGenres = library.GetGenres("rock");
            foreach (string s in filteredGenres)
                Console.WriteLine(s);

            Console.WriteLine("\nPress return to continue");
            Console.ReadLine();

            Console.WriteLine("List all artists:");
            List<string> artists = library.GetArtists();
            foreach (string s in artists)
                Console.WriteLine(s);

            Console.WriteLine("\nPress return to continue");
            Console.ReadLine();

            Console.WriteLine("Listing artists filtered by the word Dave:");
            List<string> filteredArtists = library.GetArtists("Dave");
            foreach (string s in filteredArtists)
                Console.WriteLine(s);

            Console.WriteLine("\nPress return to continue");
            Console.ReadLine();

            Console.WriteLine("Listing all albums:");
            List<string> albums = library.GetAlbums();
            foreach (string s in albums)
                Console.WriteLine(s);

            Console.WriteLine("\nPress return to continue");
            Console.ReadLine();

            Console.WriteLine("Listing albums filtered by the word rock:");
            List<string> filteredAlbums = library.GetAlbums("rock");
            foreach (string s in filteredAlbums)
                Console.WriteLine(s);

            Console.WriteLine("\nPress return to continue");
            Console.ReadLine();

            Console.WriteLine("Listing all song titles:");
            List<string> songs = library.GetSongTitles();
            foreach (string s in songs)
                Console.WriteLine(s);

            Console.WriteLine("\nPress return to continue");
            Console.ReadLine();

            Console.WriteLine("Listing song titles filtered by the word rock:");
            List<string> filteredSongs = library.GetSongTitles("rock");
            foreach (string s in filteredSongs)
                Console.WriteLine(s);


            Console.WriteLine("\nPress return to continue");
            Console.ReadLine();

            Console.WriteLine("Listing all track objects:");
            List<Track> tracks = library.GetSongs();
            foreach (Track t in tracks)
                Console.WriteLine(t.Title);

            Console.WriteLine("\nPress return to continue");
            Console.ReadLine();

            Console.WriteLine("Listing track objects filtered by the word rock:");
            List<Track> filteredTracks = library.GetSongs("rock");
            foreach (Track t in filteredTracks)
                Console.WriteLine(t.Title);

            Console.WriteLine("\nPress return to quit");
            Console.ReadLine();
        }
    }
}
