using Ddd;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DddTest
{
    public class TestAutoNrService : IAutoNrService
    {
        static TestAutoNrService()
        {
            contexts = new ConcurrentDictionary<string, AutoNrContext>();
        }

        private static ConcurrentDictionary<string, AutoNrContext> contexts;


        private class AutoNrContext
        {
            private readonly Dictionary<string, Guid> generatedNrs;
            private readonly Dictionary<Guid, string> generatedIds;

            private AutoNrContext()
            {
                generatedNrs = new Dictionary<string, Guid>();
                generatedIds = new Dictionary<Guid, string>();
            }

            public static AutoNrContext Create<TAutoNrConfig>(TAutoNrConfig config) where TAutoNrConfig : class, IAutoNrConfig, new()
            {
                var ctx = new AutoNrContext();
                ctx.SetConfig(config);
                return ctx;
            }

            public void AddNr(Guid aggregateId, string nr)
            {
                generatedIds.Add(aggregateId, nr);
                try
                {
                    generatedNrs.Add(nr, aggregateId);
                }
                catch
                {
                    generatedIds.Remove(aggregateId);
                    throw;
                }
            }

            public bool TryGet(Guid aggregateId, out string nr)
            {
                return generatedIds.TryGetValue(aggregateId, out nr);
            }

            private string configData;

            internal TAutoNrConfig GetConfig<TAutoNrConfig>() where TAutoNrConfig : class, IAutoNrConfig
            {
                return JsonConvert.DeserializeObject<TAutoNrConfig>(configData);
            }

            internal void SetConfig<TAutoNrConfig>(TAutoNrConfig config) where TAutoNrConfig : class, IAutoNrConfig
            {
                this.configData = JsonConvert.SerializeObject(config);
            }
        }

        Task<string> IAutoNrService.GetAutoNr<TAggregate, TAutoNrConfig>(TAggregate aggregate, string uniquinessContext, TAutoNrConfig config,
            Func<TAggregate, TAutoNrConfig, AutoNrResult<TAutoNrConfig>> nrGenerator)            
        {
            var context = contexts.GetOrAdd(uniquinessContext, (_uniquinessContext) => AutoNrContext.Create(config));
            lock(context) // only one thread per context
            {
                string generatedNr;
                if (context.TryGet(aggregate.Id, out generatedNr))
                    return Task.FromResult(generatedNr);
                else
                {
                    var result = nrGenerator(aggregate, context.GetConfig<TAutoNrConfig>());
                    context.AddNr(aggregate.Id, result.Nr);
                    context.SetConfig(result.Config);
                    return Task.FromResult(result.Nr);
                }
            }           
        }
    }


}
