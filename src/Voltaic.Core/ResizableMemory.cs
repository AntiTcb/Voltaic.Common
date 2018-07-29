using System;
using System.Buffers;

namespace Voltaic
{
    public struct ResizableMemory<T>
    {
        private int _length;

        public ArrayPool<T> Pool { get; private set; }
        public T[] Array { get; private set; }
        public int Length
        {
            get => _length;
            set
            {
                RequestLength(value);
                _length = value;
            }
        }

        public ResizableMemory(int initalCapacity, ArrayPool<T> pool = null)
        {
            Pool = pool ?? ArrayPool<T>.Shared;
            Array = Pool.Rent(initalCapacity);
            _length = 0;
        }

        public void Push(T item)
        {
            RequestLength(_length + 1);
            Array[Length++] = item;
        }
        public T Pop()
        {
            return Array[--Length];
        }

        public ArraySegment<T> RequestSegment(int length)
        {
            RequestLength(_length + length);
            return new ArraySegment<T>(Array, _length, length);
        }
        public ArraySegment<T> RequestSegment(int position, int length)
        {
            RequestLength(position + length);
            return new ArraySegment<T>(Array, position, length);
        }
        public Memory<T> RequestMemory(int length)
        {
            RequestLength(_length + length);
            return new Memory<T>(Array, _length, length);
        }
        public Memory<T> RequestMemory(int position, int length)
        {
            RequestLength(position + length);
            return new Memory<T>(Array, position, length);
        }
        public Span<T> RequestSpan(int length)
        {
            RequestLength(_length + length);
            return new Span<T>(Array, _length, length);
        }
        public Span<T> RequestSpan(int position, int length)
        {
            RequestLength(position + length);
            return new Span<T>(Array, position, length);
        }
        public void Advance(int count)
        {
            Length += count;
        }

        public void Clear()
        {
            Length = 0;
        }

        public ArraySegment<T> AsSegment() 
            => new ArraySegment<T>(Array, 0, _length);
        public Memory<T> AsMemory()
            => new Memory<T>(Array, 0, _length);
        public ReadOnlyMemory<T> AsReadOnlyMemory()
            => new ReadOnlyMemory<T>(Array, 0, _length);
        public Span<T> AsSpan()
            => new Span<T>(Array, 0, _length);
        public ReadOnlySpan<T> AsReadOnlySpan()
            => new ReadOnlySpan<T>(Array, 0, _length);

        public ArraySegment<T> AsSegment(int offset) 
            => new ArraySegment<T>(Array, offset, _length - offset);
        public Memory<T> AsMemory(int offset)
            => new Memory<T>(Array, offset, _length - offset);
        public ReadOnlyMemory<T> AsReadOnlyMemory(int offset)
            => new ReadOnlyMemory<T>(Array, offset, _length - offset);
        public Span<T> AsSpan(int offset)
            => new Span<T>(Array, offset, _length - offset);
        public ReadOnlySpan<T> AsReadOnlySpan(int offset)
            => new ReadOnlySpan<T>(Array, offset, _length - offset);

        public ArraySegment<T> AsSegment(int offset, int count)
        {
            int remaining = _length - offset;
            if (remaining < count)
                throw new ArgumentOutOfRangeException($"{nameof(count)} is larger than {nameof(Length)} - {nameof(offset)}");
            return new ArraySegment<T>(Array, offset, count);
        }
        public Memory<T> AsMemory(int offset, int count)
        {
            int remaining = _length - offset;
            if (remaining < count)
                throw new ArgumentOutOfRangeException($"{nameof(count)} is larger than {nameof(Length)} - {nameof(offset)}");
            return new Memory<T>(Array, offset, count);
        }
        public ReadOnlyMemory<T> AsReadOnlyMemory(int offset, int count)
        {
            int remaining = _length - offset;
            if (remaining < count)
                throw new ArgumentOutOfRangeException($"{nameof(count)} is larger than {nameof(Length)} - {nameof(offset)}");
            return new ReadOnlyMemory<T>(Array, offset, count);
        }
        public Span<T> AsSpan(int offset, int count)
        {
            int remaining = _length - offset;
            if (remaining < count)
                throw new ArgumentOutOfRangeException($"{nameof(count)} is larger than {nameof(Length)} - {nameof(offset)}");
            return new Span<T>(Array, offset, count);
        }
        public ReadOnlySpan<T> AsReadOnlySpan(int offset, int count)
        {
            int remaining = _length - offset;
            if (remaining < count)
                throw new ArgumentOutOfRangeException($"{nameof(count)} is larger than {nameof(Length)} - {nameof(offset)}");
            return new ReadOnlySpan<T>(Array, offset, count);
        }

        public T[] ToArray()
        {
            if (Length == Array.Length)
                return Array;
            var result = new T[Length];
            Array.AsSpan(0, Length).CopyTo(result);
            return result;
        }

        private void RequestLength(int minlength)
        {
            if (minlength > Array.Length)
            {
                long newSize = Array.Length;
                while (newSize < minlength)
                    newSize *= 2;
                if (newSize > int.MaxValue)
                    newSize = int.MaxValue;

                var oldArray = Array;
                Array = Pool.Rent((int)newSize);
                oldArray.AsSpan(0, Length).CopyTo(Array);
                Pool.Return(oldArray);
            }
        }

        public void Return()
        {
            if (Array != null)
            {
                Pool.Return(Array);
                Array = null;
            }
        }
    }
}
