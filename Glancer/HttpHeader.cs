using System;
using System.Collections.Generic;
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

    public class HttpHeader
    {
        public Dictionary<string, string> headers { get; set; }
        public string _source = string.Empty;
        public int _contentSize = 0;
        public bool _isParse = false;
        public string _httpVersion { get; set; }
        public bool _isBinary { get; set; }

        public static Dictionary<string, string> ParseHeader(StringReader header)
        {
            Dictionary<string, string> dict = new Dictionary<string,string>();
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

        public static bool CheckBinary(Dictionary<string, string> headers){
            if (headers.ContainsKey(HTTP_HEADER_KEY.CONTENT_TYPE))
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
    }

    public class HttpRequestHeader : HttpHeader
    {
        public string _uri { get; set; }
        public string _path { get; set; }
        public string _method { get; set; }
        public bool _isKeepAllive = false;

        public HttpRequestHeader(string source)
        {
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
                    headers = HttpHeader.ParseHeader(sreader);

                    if (headers.ContainsKey(HTTP_HEADER_KEY.CONNECTION))
                    {
                        if (headers[HTTP_HEADER_KEY.CONNECTION] == HTTP_HEADER_VALUE.KEEPALLIVE)
                        {
                            _isKeepAllive = true;
                        }
                    }
                    if (headers.ContainsKey(HTTP_HEADER_KEY.CONTENT_LENGTH))
                    {
                        _contentSize = Convert.ToInt32(headers[HTTP_HEADER_KEY.CONTENT_LENGTH]);
                    }
                }
            }catch{
                _isParse = false;
                return;
            }

            _isBinary = CheckBinary(headers);
            _isParse = true;
        }

        private bool ParseRequestMethod(string header)
        {
            string[] s = header.Split(' ');
            if (s.Length > 2)
            {
                _method = s[0];
                _path = s[1];
                _httpVersion = s[2];
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    public class HttpResponseHeader : HttpHeader
    {
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
                    headers = HttpHeader.ParseHeader(sreader);

                    if (headers.ContainsKey(HTTP_HEADER_KEY.CONNECTION))
                    {
                        if (headers[HTTP_HEADER_KEY.CONNECTION] != HTTP_HEADER_VALUE.CLOSE)
                        {
                            _isKeepAllive = true;
                        }
                    }

                    if (headers.ContainsKey(HTTP_HEADER_KEY.TRANSFER_ENCODING)){
                        if (headers[HTTP_HEADER_KEY.TRANSFER_ENCODING] == HTTP_HEADER_VALUE.CHUNKED)
                        {
                            _isChunked = true;
                        }
                    }else if(headers.ContainsKey(HTTP_HEADER_KEY.CONTENT_LENGTH)){
                        _contentSize = Convert.ToInt32(headers[HTTP_HEADER_KEY.CONTENT_LENGTH]);
                    }
                }
            }catch{
                _isParse = false;
                return;
            }

            _isBinary = CheckBinary(headers);
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
