using System;

namespace Ddd.Services
{
    public class AggregateNotFoundForAutoNrSeq : Exception
    {
        private readonly ulong seqNr;
        private readonly string uniquenessContext;
        private readonly Type aggregateType;

        public AggregateNotFoundForAutoNrSeq(string uniquenessContext, ulong seqNr, Type aggregateType)
            :base($"AggregateId {aggregateType} in uniqueness context {uniquenessContext} with sequance nr {seqNr} has not been found.")
        {            
            this.uniquenessContext = uniquenessContext;
            this.seqNr = seqNr;
			this.aggregateType = aggregateType;
		}       
    }
}