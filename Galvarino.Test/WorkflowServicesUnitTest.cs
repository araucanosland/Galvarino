using System;
using NUnit.Framework;
namespace Galvarino.Test{

    [TestFixture]
    public class WorkflowServicesUnitTest{

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void TestUnService(int val)
        {
            bool isfalse = (1!=val);
            //Assert.IsFalse(condition: isfalse, message: "Not eq to val");
        }
    }
}