using System;

namespace Ddd.Services
{
    public class DuplicateDocumentNrException : Exception
    {
        private readonly long nr;
        private readonly string sequence;        

        public DuplicateDocumentNrException(string sequence, long nr)
            :base($"Duplicate document nr {nr} in sequence {sequence}. There is another document with the same nr in this sequence of nrs.")
        {            
            this.sequence = sequence;
            this.nr = nr;
        }       
    }
}