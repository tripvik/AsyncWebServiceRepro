using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AsyncWebServiceRepro
{
    public class ExceptionHandlingHandler : IHttpAsyncHandler
    {
        private ExceptionHandlingHandlerFactory _parent;
        private IHttpAsyncHandler _originalHandler;
        private IHttpHandlerFactory _originalFactory;
        private bool _threwDuringBegin = true;

        public ExceptionHandlingHandler(IHttpAsyncHandler originalHandler, IHttpHandlerFactory originalFactory)
        {
            _originalHandler = originalHandler;
            _originalFactory = originalFactory;
        }

        #region IHttpHandler Members

        public bool IsReusable => _originalHandler.IsReusable;

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object state)
        {
            return AsyncUtils.AsApm(ProcessRequestAsync(context), cb, state);
        }

        private IAsyncResult BeginProcessRequestWrapped(HttpContext context, AsyncCallback cb, object state)
        {
            var result = _originalHandler.BeginProcessRequest(context, cb, state);
            _threwDuringBegin = false;
            return result;
        }

        public void EndProcessRequest(IAsyncResult iar)
        {
            ((Task)iar).GetAwaiter().GetResult();
        }

        private async Task ProcessRequestAsync(HttpContext context)
        {
            try
            {
                await Task.Factory.FromAsync(BeginProcessRequestWrapped,
                                             _originalHandler.EndProcessRequest,
                                             context,
                                             null,
                                             TaskCreationOptions.None);
            }
            catch (Exception e) // Might want to only catch XmlException if wanting to only change behavior of single failure case
            {
                // If _threwDuringBegin is false, then any service exceptions should already be handled and this is a different issue
                // In which case, let the exception be thrown like normal as we don't know how to handle this different case.
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException || !_threwDuringBegin)
                {
                    throw;
                }

                // Exception happened during call to BeginProcessRequest. The only code not inside a try/catch which can throw is
                // protocol.ReadParameters(). We should get the same failure calling ProcessRequest which will handle the failure
                // correctly.
                _originalHandler.ProcessRequest(context);
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException("This class should only be used for async operations");
        }
        #endregion

        internal void ReleaseHandler()
        {
            _originalFactory.ReleaseHandler(_originalHandler);
        }
    }
}
