using System;
using System.Text;

namespace Glancer
{
    public class HttpContent
    {
        private byte[] _content = null;
        public int _length { get; set; }

        public void Set(byte[] content)
        {
            _content = content;
            _length = content.Length;
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(_content);
        }

        public void Set(string content){
            byte[] _content = Encoding.UTF8.GetBytes(content);
            _length = _content.Length;
        }

        public byte[] Get()
        {
            return _content;
        }
    }
}
