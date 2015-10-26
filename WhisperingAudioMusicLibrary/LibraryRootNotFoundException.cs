using System;
namespace WhisperingAudioMusicLibrary
{
    public class LibraryRootNotFoundException : Exception
    {
        public LibraryRootNotFoundException()
        {
        }

        public LibraryRootNotFoundException(string message)
            : base(message)
        {
        }

        public LibraryRootNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}