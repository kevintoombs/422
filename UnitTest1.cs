using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CS422;
using System.Threading;

namespace UnitTestProject1
{
    [TestClass]
    public class TestPCQueue
    {
        internal int _iterations = 10000;

        [TestMethod]
        public void TestQueue()
        {

        }

    }

    [TestClass]
    public class TestThreadPool
    {
        [TestMethod]
        public void TestMethod1()
        {
            //ThreadPoolSleepSorter tpss = new ThreadPoolSleepSorter(, 10);
            Assert.AreEqual(1, 1);
        }
    }
}

