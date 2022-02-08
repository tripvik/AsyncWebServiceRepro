using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Protocols;

namespace AsyncWebServiceRepro
{
    public class ExceptionHandlingHandlerFactory : IHttpHandlerFactory
    {
        private IHttpHandlerFactory _innerHandlerFactory = new WebServiceHandlerFactory();

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            IHttpHandler handler = _innerHandlerFactory.GetHandler(context, requestType, url, pathTranslated);
            if (handler is IHttpAsyncHandler) // Only change behavior when async
            {
                return new ExceptionHandlingHandler((IHttpAsyncHandler)handler, _innerHandlerFactory);
            }
            else
            {
                return handler;
            }
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if(handler is ExceptionHandlingHandler)
            {
                ((ExceptionHandlingHandler)handler).ReleaseHandler();
            }
            else
            {
                _innerHandlerFactory.ReleaseHandler(handler);
            }
        }
    }
}