using System;
using System.IO;

namespace Glancer
{
    class HttpEventListener : IHttpEventListener
    {
        private TraceLogger _logger = null;
 
        public HttpEventListener(TraceLogger logger)
        {
            _logger = logger;
             _logger._bIndent = false;
        }

        public HttpRequestObject OnHttpRequestClient(HttpRequestObject request, Stream serverStream, Stream clientStream)
        {
            _logger.OutputLog("Request Clinet -> Proxy");
            _logger.OutputLog(request._header._source);
            return request;
        }
        public HttpRequestObject OnHttpRequestServer(HttpRequestObject request, Stream serverStream, Stream clientStream)
        {
            _logger.OutputLog("Request Proxy -> Server");
            _logger.OutputLog(request._header._source);
            return request;
        }
        public HttpResponseObject OnHttpResponseClient(HttpResponseObject response, Stream serverStream, Stream clientStream)
        {
            _logger.OutputLog("Response Proxy <- Server");
            _logger.OutputLog(response.ToString());
            return response;
        }
        public HttpResponseObject OnHttpResponseServer(HttpResponseObject response, Stream serverStream, Stream clientStream)
        {
            _logger.OutputLog("Response Clinet <- Proxy");
            _logger.OutputLog(response.ToString());
            return response;
        }
    }
}
