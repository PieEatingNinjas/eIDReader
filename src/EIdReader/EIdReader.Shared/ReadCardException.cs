using System;

namespace PieEatingNinjas.EIdReader.Shared
{
    public class ReadCardException : ApplicationException
    {
        public ReadCardException() : base("Unable to read the inserted card!")
        {

        }
    }
}
