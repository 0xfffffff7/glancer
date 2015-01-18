using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;

namespace Glancer
{
    class HttpStream
    {
        public static string STREAM_TERMINATE = System.Environment.NewLine + System.Environment.NewLine;
        static char CR = '\r';
        static char LF = '\n';
        public static int BUFFER_SIZE = 1024;
        public static int READ_TIMEOUT = 5000;
        public static int READ_CONTENT_TIMEOUT = 5000;
       
        public static HttpRequestObject ReadRequest(Stream stream)
        {
            HttpRequestObject request = null;
            byte[] buffer = null;

            buffer = Read(stream);
            if (buffer == null)
            {
                return null;
            }

            HttpRequestHeader header = new HttpRequestHeader(Encoding.UTF8.GetString(buffer));
            if (header._isParse == false)
            {
                return null;
            }

            HttpContent content = new HttpContent();
            if (0 < header._contentSize)
            {
                content.Set(ReadContent(stream, header._contentSize));
            }

            request = new HttpRequestObject(header, content);

            return request;
        }

        public static HttpResponseObject ReadResponse(Stream stream)
        {
            byte[] buffer = Read(stream);
            if (buffer == null)
            {
                return null;
            }

            HttpResponseHeader header = new HttpResponseHeader(Encoding.UTF8.GetString(buffer));
            HttpContent content = new HttpContent();
            if (0 < header._contentSize)
            {
                content.Set(ReadContent(stream, header._contentSize));
            }
            else if (header._isChunked)
            {
                content.Set(ReadChunked(stream));
            }
            HttpResponseObject response = new HttpResponseObject(header, content);
            return response;
        }

        public static byte[] Read(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            stream.ReadTimeout = READ_TIMEOUT;
            bool newline = false;
            try
            {
                while (true)
                {
                    int c = stream.ReadByte();
                    if (c == -1)
                    {
                        break;
                    }
                    if (c == CR)
                    {
                        ms.WriteByte(Convert.ToByte(c));

                        c = stream.ReadByte();
                        if (c == -1)
                        {
                            break;
                        }
                        if (c == LF)
                        {
                            if (newline)
                            {
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
                    else
                    {
                        ms.WriteByte(Convert.ToByte(c));
                        newline = false;
                    }
                }

            }
            catch (System.IO.IOException e)
            {
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (ms.Length == 0)
            {
                return null;
            }

            return ms.ToArray();
        }

        public static byte[] ReadContent(Stream stream, int length)
        {
            MemoryStream ms = new MemoryStream();
            byte[] buffer = new byte[BUFFER_SIZE];
            int n = 0;
            stream.ReadTimeout = READ_CONTENT_TIMEOUT;
            int count = 0;

            try
            {
                while (count < length)
                {
                    n = stream.Read(buffer, 0, BUFFER_SIZE);
                    ms.Write(buffer, 0, n);
                    count += n;
                }
            }
            catch (System.IO.IOException e)
            {
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (ms.Length == 0)
            {
                return null;
            }

            return ms.ToArray();
        }

        public static byte[] ReadChunked(Stream stream)
        {
            StringBuilder size = new StringBuilder();
            MemoryStream ms = new MemoryStream();
            stream.ReadTimeout = READ_CONTENT_TIMEOUT;
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

        public static void Write(HttpObject http, Stream stream){

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

            Write(stream, bytes);

            if (0 < http._content._length)
            {
                Write(stream, http._content.Get());
            }
        }

        public static void Write(Stream stream, byte[] byteContent)
        {
            stream.Write(byteContent, 0, byteContent.Length);
            stream.Flush();
        }
    }
}
