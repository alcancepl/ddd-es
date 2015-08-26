using System;
using System.Collections.Generic;
using Ddd.Commands;
using Ddd.Events;
using SimpleInjector;
using SimpleInjector.Extensions;
using System.Threading.Tasks;
using Ddd.Domain;
using Ddd;
using System.Linq;

namespace DddTest
{
    public class ApplicationUnderTest
    {
        private readonly TestEventStore theStore;
        private readonly TestCommandProcessor commandProcessor;
        private readonly Container container;

        internal T GetInstance<T>() where T : class
        {
            return container.GetInstance<T>();
        }

        public ApplicationUnderTest(Dictionary<Guid, IEnumerable<IEvent>> preConditions = null)
        {
            this.container = new Container();
            this.CommandExecutionTimeout = TimeSpan.FromSeconds(30);
            this.theStore = new TestEventStore(preConditions);
            this.commandProcessor = new TestCommandProcessor(container.GetInstance);

            try
            {
                CompositionRoot();
                container.Verify();
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    System.Diagnostics.Trace.TraceError(loaderException.ToString());
                }
                throw;
            }
        }

        //static List<System.Reflection.Assembly> rcsAssemblies;

        //static ApplicationUnderTest()
        //{
        //    var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

        //    loadedAssemblies
        //        .SelectMany(x => x.GetReferencedAssemblies().Where(a => a.FullName.StartsWith("Rcs.", StringComparison.InvariantCultureIgnoreCase)))
        //        .Distinct()
        //        .Where(y => loadedAssemblies.Any((a) => a.FullName == y.FullName) == false)
        //        .ToList()
        //        .ForEach(x => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(x)));

        //    rcsAssemblies = loadedAssemblies.Where(a => a.FullName.StartsWith("Rcs.", StringComparison.InvariantCultureIgnoreCase)).ToList();
        //}

        protected virtual void CompositionRoot()
        {
            // Register command handlers
            container.Register(typeof(ICommandHandler<>), AppDomain.CurrentDomain.GetAssemblies());

            // Register services
            container.RegisterSingleton<IEventStore>(theStore);
            container.RegisterSingleton<IRepository, Repository>();
            container.RegisterSingleton<IUniqueService, TestUniqueService>();
        }

        public void ProcessCommand<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            var task = commandProcessor.ProcessAsync(command);

            var commandTaskCompleted = task.Wait(CommandExecutionTimeout);
            if (!commandTaskCompleted || (task.Status != TaskStatus.RanToCompletion))
                throw new TimeoutException(string.Format("Command {0} timed out.", command.GetType().FullName));
        }

        public TimeSpan CommandExecutionTimeout { get; set; }

        public IEventStore EventStore { get { return theStore; } }
        public List<IEvent> GetLastEvents()
        {
            return theStore.GetLastEvents();
        }

        internal void Clear()
        {
            theStore.Clear();
        }
    }
}