using Ddd.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd
{
    /// <summary>
    /// Provides human-readable numbers for aggregates.
    /// Ensures uniquiness of the generated numbers in the specified context.
    /// </summary>
    public interface IAutoNrService
    {
        Task<string> GetAutoNr<TAggregate, TAutoNrConfig>(TAggregate aggregate, string uniquinessContext, TAutoNrConfig config, 
                Func<TAggregate, TAutoNrConfig, AutoNrResult<TAutoNrConfig>> nrGenerator)
            where TAggregate : IAggregate
            where TAutoNrConfig : class, IAutoNrConfig;
    }    

    public class AutoNrResult<TAutoNrConfig>
    {       

        public AutoNrResult(string nr, TAutoNrConfig config)
        {
            Nr = nr;
            Config = config;
        }

        public string Nr { get; private set; }
        public TAutoNrConfig Config { get; private set; }
    }

    public interface IAutoNrConfig
    {
        ulong? LastNr { get; }
    }
}
