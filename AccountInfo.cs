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
using System.Text;
using OAuth;
using MiniJSON;

namespace Dropbox
{
    /// <summary>
    /// 
    /// </summary>
    public class AccountInfo
    {
        private Session session;

        /// <summary>
        /// Return the user's unique id.
        /// </summary>
        public long UserId { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string Country { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public QuotaInfo Quota { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        public AccountInfo(Session s)
        {
            session = s;
            Quota = new QuotaInfo();
            Update();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            JsonDictionary json = (JsonDictionary)session.Request(RequestMethod.GET, RequestType.JSON,
                session.FormatAPIServerUrl("/account/info"), null);

            //
            //
            //
            UserId = json.FindValue<long>("uid");
            Email = json.FindValue<string>("email");
            DisplayName = json.FindValue<string>("display_name");
            Country = json.FindValue<string>("country");

            //
            //
            //
            JsonDictionary quota_info = json.FindValue<JsonDictionary>("quota_info");
            Quota.Normal = quota_info.FindValue<long>("normal");
			Quota.Shared = quota_info.FindValue<long>("shared");
			Quota.Total = quota_info.FindValue<long>("quota");
        }
    }
}
