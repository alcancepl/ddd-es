using Ddd;
using Ddd.Commands;
using Ddd.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DddTest
{
    public abstract class DomainTest
    {
        private Dictionary<Guid, IEnumerable<IEvent>> preConditions = new Dictionary<Guid, IEnumerable<IEvent>>();
        private List<ICommand> prevCommands = new List<ICommand>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="command">a command under test</param>
        /// <param name="preConditions">initial events in the event store</param>
        /// <param name="prevCommands">commands to be executed before the command under test</param>
        /// <returns>events produced by the command under test</returns>
        protected virtual IList<IEvent> ExecuteCommand<TCommand>(TCommand command, Dictionary<Guid, IEnumerable<IEvent>> preConditions, List<ICommand> prevCommands) where TCommand : class, ICommand
        {
            // preConditions come from the Given part;
            var applicationUnderTest = new ApplicationUnderTest(preConditions);

            // prevCommands come from the Given part;
            foreach (var prevCommand in prevCommands)
            {
                try
                {
                    applicationUnderTest.ProcessCommand(prevCommand);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format("Precondition command failed - see innerException for details. Cannot execute {0}", prevCommand), ex);
                }
            }

            applicationUnderTest.ProcessCommand(command);

            return applicationUnderTest.GetLastEvents();
        }

        internal IList<IEvent> CommandEvents { get; private set; }

        // Use TestCleanup to run code after each test has run        
        [TestCleanup]
        public void DomainTestCleanup()
        {
        }

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void DomainTestInitialize()
        {
            CommandEvents = new List<IEvent>();
            IdGenerator.GenerateGuid = null;
            preConditions = new Dictionary<Guid, IEnumerable<IEvent>>(0);
            prevCommands = new List<ICommand>();
        }

        protected void When<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            CommandEvents = ExecuteCommand(command, preConditions, prevCommands);            
        }

        protected void Then(params IEvent[] expectedEvents)
        {            
            var expectedEventsList = expectedEvents.ToList();
            Assert.AreEqual(expectedEventsList.Count, CommandEvents.Count);

            for (int i = 0; i < CommandEvents.Count; i++)
            {
                Assert.AreEqual(expectedEvents[i], CommandEvents[i]);
            }
        }

        protected void WhenThrows<TCommand, TException>(TCommand command)
            where TException : Exception
            where TCommand : class, ICommand
        {
            WhenThrows<TCommand, TException>(command, (c, ex) => true);
        }

        protected void WhenThrows<TCommand, TException>(TCommand command, Func<TCommand, TException, bool> isExpectedException)
            where TException : Exception
            where TCommand : class, ICommand
        {
            try
            {
                When<TCommand>(command);
                Assert.Fail("Expected exception " + typeof(TException));
            }
            catch (AggregateException ex)
            {
                ex.Handle(innerEx => innerEx is TException && isExpectedException(command, (TException)innerEx));
            }
        }

        protected void Given(params IEvent[] existingEvents)
        {
            preConditions = existingEvents
                .GroupBy(y => y.Id)
                .ToDictionary(y => y.Key, y => y.AsEnumerable());
        }

        protected void Given(params ICommand[] prevCommands)
        {
            this.prevCommands = prevCommands.ToList();
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion


    }
}
