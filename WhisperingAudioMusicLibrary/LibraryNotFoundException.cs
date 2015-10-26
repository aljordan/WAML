using System;
namespace WhisperingAudioMusicLibrary
{
    public class LibraryNotFoundException : Exception
    {
        public LibraryNotFoundException()
        {
        }

        public LibraryNotFoundException(string message)
            : base(message)
        {
        }

        public LibraryNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}