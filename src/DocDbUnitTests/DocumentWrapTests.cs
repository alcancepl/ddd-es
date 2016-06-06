using DocDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DocDbUnitTests
{
    [TestClass]
    public class DocumentWrapTests
    {
        [TestMethod]
        public void TypeInfoIsWhatWeExpectIttoBe()
        {
            Assert.AreEqual("A", DocumentWrap<A>.TypeInfo);
            Assert.AreEqual("A|B", DocumentWrap<B>.TypeInfo);
            Assert.AreEqual("A|B|C", DocumentWrap<C>.TypeInfo);
        }

        [TestMethod]
        public void TypeInfoIsWhatWeExpectIttoBeEvenForGenerics()
        {
            Assert.AreEqual("System.Collections.Generic.List`1[DocDbUnitTests.A]", DocumentWrap<List<A>>.TypeInfo);
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[DocDbUnitTests.B]", DocumentWrap<IEnumerable<B>>.TypeInfo);
        }

        [TestMethod]
        public void TypeInfoIsWhatWeExpectIttoBeEvenForArrays()
        {
            Assert.AreEqual("System.Array|A[]", DocumentWrap<A[]>.TypeInfo);
            Assert.AreEqual("System.Array|B[]", DocumentWrap<B[]>.TypeInfo);
        }

        [TestMethod]
        public void BuildIdWorksAsExpected()
        {
            var guid1 = new Guid("23c9125d-24ba-4c1b-b740-5b765437c30e");
            var guid2 = new Guid("f048be07-3cbe-4345-a1e8-f528738adf1d");
            Assert.AreEqual("A|SomeId", DocumentWrap<A>.BuildId("SomeId"));
            Assert.AreEqual("A|B|SomeId", DocumentWrap<B>.BuildId("SomeId"));
            Assert.AreEqual("A|B|C|SomeId", DocumentWrap<C>.BuildId("SomeId"));
            Assert.AreEqual("A|B|C|SomeId|AnotherPartOfTheId", DocumentWrap<C>.BuildId("SomeId", "AnotherPartOfTheId"));
            Assert.AreEqual("A|B|C|23c9125d24ba4c1bb7405b765437c30e|f048be073cbe4345a1e8f528738adf1d", DocumentWrap<C>.BuildId(guid1, guid2));
        }
    }


}
