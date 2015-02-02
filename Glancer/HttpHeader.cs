using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;

namespace Glancer
{
    public static class HTTP_PORT
    {
        public static readonly string HTTP = "80";
        public static readonly string HTTPS = "443";
    }

    public static class HTTP_HEADER_KEY
    {
        public static readonly string CONTENT_TYPE = "Content-Type";
        public static readonly string CONTENT_LENGTH = "Content-Length";
        public static readonly string TRANSFER_ENCODING = "Transfer-Encoding";
        public static readonly string CONNECTION = "Connection";
        public static readonly string HOST = "Host";
        public static readonly string PROXY_CONNECTION = "Proxy-Connection";
        public static readonly string SET_COOKIE = "Set-Cookie";
    }

    public static class HTTP_HEADER_VALUE
    {
        public static readonly string KEEPALLIVE = "Keep-Alive";
        public static readonly string CLOSE = "Close";
        public static readonly string CHUNKED =  "chunked";
        public static readonly string TEXT = "text/";
        public static readonly string JSON = "application/json";
    }

    public static class HTTP_METHOD
    {
        public static readonly string GET = "GET";
        public static readonly string HEAD = "HEAD";
        public static readonly string POST = "POST";
        public static readonly string PUT = "PUT";
        public static readonly string PATCH = "PATCH";
        public static readonly string OPTION = "OPTION";
        public static readonly string DELETE = "DELETE";
        public static readonly string TRACE = "TRACE";
        public static readonly string LINK = "LINK";
        public static readonly string UNLINK = "UNLINK";
        public static readonly string CONNECT = "CONNECT";
    }

    public static class HTTP_VERTION
    {
        public static readonly string HTTP10 = "HTTP/1.0";
        public static readonly string HTTP11 = "HTTP/1.1";
    }

    public class HttpHeader
    {
        public static NameValueCollection ParseHeader(StringReader header)
        {
            NameValueCollection dict = new NameValueCollection();
            string data = string.Empty;
            data = header.ReadLine();
            do
            {
                string[] s = data.Split(':');
                if (s.Length > 1)
                {
                    dict.Add(s[0], s[1].Trim());
                }

                data = header.ReadLine();
            } while (data != null);

            header.Close();
            return dict;
        }

