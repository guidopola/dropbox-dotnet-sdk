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
    public class DeltaPage
    {
        /// <summary>
        /// 
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        public delegate void EntryChangedDelegate(DeltaEntry entry);

        /// <summary>
        /// 
        /// </summary>
        public EntryChangedDelegate OnEntryChanged { get; set; }

        /// <summary>
        ///  If true, clear your local state before processing the delta entries. reset is always true on 
        ///  the initial call to /delta (i.e. when no cursor is passed in). Otherwise, it is true in rare 
        ///  situations, such as after server or account maintenance, or if a user deletes their app folder.
        /// </summary>
        public bool Reset { get; set; }

        /// <summary>
        /// A string that encodes the latest information that has been returned. 
        /// On the next call to /delta, pass in this value.
        /// </summary>
        public string Cursor { get; set; }

        /// <summary>
        /// A list of "delta entries"
        /// </summary>
        public List<DeltaEntry> Entries { get; set; }

        /// <summary>
        /// If true, then there are more entries available; you can call /delta again immediately 
        /// to retrieve those entries. If 'false', then wait for at least five minutes (preferably longer) 
        /// before checking again.
        /// </summary>
        public bool HasMore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DeltaPage()
        {
            Entries = new List<DeltaEntry>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        public DeltaPage(JsonDictionary json)
        {
            Entries = new List<DeltaEntry>();
            Reset = json.FindValue<bool>("reset");
            Cursor = json.FindValue<string>("cursor");
            HasMore = json.FindValue<bool>("has_more");

            //
            //
            //
            List<Object> entries = json.FindValue<List<Object>>("entries");
            foreach (List<Object> entry in entries)
            {
                Entries.Add(new DeltaEntry(entry));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            //
            //
            //

            //OnEntryChanged();
        }
    }
}
