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
using MiniJSON;

namespace Dropbox
{
    public class FileEntry
    {
        /// <summary>
        /// The file size in bytes.
        /// </summary>
        public long Bytes { get; set; }

        /// <summary>
        /// The last time the file was modified on Dropbox, in the standard date format (not 
        /// included for the root folder).
        /// </summary>
        public string Modified { get; set; }

        /// <summary>
        /// For files, this is the modification time set by the desktop client when the file was 
        /// added to Dropbox, in the standard date format. Since this time is not verified (the 
        /// Dropbox server stores whatever the desktop client sends up), this should only be 
        /// used for display purposes (such as sorting) and not, for example, to determine if 
        /// a file has changed or not.
        /// </summary>
        public string ClientModifiedTime { get; set; }

        /// <summary>
        /// A folder's hash is useful for indicating changes to the folder's contents in later 
        /// calls to /metadata. This is roughly the folder equivalent to a file's rev.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// The root or top-level folder depending on your access level. All paths returned 
        /// are relative to this root level. Permitted values are either dropbox or app_folder.
        /// </summary>
        public string Root { get; set; }

        /// <summary>
        /// Returns the canonical path to the file or directory.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///  A human-readable description of the file size (translated by locale).
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// The name of the icon used to illustrate the file type in Dropbox's icon library.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The file mime type.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// A deprecated field that semi-uniquely identifies a file. Use RevisionId instead.
        /// </summary>
        public long Revision { get; set; }

        /// <summary>
        /// A unique identifier for the current revision of a file. This field is the same rev as 
        /// elsewhere in the API and can be used to detect changes and avoid conflicts.
        /// </summary>
        public string RevisionId { get; set; }

        /// <summary>
        /// True if the file is an image can be converted to a thumbnail via the /thumbnails call.
        /// </summary>
        public bool ThumbnailExists { get; set; }

        /// <summary>
        /// Whether the given entry is a folder or not.
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Whether the given entry is deleted (only included if deleted files are being returned).
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        ///
        /// </summary>
        public List<FileEntry> Childs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public FileEntry()
        {
        }

        /// <summary>
        /// Create file entry object from json code.
        /// </summary>
        /// <param name="json"></param>
        public FileEntry(JsonDictionary json)
        {
            Childs = new List<FileEntry>();

            Bytes = json.FindValue<long>("bytes");
            Modified = json.FindValue<string>("modified");
            ClientModifiedTime = json.FindValue<string>("client_mtime");
            Hash = json.FindValue<string>("hash");
            Root = json.FindValue<string>("root");
            Path = json.FindValue<string>("path");
            Size = json.FindValue<string>("size");
            Icon = json.FindValue<string>("icon");
            MimeType = json.FindValue<string>("mime_type");
            Revision = json.FindValue<long>("revision");
            RevisionId = json.FindValue<string>("rev");
            ThumbnailExists = json.FindValue<bool>("thumb_exists");
            IsFolder = json.FindValue<bool>("is_dir");
            IsDeleted = json.FindValue<bool>("is_deleted");

            // 
            //
            //
            if (json.ContainsKey("contents"))
            {
                List<Object> content_list = json.FindValue<List<Object>>("contents");

                //
                // Iterate through content_list and add it
                //
                foreach (JsonDictionary content in content_list)
                {
                    Childs.Add(new FileEntry(content));
                }
            }
        }
    }
}