        public static bool CheckBinary(NameValueCollection headers)
        {
            if (headers.Get(HTTP_HEADER_KEY.CONTENT_TYPE) != null)
            {
                string contentType = headers[HTTP_HEADER_KEY.CONTENT_TYPE];

                if (contentType.StartsWith(HTTP_HEADER_VALUE.TEXT) || contentType.StartsWith(HTTP_HEADER_VALUE.JSON))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ModifyProxyRequest(HttpRequestHeader request)
        {
            // modify hostheader.
            string absolutePath = string.Empty;

            if (request._uri != null) {

                string host = request._uri.Host;

                if (request._uri.Port != Convert.ToInt32(HTTP_PORT.HTTP) || request._uri.Port != Convert.ToInt32(HTTP_PORT.HTTPS))
                {
                    host += ":" + request._uri.Port;
                }
                request._headers[HTTP_HEADER_KEY.HOST] = host;

                absolutePath = request._uri.PathAndQuery;

            }else{
                absolutePath = request._path;
            }

            // create request line.
            request._requestLine = string.Format("{0} {1} {2}", request._method, absolutePath, request._httpVersion);


            if (request._headers[HTTP_HEADER_KEY.PROXY_CONNECTION] != null)
            {
                request._headers.Add(HTTP_HEADER_KEY.CONNECTION, request._headers[HTTP_HEADER_KEY.PROXY_CONNECTION]);
                request._headers.Remove(HTTP_HEADER_KEY.PROXY_CONNECTION);
            }


            // modify source.
            StringBuilder sb = new StringBuilder();
            sb.Append(request._requestLine);
            sb.Append(System.Environment.NewLine);

            StringReader sr = new StringReader(request._source);
            sr.ReadLine();
            string line = sr.ReadLine();
            while(string.IsNullOrEmpty(line) == false)
            {
                if (line.StartsWith(HTTP_HEADER_KEY.HOST))
                {
                    line = string.Format("{0}: {1}", HTTP_HEADER_KEY.HOST, request._headers[HTTP_HEADER_KEY.HOST]);
                }
                if (line.StartsWith(HTTP_HEADER_KEY.PROXY_CONNECTION))
                {
                    line = string.Format("{0}: {1}", HTTP_HEADER_KEY.CONNECTION, request._headers[HTTP_HEADER_KEY.CONNECTION]);
                }
                sb.Append(line);
                sb.Append(System.Environment.NewLine);

                line = sr.ReadLine();
            }
            sb.Append(System.Environment.NewLine);

            request._source = sb.ToString();

            return true;
        }
    }

    public class HttpRequestHeader
    {
        public NameValueCollection _headers{get; set;}
        public string _requestLine { get; set; }
        public Uri _uri { get; set; }
        public string _source = string.Empty;
        public int _contentSize = 0;
        public bool _isParse = false;
        public string _httpVersion { get; set; }
        public bool _isBinary { get; set; }

        public string _path { get; set; }
        public string _method { get; set; }
        public string _host { get; set; }
        public int _port { get; set; }

        public bool _isKeepAllive = false;
        public bool _isData { get; set; }

        public HttpRequestHeader(string source)
        {
            if (source.Length == 0)
            {
                _isData = false;
                return;
            }
            else
            {
                _isData = true;
            }

            _isBinary = false;
            _source = source;
            StringReader sreader = new StringReader(_source);

            try
            {
                if (ParseRequestMethod(sreader.ReadLine()) == false)
                {
                    _isParse = false;
                    return;
                }
                else
                {
                    _headers = HttpHeader.ParseHeader(sreader);

                    if (_httpVersion == HTTP_VERTION.HTTP10)
                    {
                        if (_headers.Get(HTTP_HEADER_KEY.CONNECTION) != null)
                        {
                            if (_headers[HTTP_HEADER_KEY.CONNECTION] == HTTP_HEADER_VALUE.KEEPALLIVE)
                            {
                                _isKeepAllive = true;
                            }
                        }
                        if (_headers.Get(HTTP_HEADER_KEY.PROXY_CONNECTION) != null)
                        {
                            if (_headers[HTTP_HEADER_KEY.PROXY_CONNECTION] == HTTP_HEADER_VALUE.KEEPALLIVE)
                            {
                                _isKeepAllive = true;
                            }
                        }
                    }
                    else
                    {
                        _isKeepAllive = true;
                        if (_headers.Get(HTTP_HEADER_KEY.CONNECTION) != null)
                        {
                            if (_headers[HTTP_HEADER_KEY.CONNECTION] == HTTP_HEADER_VALUE.CLOSE)
                            {
                                _isKeepAllive = false;
                            }
                        }
                        if (_headers.Get(HTTP_HEADER_KEY.PROXY_CONNECTION) != null)
                        {
                            if (_headers[HTTP_HEADER_KEY.PROXY_CONNECTION] == HTTP_HEADER_VALUE.CLOSE)
                            {
                                _isKeepAllive = false;
                            }
                        }
                    }

                    if (_headers.Get(HTTP_HEADER_KEY.CONTENT_LENGTH) != null)
                    {
                        _contentSize = Convert.ToInt32(_headers[HTTP_HEADER_KEY.CONTENT_LENGTH]);
                    }

                    if (_headers.Get(HTTP_HEADER_KEY.HOST) != null)
                    {
                        _host = _headers[HTTP_HEADER_KEY.HOST];
                        
                        if(_host.IndexOf(":") != -1){
                            string[] host = _host.Split(':');
                            _host = host[0];
                            _port = Convert.ToInt32(host[1]);
                        }
                        else
                        {
                            _host = _host;
                            _port = Convert.ToInt32(HTTP_PORT.HTTP);
                        }
                    }

                }
            }catch{
                _isParse = false;
                return;
            }

            _isBinary = HttpHeader.CheckBinary(_headers);
            _isParse = true;
        }

        private bool ParseRequestMethod(string header)
        {
            _requestLine = header;
            string[] s = header.Split(' ');
            if (s.Length > 2)
            {
                _method = s[0];
                _path = s[1];
                _httpVersion = s[2];

                if (_method != HTTP_METHOD.CONNECT && _path.StartsWith("/") ==false)
                {
                    _uri = new Uri(_path);
                }
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    public class HttpResponseHeader
    {
        public NameValueCollection _headers { get; set; }
        public string _source = string.Empty;
        public int _contentSize = 0;
        public bool _isParse = false;
        public string _httpVersion { get; set; }
        public bool _isBinary { get; set; }

        public int _statuscode { get; set; }
        public bool _isChunked = false;
        public bool _isKeepAllive = false;

        public HttpResponseHeader(string source)
        {
            _isBinary = false;
            _source = source;
            StringReader sreader = new StringReader(_source);

            try
            {
                if (ParseResponseCode(sreader.ReadLine()) == false)
                {
                    _isParse = false;
                    return;
                }
                else
                {
                    _headers = HttpHeader.ParseHeader(sreader);

                    if (_httpVersion == HTTP_VERTION.HTTP10)
                    {
                        if (_headers.Get(HTTP_HEADER_KEY.CONNECTION) != null)
                        {
                            if (_headers[HTTP_HEADER_KEY.CONNECTION] == HTTP_HEADER_VALUE.KEEPALLIVE)
                            {
                                _isKeepAllive = true;
                            }
                        }
                        if (_headers.Get(HTTP_HEADER_KEY.PROXY_CONNECTION) != null)
                        {
                            if (_headers[HTTP_HEADER_KEY.PROXY_CONNECTION] == HTTP_HEADER_VALUE.KEEPALLIVE)
                            {
                                _isKeepAllive = true;
                            }
                        }
                    }
                    else
                    {
                        _isKeepAllive = true;
                        if (_headers.Get(HTTP_HEADER_KEY.CONNECTION) != null)
                        {
                            if (_headers[HTTP_HEADER_KEY.CONNECTION] == HTTP_HEADER_VALUE.CLOSE)
                            {
                                _isKeepAllive = false;
                            }
                        }
                        if (_headers.Get(HTTP_HEADER_KEY.PROXY_CONNECTION) != null)
                        {
                            if (_headers[HTTP_HEADER_KEY.PROXY_CONNECTION] == HTTP_HEADER_VALUE.CLOSE)
                            {
                                _isKeepAllive = false;
                            }
                        }
                    }

                    if (_headers.Get(HTTP_HEADER_KEY.TRANSFER_ENCODING) != null)
                    {
                        if (_headers[HTTP_HEADER_KEY.TRANSFER_ENCODING] == HTTP_HEADER_VALUE.CHUNKED)
                        {
                            _isChunked = true;
                        }
                    }
                    else if (_headers.Get(HTTP_HEADER_KEY.CONTENT_LENGTH) != null)
                    {
                        _contentSize = Convert.ToInt32(_headers[HTTP_HEADER_KEY.CONTENT_LENGTH]);
                    }
                }
            }catch{
                _isParse = false;
                return;
            }

            _isBinary = HttpHeader.CheckBinary(_headers);
            _isParse = true;
        }

        private bool ParseResponseCode(string header)
        {
            string[] s = header.Split(' ');
            if (s.Length > 2)
            {
                _httpVersion = s[0];
                _statuscode = Convert.ToInt32(s[1]);
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
