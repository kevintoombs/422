using System;
using System.IO;

namespace CS422
{
	class MainClass
	{
        static bool showPasses = false;
        static bool fail = false;

		public static void otherMain (string[] args)
		{
            Console.WriteLine("Testing CS");

            Console.WriteLine("Combining two memory streams");

            string convert1 = "abcdqwertyujhgfd2345";
            string convert2 = "123345678iuyhgfrr545";
            string both = convert1 + convert2;
            byte[] buffer1 = System.Text.Encoding.UTF8.GetBytes(convert1);
            MemoryStream ms1 = new MemoryStream (buffer1);
			byte[] buffer2 = System.Text.Encoding.UTF8.GetBytes(convert2);
            MemoryStream ms2 = new MemoryStream (buffer2);


			ConcatStream cs = new ConcatStream (ms1,ms2);

            byte[] buffer3 = new byte[100000];
            cs.Read(buffer3, 0, 9);
            var s = System.Text.Encoding.UTF8.GetString(buffer3, 0, 9);
            Expect(both.Substring(0,9), s, "read 1");

            Console.WriteLine("Checking length of concat stream");
            cs.Seek (0, SeekOrigin.End);
            Expect(ms1.Length, ms1.Position, "stream 1 seek");
            Expect(ms2.Length, ms2.Position, "stream 2 seek");
            Expect(cs.Length, cs.Position, "cs seek");
            cs.Seek(0, SeekOrigin.Begin);
            Expect(0, ms1.Position, "stream 1 seek back");
            Expect(0, ms2.Position, "stream 2 seek back");
            Expect(0, cs.Position, "cs seek back");

            cs.Read(buffer3, 0, 9);
            s = System.Text.Encoding.UTF8.GetString(buffer3, 0, 9);
            Expect(both.Substring(0,9), s, "read 2");
            cs.Seek(0, SeekOrigin.Begin);

            cs.Read(buffer3, 0, 4);
            cs.Read(buffer3, 4, 5);
            s = System.Text.Encoding.UTF8.GetString(buffer3, 0, 4);
            string t = System.Text.Encoding.UTF8.GetString(buffer3, 4, 5);
            Expect(both.Substring(0,4), s, "two part read a");
            Expect(both.Substring(4,5), t, "two part read b");
            cs.Seek(0, SeekOrigin.Begin);

            Console.WriteLine("Reading random chunks");

            Random rnd = new Random();
            for (int i = 0; i<50; i++)
            {
                int start = rnd.Next(0, Convert.ToInt32(cs.Length-4)); // creates a number between 1 and 12
                int count = rnd.Next(1, Convert.ToInt32(cs.Length) - start);
                cs.Seek(start, SeekOrigin.Begin);
                int readBytes = cs.Read(buffer3, 0, count);
                s = System.Text.Encoding.UTF8.GetString(buffer3, 0, count);
                t = both.Substring(start, count);
                string m = String.Format("Start: {0}, Count:{1}, i:{2}, r:{3}", start, count, i, readBytes);
                Expect(t, s, m);
                cs.Seek(0, SeekOrigin.Begin);
            }

            TestNSMS();

            if (fail) { }
            else Console.WriteLine("No tests failed.");

            Console.ReadLine();

        }

        public static void TestNSMS()
        {

            Console.WriteLine("Testing NSMS");

            Console.WriteLine("Making concat stream with NSMS as stream 2");
        
            string convert1 = "abcd";
            string convert2 = "12345";
            byte[] buffer1 = System.Text.Encoding.UTF8.GetBytes(convert1);
            MemoryStream ms1 = new MemoryStream(buffer1);
            byte[] buffer2 = System.Text.Encoding.UTF8.GetBytes(convert2);
            MemoryStream ms2 = new NoSeekMemoryStream(buffer2);


            ConcatStream cs = new ConcatStream(ms1, ms2);

            Console.WriteLine("Verifying all data can be read");
            byte[] buffer3 = new byte[30];
            cs.Read(buffer3, 0, 20);
            var s = System.Text.Encoding.UTF8.GetString(buffer3, 0, 9);
            Expect("abcd12345", s, "read with nsms");

            Console.WriteLine("Checking for exception on Length property access");
            try
            {
                long l = cs.Length;
            }
            catch (Exception ex)
            {
                s = ex.Message;
            }
            Expect("Specified method is not supported.", s, "access length on cs with unseekable 2nd");

            Console.WriteLine("Checking NSMS seeking");

            Expect("False", ms2.CanSeek.ToString(), "can seek nsms");

            s = "";
            try
            {
                long l = ms2.Length;
            }
            catch (Exception ex )
            {
                s = ex.Message;
            }
            Expect("Specified method is not supported.", s, "NSMS.Length");

            s = "";
            try
            {
                ms2.SetLength(4);
            }
            catch (Exception ex)
            {
                s = ex.Message;
            }
            Expect("Specified method is not supported.", s, "NSMS.SetLength");

            s = "";
            try
            {
                long l = ms2.Position;
            }
            catch (Exception ex)
            {
                s = ex.Message;
            }
            Expect("Specified method is not supported.", s, "NSMS.Position");

            s = "";
            try
            {
                long l = ms2.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                s = ex.Message;
            }
            Expect("Specified method is not supported.", s, "NSMS.Seek");





        }

        public static void Expect(long i, long j, string more)
        {
            if (i != j)
            {
                Console.WriteLine("Expected {0} got {1}", i, j);
                Console.WriteLine(more);
            }
        }
        public static void Expect(string s, string t, string more)
        {
            if (!s.Equals(t))
            {
                Console.WriteLine("Failed: " + more);
                Console.WriteLine("Expected {0} got {1}", s, t);
                fail = true;
            } else if (showPasses)
            {
                Console.WriteLine("Passed");
                Console.WriteLine("Expected {0} got {1}", s, t);

            }
        }
    }
}
