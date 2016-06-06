using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rcs.Eurad.Domain.Aggregates;
using Ddd.Events;
using Rcs.Eurad.Contracts.Types;
using Ddd.Services;

namespace Rcs.Eurad.Azure.Tests
{
    // MOVE THIS TO THE DddAzureTests.csproj

    [TestClass()]
    public class StreamstoneEventStoreTests
    {
        const string connString = "UseDevelopmentStorage=true";
        //const string connString = "DefaultEndpointsProtocol=https;AccountName=euradsbwebjob;AccountKey=YCqrgh+vdCdoBI/VkYwJcLOqPdumujBGrtMucVeAEbhYLINQ4aU8YLKRyXKbKjolPKDZHmmno52JOwEn1Cws4w==";

        [TestMethod()]
        public async Task GetEventsFromSpecifiedVersion()
        {
            //var context = new Ddd.Commands.CommandContext();
            var store = new StreamstoneEventStore(null, connString);

            var idA = new UserAccountId(Guid.NewGuid());
            var e1A = new Contracts.Events.UserAccountPasswordChanged(idA, new HashedPassword("new-password-1"));
            var e2A = new Contracts.Events.UserAccountPasswordChanged(idA, new HashedPassword("new-password-2"));
            var e3A = new Contracts.Events.UserAccountPasswordChanged(idA, new HashedPassword("new-password-3"));
            var e4A = new Contracts.Events.UserAccountPasswordChanged(idA, new HashedPassword("new-password-4"));
            var idB = new UserAccountId(Guid.NewGuid());
            var e1B = new Contracts.Events.UserAccountPasswordChanged(idB, new HashedPassword("new-password-5"));
            var e2B = new Contracts.Events.UserAccountPasswordChanged(idB, new HashedPassword("new-password-6"));
            var e3B = new Contracts.Events.UserAccountPasswordChanged(idB, new HashedPassword("new-password-7"));
            var e4B = new Contracts.Events.UserAccountPasswordChanged(idB, new HashedPassword("new-password-8"));
            var idC = new UserAccountId(Guid.NewGuid());
            var e1C = new Contracts.Events.UserAccountPasswordChanged(idC, new HashedPassword("new-password-9"));
            var e2C = new Contracts.Events.UserAccountPasswordChanged(idC, new HashedPassword("new-password-10"));
            var e3C = new Contracts.Events.UserAccountPasswordChanged(idC, new HashedPassword("new-password-11"));
            var e4C = new Contracts.Events.UserAccountPasswordChanged(idC, new HashedPassword("new-password-12"));

            await store.SaveAsync(typeof(CompanyAccount), idA, new IEvent[] { e1A, e2A, e3A, e4A });
            await store.SaveAsync(typeof(CompanyAccount), idB, new IEvent[] { e1B, e2B, e3B, e4B });
            await store.SaveAsync(typeof(UserAccount), idC, new IEvent[] { e1C, e2C, e3C, e4C });

            var eventsA = await store.GetAsync(typeof(CompanyAccount), idA, 2); // should return 3 events
            var eventsB = await store.GetAsync(typeof(CompanyAccount), idB, 4); // should return 1 events
            var eventsC = await store.GetAsync(typeof(UserAccount), idC, 10); // should return 0 events
            var noEvents = await store.GetAsync(typeof(UserAccount), new UserAccountId(Guid.NewGuid())); // should return 0 events

            Assert.AreEqual(3, eventsA.Count);
            Assert.AreEqual(1, eventsB.Count);
            Assert.AreEqual(0, eventsC.Count);
            Assert.AreEqual(0, noEvents.Count);

            Assert.AreEqual(e2A, eventsA.ElementAt(0));
            Assert.AreEqual(e3A, eventsA.ElementAt(1));
            Assert.AreEqual(e4A, eventsA.ElementAt(2));

            Assert.AreEqual(e4B, eventsB.ElementAt(0));
        }

