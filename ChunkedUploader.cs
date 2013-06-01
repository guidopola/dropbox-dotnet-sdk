using MiniJSON;
using OAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dropbox
{
    public class ChunkedUploader
    {
        /// <summary>
        /// 
        /// </summary>
        private const int kChunkBufferSize = 4 * 1024 * 1024;

        /// <summary>
        /// 
        /// </summary>
        private Session Session { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public FileStream File { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UploadId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long UploadOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="totalSize"></param>
        public ChunkedUploader(Session s, FileStream file,  long totalSize)
        {
            Session = s;
            File = file;
            TotalSize = totalSize;
        }

        public void Upload()
        {
            byte[] ChunkBuffer = new byte[kChunkBufferSize];

            while (UploadOffset < TotalSize)
            {
                File.Read(ChunkBuffer, 0, kChunkBufferSize);

                UploadChunk(ChunkBuffer);
            }
        }

        private void UploadChunk(byte[] ChunkBuffer)
        {
            List<QueryParameter> parameters = new List<QueryParameter>();

            parameters.Add(new QueryParameter("offset", UploadOffset.ToString()));

            //
            //
            //
            if(!String.IsNullOrEmpty(UploadId))
                parameters.Add(new QueryParameter("upload_id", UploadId));

            //
            //
            //
            JsonDictionary json = (JsonDictionary)Session.Request(RequestMethod.PUT, RequestType.JSON,
                Session.FormatContentUrl("/chunked_upload"), parameters, ChunkBuffer);


            UploadId = json.FindValue<string>("upload_id");

            if( json.FindValue<long>("offset") > UploadOffset )
                UploadOffset += (json.FindValue<long>("offset") - UploadOffset);
        }
    }
}
