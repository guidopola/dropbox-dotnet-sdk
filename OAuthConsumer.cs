/*
 * Copyright (c) 2013 Guido Pola <prodito@live.com>.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using MiniJSON;

namespace OAuth
{
    /// <summary>
    /// 
    /// </summary>
    public class OAuthException : Exception {

        public OAuthException(string message)
            : base(message) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RequestMethod { GET, POST, PUT, DELETE }

    /// <summary>
    /// 
    /// </summary>
    public enum RequestType { TEXTPLAIN, JSON, STREAM }

    /// <summary>
    /// 
    /// </summary>
    public class OAuthToken 
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly OAuthToken Empty;

        /// <summary>
        /// 
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// 
        /// </summary>
        static OAuthToken()
        {
            Empty = new OAuthToken(String.Empty, String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        public OAuthToken(string token, string secret)
        {
            Token = token;
            Secret = secret;
        }
    }

    

    /// <summary>
    /// 
    /// </summary>
    public class OAuthConsumer : OAuthBase
    {
        /// <summary>
        /// 
        /// </summary>
        public string RequestTokenUrl { get; set; }

        /// <summary>
        /// Set the base url for authorization.
        /// Use <>AuthorizationUrl</>
        /// </summary>
        public string AuthorizationUrlBase { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AccessTokenUrl { get; set; }

        /// <summary>
        /// The consumer key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The consumer secret.
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// A private copy of the request token.
        /// </summary>
        internal OAuthToken _RequestToken { get; set; }

        /// <summary>
        /// Returns the request token.
        /// </summary>
        /// <remarks>
        /// If the request token is empty, this accessor request the token.
        /// </remarks>
        public OAuthToken RequestToken { get { return _GetRequestToken(); } }

        /// <summary>
        /// A private copy of the access token.
        /// </summary>
        internal OAuthToken _AccessToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public OAuthToken AccessToken { get { return _GetAccessToken(); } set { _AccessToken = value; } }

        /// <summary>
        /// 
        /// </summary>
        public string AuthorizationUrl { get { return _BuildAuthorizationUrl(); } }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Secret"></param>
        public OAuthConsumer(string key, string secret)
        {
            _RequestToken = OAuthToken.Empty;
            _AccessToken = OAuthToken.Empty;

            Key = key;
            Secret = secret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual object Request(RequestMethod method, RequestType type, string url, List<QueryParameter> parameters)
        {
            switch (type)
            {
                case RequestType.TEXTPLAIN:
                    return _RequestTextPlain(method, url, parameters);
                case RequestType.JSON:
                    return _RequestJSON(method, url, parameters);
                case RequestType.STREAM:
                    return _RequestStream(method, url, parameters);
            }
            throw new Exception("Invalid RequestType.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private StreamReader _RequestStream(RequestMethod method, string url, List<QueryParameter> parameters)
        {
            Uri BaseUri = BuildUrlWithParams(url, parameters);

            //
            // 
            //
            OAuthToken Token = (AccessToken == OAuthToken.Empty) ? _RequestToken : AccessToken;

            string Nonce = GenerateNonce();
            string TimeStamp = GenerateTimeStamp();
            string normalizedParams;
            string normalizedUrl;

            //
            // Generate oauth signature
            //
            string signature = GenerateSignature(BaseUri, Key, Secret, Token.Token, Token.Secret, method.ToString(),
                TimeStamp, Nonce, out normalizedUrl, out normalizedParams);

            signature = Uri.EscapeDataString(signature).Replace("%20", "+");

            //
            UriBuilder requestUri = new UriBuilder(normalizedUrl);
            requestUri.Query = normalizedParams + "&oauth_signature=" + signature;


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri.Uri);
            request.Method = method.ToString();

            //
            //
            //
            if (method == RequestMethod.POST || method == RequestMethod.PUT)
            {
                request.ContentType = "application/x-www-form-urlencoded";

                using (Stream reqStream = request.GetRequestStream())
                {
                    byte[] postArray = Encoding.ASCII.GetBytes(BaseUri.Query);
                    reqStream.Write(postArray, 0, postArray.Length);
                }
            }

            return new StreamReader(request.GetResponse().GetResponseStream());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string _RequestTextPlain(RequestMethod method, string url, List<QueryParameter> parameters)
        {
            //
            using (StreamReader r = _RequestStream(method, url, parameters))
            {
                return r.ReadToEnd();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private object _RequestJSON(RequestMethod method, string url, List<QueryParameter> parameters)
        {
            return Json.Deserialize(_RequestTextPlain(method, url, parameters));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private OAuthToken _GetRequestToken()
        {
            //
            // Raise an exception if the Request token url is not set.
            //
            if (string.IsNullOrEmpty(RequestTokenUrl))
                throw new OAuthException("The RequestTokenUrl must be set.");


            if (_RequestToken != OAuthToken.Empty)
                return _RequestToken;

            //
            //
            //
            string response = (string)Request(RequestMethod.GET, RequestType.TEXTPLAIN, RequestTokenUrl, null);

            //
            //
            //
            Dictionary<string, string> query = _QueryStringToDictionary(response);
            
            //
            _RequestToken = new OAuthToken(query["oauth_token"], query["oauth_token_secret"]);

            //
            return _RequestToken;

        }

        private OAuthToken _GetAccessToken()
        {
            //
            // Raise an exception if the Request token url is not set.
            //
            if (string.IsNullOrEmpty(AccessTokenUrl))
                throw new OAuthException("The AccessTokenUrl must be set.");


            if (_AccessToken != OAuthToken.Empty)
                return _RequestToken;

            //
            //
            //
            string response = (string)Request(RequestMethod.GET, RequestType.TEXTPLAIN, AccessTokenUrl, null);

            //
            //
            //
            Dictionary<string, string> query = _QueryStringToDictionary(response);

            //
            _AccessToken = new OAuthToken(query["oauth_token"], query["oauth_token_secret"]);

            //
            return _AccessToken;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string _BuildAuthorizationUrl()
        {
            //
            // Raise an exception if the Request token url is not set.
            //
            if (string.IsNullOrEmpty(AuthorizationUrlBase))
                throw new OAuthException("The AuthorizationUrlBase must be set.");

            //
            //
            //
            return String.Format("{0}?oauth_token={1}", AuthorizationUrlBase, RequestToken.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private Uri BuildUrlWithParams(string url, List<QueryParameter> parameters)
        {
            UriBuilder uri = new UriBuilder(url);

            //
            // If the list it's empty, return the url.
            //
            if (parameters == null || parameters.Count == 0)
                return uri.Uri;


            //
            // Build the query string.
            //
            StringBuilder QueryString = new StringBuilder();

            //
            //
            //
            for (int i = 0; i < parameters.Count; i++)
            {
                if (i > 0)
                    QueryString.Append('&');

                //
                // Format as Key=Value.
                //
                QueryString.Append(parameters[i].Name + "=" + parameters[i].Value);
            }
            uri.Query = QueryString.ToString();

            return uri.Uri;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        private Dictionary<string, string> _QueryStringToDictionary(string q)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            
            string[] querySegments = q.Split('&');
            foreach (string segment in querySegments)
            {
                string[] parts = segment.Split('=');
                if (parts.Length > 0)
                {
                    string key = parts[0].Trim(new char[] { '?', ' ' });
                    string val = parts[1].Trim();

                    result.Add(key, val);
                }
            }
            return result;
        }
    }
}
