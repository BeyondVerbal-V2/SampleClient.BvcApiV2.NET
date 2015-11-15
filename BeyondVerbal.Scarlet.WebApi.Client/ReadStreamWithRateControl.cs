using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeyondVerbal.Scarlet.WebApi.Client
{
    class ReadStreamWithRateControl : Stream
    {
        Stream inner;
        int bytesPerSec;
        public ReadStreamWithRateControl(Stream inner, int bytesPerSec)
        {
            this.inner = inner;
            this.bytesPerSec = bytesPerSec;
        }

        public override bool CanRead { get { return inner.CanRead; } }

        public override bool CanSeek { get { return inner.CanSeek; } }

        public override bool CanWrite { get { return false; } }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length { get { return inner.Length; } }

        public override long Position { get { return inner.Position; } set { inner.Position = value; } }



        DateTime? startTime;
        int totalReadCount = 0;

        void SleepIfNeeded(int readBytes)
        {
            if (bytesPerSec > 0)
            {
                var now = DateTime.Now;
                totalReadCount += readBytes;
                if (startTime.HasValue)
                {
                    var elapsedSec = (now - startTime.Value).TotalSeconds;
                    var calcTimeSec = (double)totalReadCount / bytesPerSec;
                    var needToSleepSec = calcTimeSec - elapsedSec;
                    if (needToSleepSec > 0)
                        Thread.Sleep(TimeSpan.FromSeconds(needToSleepSec));
                }
                else
                {
                    startTime = now;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {

            int read = inner.Read(buffer, offset, count);
            SleepIfNeeded(read);
            return read;
        }



        public override long Seek(long offset, SeekOrigin origin)
        {



            return inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                inner.Dispose();
                //base.Dispose();
                disposed = true;
            }
        }
    }
}
