using Ddd.Domain;
using System;
using System.Threading.Tasks;

namespace Ddd.Services
{
	public delegate AutoNrResult<TSequenceData, TNrData> AutoNrGenerator<TSequenceData, TNrData>(
		long sequenceNr,
		TSequenceData config, // or null
		TNrData prev) // or null        
			where TSequenceData : class
			where TNrData : class;

	public delegate AutoNrResult<TSequenceData, TNrData> AutoNrUpdater<TSequenceData, TNrData>(
		TNrData current,
		TSequenceData config, // or null        
		TNrData prev, // or null
		TNrData next) // or null
			where TSequenceData : class
			where TNrData : class;

	/// <summary>
	/// Provides human-readable numbers for aggregates.
	/// Ensures uniquiness of the generated numbers in the specified uniqueness context.
	/// </summary>
	public interface IAutoNrService
	{
		/// <summary>
		/// 
		/// </summary>        
		/// <typeparam name="TSequenceData"></typeparam>        
		/// <typeparam name="TNrData"></typeparam>        
		/// <param name="sequence"></param>
		/// <param name="aggregateId"></param>
		/// <param name="generator"></param>
		/// <returns></returns>
		Task<TNrData> GetAutoNr<TSequenceData, TNrData>(
			string sequence,            
			string aggregateId,            
			AutoNrGenerator<TSequenceData, TNrData> generator)
				where TSequenceData : class
				where TNrData : class;

		Task<TNrData> UpdateAutoNr<TSequenceData, TNrData>(
			string sequence,
			string aggregateId,
			AutoNrUpdater<TSequenceData, TNrData> updater)
				where TSequenceData : class
				where TNrData : class;

		Task SetLastNr<TSequenceData, TNrData>(string sequence, long lastNr)
			where TSequenceData : class
			where TNrData : class;

	}

	/// <summary>
	/// Represents auto nr generator result.
	/// </summary>
	/// <typeparam name="TSequenceData">the type of the configuration used by the generator</typeparam>
	/// <typeparam name="TNrData"></typeparam>
	public class AutoNrResult<TSequenceData, TNrData>
	{
		public AutoNrResult(TSequenceData sequenceData, TNrData nrData)
		{
			NrData = nrData;
			SequenceData = sequenceData;            
		}        

		/// <summary>
		/// The updated config.
		/// </summary>
		public TSequenceData SequenceData { get; private set; }

		/// <summary>
		/// Additional data, required by the nr-checker.
		/// </summary>
		public TNrData NrData { get; private set; }
	}    
}
