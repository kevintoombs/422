//Kevin Toombs

using System;
using System.Threading;
using System.Threading.Tasks;

namespace CS422
{
	public class ThreadPoolSleepSorter: IDisposable
	{
		ushort _threadCount;
		Sleeper[] _sleepers;
		Thread[] _threads; 

		//Super simple constructor. Creates the sleepers and asigns them to threads.
		public ThreadPoolSleepSorter(System.IO.TextWriter output, ushort threadCount)
		{
			_threads = new Thread[threadCount]; 
			_sleepers = new Sleeper[threadCount]; 
			_threadCount = threadCount;
			for (int i = 0; i < _threadCount; i++) {
				Sleeper sleeper = new Sleeper(output);
				_sleepers[i] = sleeper;
				_threads[i] = new Thread(sleeper.ThreadRun);
			}
		}


		//I chode to assign values before begining sort because n is small,
		//and this reducdes the likelyhood 
		public void Sort(byte[] values)
		{
			for (int i = 0; i<values.Length; i++){
				_sleepers [i]._value = values [i];
			}
			for (int i = 0; i<values.Length; i++){
				_threads [i].Start ();
			}
		}

		//MSDN defined #region 
		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.
				_sleepers = null;
				_threads = null;

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~ThreadPoolSleepSorter() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		void IDisposable.Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion

		//This is my "worker" object for the thread.
		//The value isn't part of the consturctor because this oject needs to be reused
		//each time sort is called
		internal class Sleeper
		{
			System.IO.TextWriter _tw;
			internal int _value;

			internal Sleeper(System.IO.TextWriter tw) {_tw=tw;}
			void setValue(int value){_value = value;}
			public void ThreadRun()
			{
				Thread.Sleep (_value * 1000);
				_tw.WriteLine(_value);
				Console.WriteLine (_value);

			}
		}
	}
}