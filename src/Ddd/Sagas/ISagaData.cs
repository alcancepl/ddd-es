using Ddd.Commands;
using System.Threading.Tasks;

namespace Ddd.Sagas
{
    public interface ISagaData
    {
        ICommand ProducedCommand { get; set; }

    }

    public class SagaDataBase : ISagaData
    {
        //[JsonIgnore]
        public ICommand ProducedCommand { get; set; }

        //[JsonProperty("ProducedCommand")]
        //public KeyValuePair<Type, string> ProducedCommandStr
        //{
        //	get
        //	{
        //		return this.ProducedCommand != null ?
        //		  new KeyValuePair<Type, string>(this.ProducedCommand != null ? this.ProducedCommand.GetType() : null, JsonConvert.SerializeObject(this.ProducedCommand)) :
        //		  new KeyValuePair<Type, string>(null, "");
        //	}
        //	set {
        //		if (value.Key != null)
        //		{
        //			//Type t = Type.GetType(value.Key);
        //			this.ProducedCommand = (ICommand)JsonConvert.DeserializeObject(value.Value, value.Key);
        //		}
        //		else
        //			this.ProducedCommand = null;
        //	}
        //}

    }

    /// <summary>
    /// Special data for transient saga.
    /// </summary>
    public sealed class TransientSagaData : SagaDataBase
    {
    }

    public class TransientSagaFinder<TMessage> : ISagaByMessageFinder<TransientSagaData, TMessage> where TMessage : class, IMessage
    {
        public Task<SagaFindResult<TransientSagaData>> FindAsync(TMessage message)
        {
            return Task.FromResult(SagaFindResult<TransientSagaData>.NotFound());
        }
    }

    public class TransientSagaSaver : ISagaSaver<TransientSagaData>
    {
        public Task CompleteAsync(TransientSagaData sagaData)
        {
            return Task.CompletedTask;
        }

        public Task SaveAsync(TransientSagaData sagaData)
        {
            return Task.CompletedTask;
        }
    }
}
