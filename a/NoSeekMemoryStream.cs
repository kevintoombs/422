using System;
using System.IO;

namespace CS422
{
	/// <summary>
	/// Represents a memory stream that does not support seeking, but otherwise has
	/// functionality identical to the MemoryStream class.
	/// </summary>
	public class NoSeekMemoryStream : MemoryStream
	{
	

		public NoSeekMemoryStream(byte[] buffer): base(buffer)
		{

		}
			
		public NoSeekMemoryStream(byte[] buffer, int offset, int count): base(buffer, offset, count)
		{

		}

		public override bool CanSeek {
			get {
				return false;
			}
		}

		public override long Seek (long offset, SeekOrigin loc)
		{
			throw new NotSupportedException ();
		}

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

    }
}

