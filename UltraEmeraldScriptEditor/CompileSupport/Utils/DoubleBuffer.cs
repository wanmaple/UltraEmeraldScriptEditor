using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Utils
{
    /// <summary>
    /// 双缓冲区。
    /// </summary>
    internal sealed class DoubleBuffer
    {
        private static readonly Int32 BufferSize = 256;

        private class Buffer
        {
            public Buffer()
            {
                buffer = new Char[256];
                startOffset = 0;
                length = 0;
            }

            public Char[] buffer;
            public Int32 startOffset;
            public Int32 length;
        }

        public DoubleBuffer(TextReader reader)
        {
            _buffer1 = new Buffer();
            _buffer2 = new Buffer();
            _front = true;
            _reader = reader ?? throw new ArgumentNullException("reader");
            _cursor = 0;
        }

        public Char Read()
        {
            Buffer buffer = _front ? _buffer1 : _buffer2;
            if (buffer.length == 0)
            {
                buffer.startOffset = _cursor;
                buffer.length = _reader.Read(buffer.buffer, 0, BufferSize);
            }
            Char ret = buffer.buffer[_cursor - buffer.startOffset];
            ++_cursor;
            if (_cursor % BufferSize == 0)
            {
                _front = !_front;
            }
            return ret;
        }

        public void Seek(Int32 offset)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "offset must be greater than 0.");
            }
            if (offset > _cursor)
            {
                throw new InvalidOperationException("Cannot seek latest cursor in DoubleBuffer.");
            }

            Buffer buffer = _front ? _buffer1 : _buffer2;
            if (offset >= buffer.startOffset)
            {
                _cursor = offset;
            }
            else
            {
                buffer = _front ? _buffer2 : _buffer1;
                if (offset < buffer.startOffset)
                {
                    throw new InvalidOperationException("Seek offset is out of range in DoubleBuffer.");
                }
                _cursor = offset;
            }
        }

        private Buffer _buffer1;
        private Buffer _buffer2;
        private Boolean _front;
        private TextReader _reader;
        private Int32 _cursor;
    }
}
