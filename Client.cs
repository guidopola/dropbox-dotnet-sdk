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
    /// Defines the dropbox api client.
    /// </summary>
    public class Client
    {
        public const int kFileLimit = 25000;
        public const int kFileLimitSearch = 1000;
        public const int kRevisionsLimit = 1000;

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
            AccountInfo = new AccountInfo(session);
        }

        /// <summary>
        /// Create a folder in the user's dropbox account.
        /// </summary>
        /// <param name="path">The path where the folder will be created.</param>
        /// <remarks>Path must start with a slash '/'</remarks>
        /// <returns>The newly created FileEntry</returns>
        public FileEntry CreateFolder(string path)
        {
            List<QueryParameter> parameters = new List<QueryParameter>();
            path = Util.SafeFilePath(path);

            parameters.Add(new QueryParameter("root", session.AccessType));
            parameters.Add(new QueryParameter("path", path));
            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));

            return new FileEntry((JsonDictionary)session.Request(RequestMethod.POST, RequestType.JSON,
                session.FormatAPIServerUrl("/fileops/create_folder"), parameters));
        }

        /// <summary>
        /// Copies a file or folder to a new location.
        /// </summary>
        /// <param name="PathOrRef">Either the file or folder to be copied from relative to root or the Reference code
        /// created from a previous call to <see cref="Dropbox.Client.CopyReference"/>.</param>
        /// <param name="Destination">Specifies the destination path, including the new name for the file or folder, 
        /// relative to root.</param>
        /// <param name="CopyFromRef">We are copying from a reference or a path?</param>
        /// <returns>FileEntry for the copy of the file or folder</returns>
        public FileEntry CopyFile(string PathOrRef, string Destination, bool CopyFromRef) 
        {
            List<QueryParameter> parameters = new List<QueryParameter>();
            
            if(!CopyFromRef)
                PathOrRef = Util.SafeFilePath(PathOrRef);

            parameters.Add(new QueryParameter("root", session.AccessType));
            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));
            parameters.Add(new QueryParameter("to_path", Destination));
            parameters.Add(new QueryParameter((CopyFromRef ? "from_copy_ref" : "from_path"), PathOrRef));

            return new FileEntry((JsonDictionary)session.Request(RequestMethod.POST, RequestType.JSON,
                session.FormatAPIServerUrl("/fileops/copy"), parameters));
        }

        /// <summary>
        /// Copies a file or folder to a new location.
        /// </summary>
        /// <param name="Path">The file or folder to be copied from relative to root</param>
        /// <param name="Destination">Specifies the destination path, including the new name for the file or folder, 
        /// relative to root.</param>
        /// <returns>FileEntry for the copy of the file or folder</returns>
        public FileEntry CopyFile(string Path, string Destination)
        {
            return CopyFile(Path, Destination, false);
        }

        /// <summary>
        /// Deletes a file or folder.
        /// </summary>
        /// <param name="Path">The path to the file or folder to be deleted.</param>
        /// <returns>FileEntry for the deleted file or folder.</returns>
        public FileEntry RemoveFile(string Path)
        {
            List<QueryParameter> parameters = new List<QueryParameter>();
            Path = Util.SafeFilePath(Path);

            parameters.Add(new QueryParameter("root", session.AccessType));
            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));
            parameters.Add(new QueryParameter("path", Path));

            return new FileEntry((JsonDictionary)session.Request(RequestMethod.POST, RequestType.JSON,
                session.FormatAPIServerUrl("/fileops/delete"), parameters));
        }

        /// <summary>
        /// Moves a file or folder to a new location.
        /// </summary>
        /// <param name="Path">Specifies the file or folder to be moved from relative to root.</param>
        /// <param name="Destination">Specifies the destination path, including the new name for the file or folder, 
        /// relative to root.</param>
        /// <returns>FileEntry for the moved file or folder.</returns>
        public FileEntry MoveFile(string Path, string Destination)
        {
            List<QueryParameter> parameters = new List<QueryParameter>();
            Path = Util.SafeFilePath(Path);
            Destination = Util.SafeFilePath(Destination);

            parameters.Add(new QueryParameter("root", session.AccessType));
            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));
            parameters.Add(new QueryParameter("from_path", Path));
            parameters.Add(new QueryParameter("to_path", Destination));

            return new FileEntry((JsonDictionary)session.Request(RequestMethod.POST, RequestType.JSON,
                session.FormatAPIServerUrl("/fileops/move"), parameters));
        }

        /// <summary>
        /// Retrieves file and folder metadata.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="FileLimit"></param>
        /// <param name="ListChildren"></param>
        /// <param name="Hash"></param>
        /// <param name="RevisionId"></param>
        /// <returns></returns>
        public FileEntry EntryMetaData(string Path, int FileLimit, bool ListChildren, string Hash, string RevisionId)
        {
            if( FileLimit > kFileLimit )
                throw new DropboxException("FileLimit exceed 25,000.");

            List<QueryParameter> parameters = new List<QueryParameter>();
            Path = Util.SafeFilePath(Path);

            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));
            parameters.Add(new QueryParameter("file_limit", FileLimit.ToString()));
            parameters.Add(new QueryParameter("list", ListChildren.ToString()));

            if(!String.IsNullOrEmpty(Hash))
                parameters.Add(new QueryParameter("hash", Hash));

            if (!String.IsNullOrEmpty(RevisionId))
                parameters.Add(new QueryParameter("rev", RevisionId));

            return new FileEntry((JsonDictionary)session.Request(RequestMethod.GET, RequestType.JSON,
                session.FormatAPIServerUrl("/metadata/" + session.AccessType + Path), parameters));
        }

        /// <summary>
        /// Obtains a list of FileEntry for the previous revisions of a file.
        /// </summary>
        /// <param name="Path">The path to the file.</param>
        /// <param name="Limit">Default is 10. Max is 1,000. When listing a file, the service will not report listings 
        /// containing more than the amount specified and will instead respond with a 406 (Not Acceptable) status 
        /// response.</param>
        /// <returns>A list of all revisions.</returns>
        public List<FileEntry> FileRevisions(string Path, int Limit)
        {
            if (Limit > kRevisionsLimit)
                throw new DropboxException("Limit exceed 1,000.");

            List<FileEntry> result = new List<FileEntry>();
            List<QueryParameter> parameters = new List<QueryParameter>();
            Path = Util.SafeFilePath(Path);

            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));
            parameters.Add(new QueryParameter("path", Path));
            parameters.Add(new QueryParameter("rev_limit", Limit.ToString()));

            List<Object> json = (List<Object>)session.Request(RequestMethod.GET, RequestType.JSON,
                session.FormatAPIServerUrl("/revisions/" + session.AccessType + Path), parameters);

            foreach (JsonDictionary metadata in json)
            {
                result.Add(new FileEntry(metadata));
            }
            return result;
        }
        
        /// <summary>
        /// Obtains a list of FileEntry for the previous revisions of a file.
        /// </summary>
        /// <param name="Path">The path to the file.</param>
        /// <returns>A list of all revisions.</returns>
        public List<FileEntry> FileRevisions(string Path)
        {
            return FileRevisions(Path, 10);
        }

        /// <summary>
        /// Restores a file path to a previous revision.
        /// 
        /// Unlike downloading a file at a given revision and then re-uploading it, this call is atomic. 
        /// It also saves a bunch of bandwidth.
        /// </summary>
        /// <param name="Path">The path to the file.</param>
        /// <param name="Revision">The revision of the file to restore.</param>
        /// <returns>The FileEntry of the restored file.</returns>
        public FileEntry RestoreFile(string Path, string Revision)
        {
            List<QueryParameter> parameters = new List<QueryParameter>();
            Path = Util.SafeFilePath(Path);

            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));
            parameters.Add(new QueryParameter("path", Path));
            parameters.Add(new QueryParameter("rev", Revision));

            return new FileEntry((JsonDictionary)session.Request(RequestMethod.POST, RequestType.JSON,
                session.FormatAPIServerUrl("/restore/" + session.AccessType + Path), parameters));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public StreamReader DownloadFile(string Path)
        {
            Path = Util.SafeFilePath(Path);

            StreamReader r = (StreamReader)session.Request(RequestMethod.GET, RequestType.STREAM,
                session.FormatContentUrl("/files/" + session.AccessType + Path), null);

            return r;
        }

        /// <summary>
        /// A way of letting you keep up with changes to files and folders in a user's Dropbox. 
        /// You can periodically call RequestDelta to get a list of "delta entries", which are instructions on how to 
        /// update your local state to match the server's state.
        /// </summary>
        /// <param name="Cursor">A string that is used to keep track of your current state</param>
        /// <returns>A DeltaPage object.</returns>
        public DeltaPage RequestDelta(string Cursor)
        {
            List<QueryParameter> parameters = new List<QueryParameter>();

            if (!String.IsNullOrEmpty(Cursor))
                parameters.Add(new QueryParameter("cursor", Cursor));

            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));

            //
            //
            //
            return new DeltaPage((JsonDictionary)session.Request(RequestMethod.POST, RequestType.JSON,
                session.FormatAPIServerUrl("/delta"), parameters));
        }

        /// <summary>
        /// A way of letting you keep up with changes to files and folders in a user's Dropbox. 
        /// You can periodically call RequestDelta to get a list of "delta entries", which are instructions on how to 
        /// update your local state to match the server's state.
        /// </summary>
        /// <returns>A DeltaPage object.</returns>
        public DeltaPage RequestDelta()
        {
            return RequestDelta(String.Empty);
        }

        /// <summary>
        /// Returns a list of FileEntry for all files and folders whose filename contains the given search string as a 
        /// substring.
        /// 
        /// Searches are limited to the folder path and its sub-folder hierarchy provided in the call.
        /// </summary>
        /// <param name="Path">The path to the folder you want to search from</param>
        /// <param name="Query">The search string. Must be at least three characters long</param>
        /// <param name="FileLimit">The maximum and default value is 1,000. No more than FileLimit search results will 
        /// be returned.</param>
        /// <param name="IncludeDeleted">If this parameter is set to true, then files and folders that have been 
        /// deleted will also be included in the search</param>
        /// <returns>List of FileEntry for any matching files and folders</returns>
        public List<FileEntry> Search(string Path, string Query, int FileLimit, bool IncludeDeleted)
        {
            List<FileEntry> result = new List<FileEntry>();
            List<QueryParameter> parameters = new List<QueryParameter>();
            Path = Util.SafeFilePath(Path);

            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));
            parameters.Add(new QueryParameter("query", Query));
            parameters.Add(new QueryParameter("file_limit", FileLimit.ToString()));
            parameters.Add(new QueryParameter("include_deleted", IncludeDeleted.ToString()));

            //
            //
            //
            List<Object> json = (List<Object>)session.Request(RequestMethod.POST, RequestType.JSON,
                session.FormatAPIServerUrl("/search/" + session.AccessType + Path), parameters);

            //
            // Iterate through json and add it
            //
            foreach (JsonDictionary metadata in json)
            {
                result.Add(new FileEntry(metadata));
            }
            return result;
        }

        /// <summary>
        /// Returns a list of FileEntry for all files and folders whose filename contains the given search string as a 
        /// substring.
        /// 
        /// Searches are limited to the folder path and its sub-folder hierarchy provided in the call.
        /// </summary>
        /// <param name="Path">The path to the folder you want to search from</param>
        /// <param name="Query">The search string. Must be at least three characters long</param>
        /// <param name="FileLimit">The maximum and default value is 1,000. No more than FileLimit search results will 
        /// be returned.</param>
        /// <returns>List of FileEntry for any matching files and folders</returns>
        public List<FileEntry> Search(string Path, string Query, int FileLimit)
        {
            return Search(Path, Query, FileLimit, false);
        }

        /// <summary>
        /// Returns a list of FileEntry for all files and folders whose filename contains the given search string as a 
        /// substring.
        /// 
        /// Searches are limited to the folder path and its sub-folder hierarchy provided in the call.
        /// </summary>
        /// <param name="Path">The path to the folder you want to search from</param>
        /// <param name="Query">The search string. Must be at least three characters long</param>
        /// <returns>List of FileEntry for any matching files and folders</returns>
        public List<FileEntry> Search(string Path, string Query)
        {
            return Search(Path, Query, kFileLimitSearch);
        }
        
        /// <summary>
        /// Creates and returns a Dropbox link to files or folders users can use to view a preview of the file in a 
        /// web browser.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="ShortUrl">When true (default), the url returned will be shortened using the Dropbox url 
        /// shortener. If false, the url will link directly to the file's preview page.</param>
        /// <returns>A Dropbox link to the given path. The link can be used publicly and directs to a preview page of 
        /// the file</returns>
        public string CreateShareLink(string Path, bool ShortUrl)
        {
            List<QueryParameter> parameters = new List<QueryParameter>();
            Path = Util.SafeFilePath(Path);

            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));
            parameters.Add(new QueryParameter("short_url", ShortUrl.ToString()));

            JsonDictionary json = (JsonDictionary)session.Request(RequestMethod.POST, RequestType.JSON,
                session.FormatAPIServerUrl("/shares/" + session.AccessType + Path), parameters);

            return json.FindValue<string>("url");
        }

        /// <summary>
        /// Creates and returns a Dropbox link to files or folders users can use to view a preview of the file in a 
        /// web browser.
        /// </summary>
        /// <param name="Path"></param>
        /// <returns>A Dropbox link to the given path. The link can be used publicly and directs to a preview page of 
        /// the file</returns>
        public string CreateShareLink(string Path)
        {
            return CreateShareLink(Path, true);
        }

        /// <summary>
        /// Similar to CreateShareLink. The difference is that this bypasses the Dropbox webserver, used to provide a 
        /// preview of the file, so that you can effectively stream the contents of your media.
        /// </summary>
        /// <remarks>The /media link expires after four hours, allotting enough time to stream files, but not enough to
        /// leave a connection open indefinitely.</remarks>
        /// <param name="Path">The path to the media file you want a direct link to.</param>
        /// <returns>Returns a link directly to a file.</returns>
        public string MediaLink(string Path)
        {
            List<QueryParameter> parameters = new List<QueryParameter>();
            Path = Util.SafeFilePath(Path);

            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));

            //
            //
            //
            JsonDictionary json = (JsonDictionary)session.Request(RequestMethod.POST, RequestType.JSON,
                session.FormatAPIServerUrl("/media/" + session.AccessType + Path), parameters);

            return json.FindValue<string>("url");
        }

        /// <summary>
        /// Creates and returns a copy_ref to a file. This reference string can be used to copy that file to another 
        /// user's Dropbox by passing it in as the from_copy_ref parameter on <seealso cref="Dropbox.Client.CopyFile"/>
        /// </summary>
        /// <param name="Path">The path to the file you want a copy reference to refer to.</param>
        /// <returns>A copy reference to the specified file</returns>
        public string CopyReference(string Path)
        {
            List<QueryParameter> parameters = new List<QueryParameter>();
            Path = Util.SafeFilePath(Path);

            parameters.Add(new QueryParameter("locale", session.Locale.ToString()));

            //
            //
            //
            JsonDictionary json = (JsonDictionary)session.Request(RequestMethod.GET, RequestType.JSON,
                session.FormatAPIServerUrl("/copy_ref/" + session.AccessType + Path), parameters);

            return json.FindValue<string>("copy_ref");
        }
    }
}
