using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DocDbUnitTests
{
	[TestClass]
	public class MainTests
	{
		[TestMethod]
		public void TestFullTypeNameGenerator()
		{
			var resA = DocDb.DocumentWrapHelper.GetTypeNameWithBaseTypes(typeof(A));
			Assert.AreEqual(resA, "A");

			var resB = DocDb.DocumentWrapHelper.GetTypeNameWithBaseTypes(typeof(B));
			Assert.AreEqual(resB, "A::B");

			var resC = DocDb.DocumentWrapHelper.GetTypeNameWithBaseTypes(typeof(C));
			Assert.AreEqual(resC, "A::B::C");

		}
	}


}
