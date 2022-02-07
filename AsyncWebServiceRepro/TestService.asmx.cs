using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services;

namespace AsyncWebServiceRepro
{
    /// <summary>
    /// Summary description for TestService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class TestService : WebService
    {
        const int Delay = 500;

        #region Simple Hello World - Synchronous
        [WebMethod]
        public string HelloWorld()
        {
            Thread.Sleep(Delay);
            return "Hello World";
        }
        #endregion 

        #region Simple Hello World - Asynchronous
        [WebMethod]
        public IAsyncResult BeginHelloWorldAsync(AsyncCallback cb, object state)
        {
            return AsyncUtils.AsApm(HelloWorldAsync(), cb, state);
        }

        [WebMethod]
        public string EndHelloWorldAsync(IAsyncResult iar)
        {
            return ((Task<string>)iar).GetAwaiter().GetResult();
        }

        async Task<string> HelloWorldAsync()
        {
            await Task.Delay(Delay);
            return "Hello World (Asynchronously)";
        }
        #endregion

        #region Throw an exception - Synchronous
        [WebMethod]
        public string HelloException()
        {
            Thread.Sleep(Delay);
            RaiseAnException();
            return "should never have gotten here";
        }
        #endregion

        #region Throw an exception - Asynchronous
        [WebMethod]
        public IAsyncResult BeginHelloExceptionAsync(AsyncCallback cb, object state)
        {
            return AsyncUtils.AsApm(HelloExceptionAsync(), cb, state);
        }

        [WebMethod]
        public string EndHelloExceptionAsync(IAsyncResult iar)
        {
            return ((Task<string>)iar).GetAwaiter().GetResult();
        }

        async Task<string> HelloExceptionAsync()
        {
            await Task.Delay(Delay);
            RaiseAnException();
            return "should never have gotten here";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void RaiseAnException()
        {
            // a SoapException with a ClientFaultCode and an inner XmlException will produce a 400 response; everything else gets a 500.
            //throw new SoapException("Invalid request", SoapException.ClientFaultCode, new XmlException());
            throw new NotImplementedException("Sorry, the operation to throw/raise an exception has not been implemented. (beat) Wait a second...");
        }
        #endregion
    }
}
