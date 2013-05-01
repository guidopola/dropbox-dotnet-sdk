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
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using OAuth;

namespace Dropbox
{
    /// <summary>
    /// 
    /// </summary>
    public class Session : OAuthConsumer
    {
        private const string API_SERVER = "api.dropbox.com";
        private const string CONTENT_SERVER = "api-content.dropbox.com";
        private const string WEB_SERVER = "www.dropbox.com";
        private const int API_VERSION = 1;

        /// <summary>
        /// 
        /// </summary>
        public readonly AccessType AppAccess;

        /// <summary>
        /// 
        /// </summary>
        public CultureInfo Locale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ApplicationKey"></param>
        /// <param name="ApplicationSecret"></param>
        /// <param name="Access"></param>
        public Session(string ApplicationKey, string ApplicationSecret, AccessType Access)
            : base(ApplicationKey, ApplicationSecret)
        {
            Locale = new CultureInfo("en", false);
            AppAccess = Access;

            //
            //
            //
            base.RequestTokenUrl = "https://api.dropbox.com/1/oauth/request_token";
            base.AuthorizationUrlBase = "https://www.dropbox.com/1/oauth/authorize";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="type"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object Request(RequestMethod method, RequestType type, string url, List<QueryParameter> data)
        {
            try
            {
                return base.Request(method, type, url, data);
            }
            catch (WebException e)
            {
                throw new DropboxException(e);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string FormatAPIServerUrl(string path)
        {
            return String.Format("https://{0}/{1}{2}", 
                API_SERVER, API_VERSION, path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string FormatContentUrl(string path)
        {
            return String.Format("https://{0}/{1}{2}",
                CONTENT_SERVER, API_VERSION, path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetAccessType()
        {
            return AppAccess == AccessType.DropboxFolder ? 
                "dropbox" : "sandbox";
        }
    }
}
