using CS422;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{

	public class App
	{
		static void newMain()
		{
			PCQueue queue = new PCQueue();
			queue.Enqueue(2);
			int outval = 0;
			queue.Dequeue(ref outval);
			Console.WriteLine(outval);
		}

		static void Main()
		{
			int iterations = 90000;
			PCQueue queue = new PCQueue();
			Console.WriteLine("Configuring worker threads...");
			Producer producer = new Producer(queue, iterations);
			Consumer consumer = new Consumer(queue, iterations);
			Thread producerThread = new Thread(producer.ThreadRun);
			Thread consumerThread = new Thread(consumer.ThreadRun);

			Console.WriteLine("Launching producer and consumer threads...");

			consumerThread.Start();
			producerThread.Start();

			Thread.Sleep(250);

			Console.WriteLine("Signaling threads to terminate...");



			consumerThread.Join();
			producerThread.Join();
		}

	}

	internal class Consumer
	{
		PCQueue _pcq;
		int _iterations;
		internal Consumer(PCQueue pcq, int iterations) { _pcq = pcq; _iterations = iterations; }
		public void ThreadRun()
		{
			int i = 0;
			int nah = 0;
			while (i < _iterations) {
				int outval = 0;
				if (_pcq.Dequeue(ref outval))
				{
					if (outval != i){
						Console.WriteLine ("wrong");
						Console.WriteLine(outval);
						Console.WriteLine(i);
					
					}
					i++;
				}
				else
				{
					nah++;
					Console.WriteLine("nah");
				}
			}
			Console.WriteLine("nah: " + nah);
		}
	}

	internal class Producer
	{
		PCQueue _pcq;
		int _iterations;
		internal Producer(PCQueue pcq, int iterations) { _pcq = pcq; _iterations = iterations; }
		public void ThreadRun()
		{
			for (int i = 0; i < _iterations; i++)
			{
				//Console.WriteLine("E: " + i);
				_pcq.Enqueue(i);
			}
		}
	}
}