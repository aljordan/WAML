using System;

namespace WhisperingAudioMusicLibrary
{
    public class StatusChangedEventArgs : EventArgs
    {
        private string msg;


        public StatusChangedEventArgs(string status)
        {
            msg = status;
        }

        public string Message
        {
            get { return msg; }
        }
    }
}

