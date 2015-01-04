using System;
using System.IO;

namespace Glancer
{
    class HttpStreamProxy
    {

        static public void Proxy(Stream client, int writeTimeout, Stream server, int readTimeout, IHttpEventListener listner)
        {

            while (true)
            {
                //-----------------------------------------
                // Request.
                // Clinet -> Proxy
                //-----------------------------------------
                HttpRequestObject request = null;
                try
                {
                    request = HttpStream.ReadRequest(server, readTimeout);

                    if (request == null)
                    {
                        break;
                    }

                    //-----------------------------------------
                    // Observer.
                    //-----------------------------------------
                    if (listner != null)
                    {
                        request = listner.OnHttpRequestClient(request, server, client);
                    }


                }catch{
                    break;
                }



                //-----------------------------------------
                // Request.
                // Proxy -> Server
                //-----------------------------------------
                try
                {
                    HttpStream.Write(request, client, writeTimeout);

                    //-----------------------------------------
                    // Observer.
                    //-----------------------------------------
                    if (listner != null)
                    {
                        request = listner.OnHttpRequestServer(request, server, client);
                    }

                }catch{
                    break;
                }




                //-----------------------------------------
                // Response.
                // Proxy <- Server
                //-----------------------------------------
                HttpResponseObject response = null;
                try
                {
                    response = HttpStream.ReadResponse(client, readTimeout);
                    if (response == null)
                    {
                        break;
                    }

                    //-----------------------------------------
                    // Observer.
                    //-----------------------------------------
                    if (listner != null)
                    {
                        response = listner.OnHttpResponseClient(response, server, client);
                    }

                }catch{
                    break;
                }



                //-----------------------------------------
                // Response.
                // Client <- Proxy
                //-----------------------------------------
                try
                {
                    HttpStream.Write(response, server, writeTimeout);

                    //-----------------------------------------
                    // Observer.
                    //-----------------------------------------
                    if (listner != null)
                    {
                        response = listner.OnHttpResponseServer(response, server, client);
                    }

                }catch{
                    break;
                }




                if (request._header._isKeepAllive == false || response._header._isKeepAllive == false)
                {
                    break;
                }
            }

            client.Close();
            server.Close();
        }

    }
}
