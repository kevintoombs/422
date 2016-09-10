using NUnit.Framework;
using System;
using System.Text;
using System.IO;
using System.Threading;
using CS422;

namespace sleeptest
{
	[TestFixture ()]
	public class Test
	{
		[Test ()]
		public void TestCase ()
		{

			var writer = new StringWriter ();
					// act
					//sut.SomeMethod(writer);
					CS422.ThreadPoolSleepSorter tpss = new ThreadPoolSleepSorter (writer, 20);
					byte[] bytes = new byte[3];
					bytes [0] = 3;
					bytes [1] = 7;
					bytes [2] = 1;

					tpss.Sort (bytes);

					Thread.Sleep (8000);
					// assert
					string actual = writer.ToString();
					Assert.AreEqual ("1\n3\n7\n", actual);


		}
	}
}

