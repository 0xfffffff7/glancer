using System;
using System.Text;

namespace Glancer
{
    public class HttpObject
    {
        public HttpHeader _header = null;
        public HttpContent _content = null;
    }

    public class HttpRequestObject : HttpObject
    {
        public new HttpRequestHeader _header = null;

        public HttpRequestObject(HttpRequestHeader header, HttpContent content)
        {
            _header = header;
            _content = content;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(_header._source);
            if (_header._isBinary == false)
            {
                sb.Append(Encoding.UTF8.GetString(_content.Get()));
            }

            return sb.ToString();
        }
    }

    public class HttpResponseObject : HttpObject
    {
        public new HttpResponseHeader _header = null;

        public HttpResponseObject(HttpResponseHeader header, HttpContent content)
        {
            _header = header;
            _content = content;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(_header._source);
            if (_header._isBinary == false && 0 < _content._length)
            {
                sb.Append(Encoding.UTF8.GetString(_content.Get()));
            }

            return sb.ToString();
        }
    }
}
