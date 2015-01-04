using System;
using System.IO;

namespace Glancer
{
    interface IHttpEventListener
    {
        HttpRequestObject OnHttpRequestClient(HttpRequestObject request, Stream serverStream, Stream clientStream);
        HttpRequestObject OnHttpRequestServer(HttpRequestObject request, Stream serverStream, Stream clientStream);
        HttpResponseObject OnHttpResponseClient(HttpResponseObject response, Stream serverStream, Stream clientStream);
        HttpResponseObject OnHttpResponseServer(HttpResponseObject response, Stream serverStream, Stream clientStream);
    }
}
