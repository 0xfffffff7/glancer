using System;
using System.Text;
using System.IO;

namespace Glancer
{
    class HttpStream
    {
        public static string STREAM_TERMINATE = System.Environment.NewLine + System.Environment.NewLine;
        static char CR = '\r';
        static char LF = '\n';

        public static HttpRequestObject ReadRequest(Stream stream, int timeout)
        {
            HttpRequestHeader header = new HttpRequestHeader( Encoding.UTF8.GetString(Read(stream, timeout)) );
            if (header._isParse == false)
            {
                return null;
            }

            HttpContent content = new HttpContent();
            if (0 < header._contentSize)
            {
                content.Set(ReadContent(stream, header._contentSize, timeout));
            }

            HttpRequestObject request = new HttpRequestObject(header, content);
            return request;
        }

        public static HttpResponseObject ReadResponse(Stream stream, int timeout)
        {
            HttpResponseHeader header = new HttpResponseHeader( Encoding.UTF8.GetString(Read(stream, timeout)));
            HttpContent content = new HttpContent();
            if (0 < header._contentSize)
            {
                content.Set(ReadContent(stream, header._contentSize, timeout));
            }
            else if (header._isChunked)
            {
                content.Set(ReadChunked(stream, timeout));
            }
            HttpResponseObject response = new HttpResponseObject(header, content);
            return response;
        }

        public static byte[] Read(Stream stream, int timeout)
        {
            MemoryStream ms = new MemoryStream();
            stream.ReadTimeout = timeout;
            bool newline = false;
            try {
                while (true)
                {
                    int c = stream.ReadByte();
                    if (c == CR)
                    {
                        ms.WriteByte(Convert.ToByte(c));

                        c = stream.ReadByte();
                        if (c == LF)
                        {
                            if (newline) {
                                ms.WriteByte(Convert.ToByte(c)); 
                                break;
                            }
                            else
                            {
                                newline = true;
                            }
                        }

                        ms.WriteByte(Convert.ToByte(c));
                    }
                    else{
                        ms.WriteByte(Convert.ToByte(c));
                        newline = false;
                    }
                }

            }catch(Exception e){
                throw e;
            }

            return ms.ToArray();
        }

        public static byte[] ReadContent(Stream stream, int length, int timeout)
        {
            MemoryStream ms = new MemoryStream();

            stream.ReadTimeout = timeout;
            int count = 0;
            try
            {
                while (count < length)
                {
                    int c = stream.ReadByte();
                    ms.WriteByte(Convert.ToByte(c));
                    count++;
                }
            }
            catch (Exception e){
                throw e;
            }

            return ms.ToArray();
        }

        public static byte[] ReadChunked(Stream stream, int timeout)
        {
            StringBuilder size = new StringBuilder();
            MemoryStream ms = new MemoryStream();

            stream.ReadTimeout = timeout;
            int c = 0;

            try {
                while (true)
                {
                    while (true)
                    {
                        c = stream.ReadByte();
                        ms.WriteByte(Convert.ToByte(c));

                        if (c == CR)
                        {
                            c = stream.ReadByte();
                            ms.WriteByte(Convert.ToByte(c));
                            if (c == LF)
                            {
                                break;
                            }
                        }

                        if (Convert.ToChar(c) != ';')
                        {
                            size.Append(Convert.ToChar(c));
                        }
                    }

                    int readSize = Convert.ToInt32(size.ToString(), 16);
                    if (readSize == 0)
                    {
                        stream.ReadByte();
                        stream.ReadByte();
                        ms.Write(Encoding.UTF8.GetBytes((System.Environment.NewLine)), 0, 2);
                        break;
                    }
                    size.Clear();

                    int count = 0;
                    while (count++ < readSize)
                    {
                        c = stream.ReadByte();
                        ms.WriteByte(Convert.ToByte(c));
                    }


                    // New line.
                    stream.ReadByte();
                    stream.ReadByte();
                    ms.Write(Encoding.UTF8.GetBytes((System.Environment.NewLine)), 0, System.Environment.NewLine.Length);
                }

            }catch(Exception e){
                throw e;
            }

            return ms.ToArray();
        }

        public static void Write(HttpObject http, Stream stream, int timeout){

            byte[] bytes = null;
            if (http.GetType() == typeof(HttpRequestObject))
            {
                HttpRequestObject request = (HttpRequestObject)http;
                bytes = Encoding.UTF8.GetBytes(request._header._source);
            }
            else if (http.GetType() == typeof(HttpResponseObject))
            {
                HttpResponseObject response = (HttpResponseObject)http;
                bytes = Encoding.UTF8.GetBytes(response._header._source);
            }

            Write(stream, bytes, timeout);

            if (0 < http._content._length)
            {
                Write(stream, http._content.Get(), timeout);
            }
        }

        public static void Write(Stream stream, byte[] byteContent, int timeout)
        {
            stream.WriteTimeout = timeout;
            stream.Write(byteContent, 0, byteContent.Length);
            stream.Flush();
        }
    }
}
