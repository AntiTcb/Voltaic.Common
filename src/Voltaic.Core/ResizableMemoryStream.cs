using System;
using System.Buffers;
using System.IO;

namespace Voltaic
{
    // TODO: Test
    public class ResizableMemoryStream : Stream
    {
        private ResizableMemory<byte> _buffer;
        private int _position;
        
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;

        public ref ResizableMemory<byte> Buffer => ref _buffer;
        public override long Length => _buffer.Length;
        public override long Position
        {
            get => _position;
            set
            {
                if (value > int.MaxValue)
                    throw new ArgumentOutOfRangeException($"{nameof(ResizableMemoryStream)} does not support streams larger than Int32.MaxValue");
                _position = (int)value;
            }
        }

        public ResizableMemoryStream(int initialCapacity, ArrayPool<byte> pool = null)
        {
            _buffer = new ResizableMemory<byte>(initialCapacity, pool);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int remaining = _buffer.Length - _position;
            if (remaining < count)
                count = remaining;
            _buffer.AsReadOnlySpan(_position, count).CopyTo(buffer.AsSpan(offset, count));
            Position += count;
            return count;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            var span = _buffer.RequestSpan(_position, count);
            buffer.AsSpan(offset, count).CopyTo(span);
            _buffer.Advance(count);
            Position += count;
        }

        public override void SetLength(long value)
        {
            if (value > int.MaxValue)
                throw new InvalidOperationException($"{nameof(ResizableMemoryStream)} does not support streams larger than Int32.MaxValue elements");
            _buffer.Length = (int)value;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }
            return Position;
        }

        public override void Flush() { }
    }
}