        [TestMethod()]
        public async Task StoresAndGetsMultipleAREvents()
        {
            //var context = new Ddd.Commands.CommandContext();
            var store = new StreamstoneEventStore(null, connString);

            var idA = new UserAccountId(Guid.NewGuid());
            var e1A = new Contracts.Events.UserAccountPasswordChanged(idA, new HashedPassword("new-password-1"));
            var e2A = new Contracts.Events.UserAccountPasswordChanged(idA, new HashedPassword("new-password-2"));
            var e3A = new Contracts.Events.UserAccountPasswordChanged(idA, new HashedPassword("new-password-3"));
            var e4A = new Contracts.Events.UserAccountPasswordChanged(idA, new HashedPassword("new-password-4"));
            var idB = new UserAccountId(Guid.NewGuid());
            var e1B = new Contracts.Events.UserAccountPasswordChanged(idB, new HashedPassword("new-password-1"));
            var e2B = new Contracts.Events.UserAccountPasswordChanged(idB, new HashedPassword("new-password-2"));
            var e3B = new Contracts.Events.UserAccountPasswordChanged(idB, new HashedPassword("new-password-3"));
            var e4B = new Contracts.Events.UserAccountPasswordChanged(idB, new HashedPassword("new-password-4"));
            var idC = new UserAccountId(Guid.NewGuid());
            var e1C = new Contracts.Events.UserAccountPasswordChanged(idC, new HashedPassword("new-password-1"));
            var e2C = new Contracts.Events.UserAccountPasswordChanged(idC, new HashedPassword("new-password-2"));
            var e3C = new Contracts.Events.UserAccountPasswordChanged(idC, new HashedPassword("new-password-3"));
            var e4C = new Contracts.Events.UserAccountPasswordChanged(idC, new HashedPassword("new-password-4"));

            await store.SaveAsync(typeof(CompanyAccount), idA, new IEvent[] { e1A, e2A, e3A, e4A });
            await store.SaveAsync(typeof(CompanyAccount), idB, new IEvent[] { e1B, e2B, e3B, e4B });
            await store.SaveAsync(typeof(UserAccount), idC, new IEvent[] { e1C, e2C, e3C, e4C });

            var allEventsA = await store.GetAsync(typeof(CompanyAccount), idA);
            var allEventsB = await store.GetAsync(typeof(CompanyAccount), idB);
            var allEventsC = await store.GetAsync(typeof(UserAccount), idC);

            Assert.AreEqual(4, allEventsA.Count);
            Assert.AreEqual(4, allEventsB.Count);
            Assert.AreEqual(4, allEventsC.Count);

            Assert.AreEqual(e1A, allEventsA.ElementAt(0));
            Assert.AreEqual(e2A, allEventsA.ElementAt(1));
            Assert.AreEqual(e3A, allEventsA.ElementAt(2));
            Assert.AreEqual(e4A, allEventsA.ElementAt(3));

            Assert.AreEqual(e1B, allEventsB.ElementAt(0));
            Assert.AreEqual(e2B, allEventsB.ElementAt(1));
            Assert.AreEqual(e3B, allEventsB.ElementAt(2));
            Assert.AreEqual(e4B, allEventsB.ElementAt(3));

            Assert.AreEqual(e1C, allEventsC.ElementAt(0));
            Assert.AreEqual(e2C, allEventsC.ElementAt(1));
            Assert.AreEqual(e3C, allEventsC.ElementAt(2));
            Assert.AreEqual(e4C, allEventsC.ElementAt(3));
        }

        [TestMethod()]
        public async Task SaveAsyncThrowsConcurencyExceptionWhenTheStreamHasChangedAfterTheLastRead()
        {
            //var context = new Ddd.Commands.CommandContext();
            var storeA = new StreamstoneEventStore(null, connString);
            var storeB = new StreamstoneEventStore(null, connString);

            var id = new UserAccountId(Guid.NewGuid());
            var e1 = new Contracts.Events.UserAccountPasswordChanged(id, new HashedPassword("new-password"));
            var e2 = new Contracts.Events.UserAccountCreated(id, "userlogin", "mail@localhost", "Full Name", "pl");

            //await storeB.GetAsync(typeof(CompanyAccount), id);
            await storeA.SaveAsync(typeof(CompanyAccount), id, new[] { e1 });
            try
            {
                await storeB.SaveAsync(typeof(CompanyAccount), id, new[] { e2 });
                Assert.Fail("Should throw ConcurrencyException!");
            }
            catch (Exception ex)
            {
                // expect this                
                Assert.IsInstanceOfType(ex, typeof(Ddd.Domain.Exceptions.ConcurrencyException));
            }
        }

        [TestMethod()]
        public async Task DistinctStoreInstancesCanHandleConcurrency()
        {
            //var context = new Ddd.Commands.CommandContext();            
            var storeA = new StreamstoneEventStore(null, connString);
            var storeB = new StreamstoneEventStore(null, connString);

            var idA = new UserAccountId(Guid.NewGuid());
            var e1 = new Contracts.Events.UserAccountPasswordChanged(idA, new HashedPassword("pass1"));
            var e2 = new Contracts.Events.UserAccountCreated(idA, "userlogin", "mail@localhost", "Full Name", "pl");

            var idB = new UserAccountId(Guid.NewGuid());
            var e3 = new Contracts.Events.UserAccountPasswordChanged(idB, new HashedPassword("pass2"));

            await storeA.SaveAsync(typeof(CompanyAccount), idA, new[] { e1 });
            await storeB.SaveAsync(typeof(CompanyAccount), idB, new[] { e3 });
            await storeA.SaveAsync(typeof(CompanyAccount), idA, new[] { e2 });
        }
    }
}