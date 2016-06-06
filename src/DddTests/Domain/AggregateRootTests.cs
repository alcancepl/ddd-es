using Ddd.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ddd.Domain.Tests
{
    [TestClass()]
    public class AggregateRootTests
    {
        [TestMethod()]        
        public void ApplyEventThrowsExceptionForMissingTransitionsTest()
        {
            var ar = new TestAggregate();
            var e = new TestEvent(new TestId("5ad093ef-15b9-4cce-9e20-a75772972061"));
            try
            {
                ar.ApplyEvent(e);
                Assert.Fail("Aggregate root should throw AggregateException for missing transitions.");
            }
            catch(AggregateException)
            {
                // Expect this exception                
            }            
        }

        public class TestId : IAggregateIdentity
        {
            public TestId(string value)
            {
                Value = value;
            }
            public string Value { get; private set; }
        }

        public class TestAggregate : AggregateRoot<TestId>
        {
            public TestAggregate()
            {
                Id = new TestId(Guid.NewGuid().ToString("N"));
            }            
        }

        public class TestEvent : IEvent
        {
            public TestEvent(IAggregateIdentity id)
            {
                Id = id;
            }
            public IAggregateIdentity Id { get; private set; }
        }
    }
}