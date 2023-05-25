using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodServiceBackend.Networking.Webserver
{
    /// <summary>
    /// Standard API reponse object
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// PostResponse standard constructor.
        /// </summary>
        /// <param name="_httpStatusCode">Standard http status code. Values from 200 to 299 indicate a successful operation.</param>
        /// <param name="_errorMessage">Any success- or errormessage for debug purposes.</param>
        /// <param name="_responseObject">A response object.</param>
        public ApiResponse(WebSocketSharp.Net.HttpStatusCode _httpStatusCode, string _errorMessage, object _responseObject)
        {
            httpStatusCode = _httpStatusCode;
            errorMessage = _errorMessage;
            responseObject = _responseObject;
        }

        private WebSocketSharp.Net.HttpStatusCode httpStatusCode;
        public WebSocketSharp.Net.HttpStatusCode HttpStatusCode
        {
            get { return httpStatusCode; }
        }
        public bool Success { get { return (((int)httpStatusCode) >= 200 && (int)httpStatusCode < 300); } }

        private string errorMessage;
        public string ErrorMesage { get { return errorMessage; } }


        private object responseObject;
        public object ResponseObject { get { return responseObject; } }
    }

}
