using System;

namespace MediaInTheCloud
{
    public class AudioFormatChangedException : Exception
    {
        public AudioFormatChangedException(string message) : base(message) { }
    }
}
