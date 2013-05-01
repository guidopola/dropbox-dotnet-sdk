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
using System.IO;
using OAuth;
using MiniJSON;

namespace Dropbox
{
    /// <summary>
    /// 
    /// </summary>
    public class Client
    {
        /// <summary>
        /// 
        /// </summary>
        protected Session session;

        /// <summary>
        /// https://www.dropbox.com/developers/core/api#account-info
        /// </summary>
        public AccountInfo AccountInfo { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        public Client(Session s)
        {
            session = s;

            //
            //
            //
            AccountInfo = new AccountInfo(session);
        }

        /// <summary>
        /// Create a folder in the user's dropbox account.
        /// </summary>
        /// <param name="path">The path where the folder will be created. [NOTE] Start with a slash '/'</param>
        /// <returns>The newly created FileEntry</returns>
        public FileEntry CreateFolder(string path)
        {
            List<QueryParameter> ParamList = new List<QueryParameter>();

            //
            //
            //
            ParamList.Add(new QueryParameter("root", session.GetAccessType()));
            ParamList.Add(new QueryParameter("path", path));
            ParamList.Add(new QueryParameter("locale", session.Locale.ToString()));

            JsonDictionary json = (JsonDictionary)session.Request(RequestMethod.POST, RequestType.JSON,
                session.FormatAPIServerUrl("/fileops/create_folder"), ParamList);

            //
            //
            //
            FileEntry entry = new FileEntry(json);


            return entry;
        }

        /// <summary>
        /// https://www.dropbox.com/developers/reference/api#metadata
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="FileLimit"></param>
        /// <param name="ListChildren"></param>
        /// <param name="Hash"></param>
        /// <param name="RevisionId"></param>
        /// <returns></returns>
        public FileEntry EntryMetaData(string Path, int FileLimit, bool ListChildren, string Hash, string RevisionId)
        {
            //
            // @TODO: Create function FormatPath or similar
            //
            if (Path.EndsWith("/"))
                Path.Remove(Path.Length - 1);

            List<QueryParameter> ParamList = new List<QueryParameter>();

            ParamList.Add(new QueryParameter("file_limit", FileLimit.ToString()));
            ParamList.Add(new QueryParameter("list", ListChildren.ToString()));

            if(!String.IsNullOrEmpty(Hash))
                ParamList.Add(new QueryParameter("hash ", Hash));

            if (!String.IsNullOrEmpty(RevisionId))
                ParamList.Add(new QueryParameter("rev ", RevisionId));


            JsonDictionary json = (JsonDictionary)session.Request(RequestMethod.GET, RequestType.JSON,
                session.FormatAPIServerUrl(String.Format("/metadata/{0}{1}", 
                    session.GetAccessType(), Path)), ParamList);

            //
            //
            //
            FileEntry entry = new FileEntry(json);

            return entry;
        }

        public StreamReader DownloadFile(string Path)
        {
            if (Path.EndsWith("/"))
                Path.Remove(Path.Length - 1);

            StreamReader r = (StreamReader)session.Request(RequestMethod.GET, RequestType.STREAM,
                session.FormatContentUrl(String.Format("/files/{0}{1}", session.GetAccessType(), Path)), null);

            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Cursor"></param>
        /// <returns></returns>
        public DeltaPage RequestDelta(string Cursor = null)
        {
            List<QueryParameter> parameters = new List<QueryParameter>();

            if( !String.IsNullOrEmpty(Cursor))
                parameters.Add(new QueryParameter("cursor", Cursor));

            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));

            JsonDictionary json = (JsonDictionary)session.Request(RequestMethod.POST, RequestType.JSON, 
                session.FormatAPIServerUrl("/delta"), parameters);

            //
            //
            return new DeltaPage(json);
        }
    }
}
