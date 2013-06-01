using System;
using System.Collections.Generic;
using System.Text;

namespace Dropbox
{
    /// <summary>
    /// 
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static string SafeFilePath(string Path)
        {
            string SafePath = Path;

            if (!SafePath.StartsWith("/"))
                SafePath.Insert(0, "/");

            if (SafePath.EndsWith("/"))
                SafePath.Remove(SafePath.Length - 1);

            return SafePath;
        }
    }
}
