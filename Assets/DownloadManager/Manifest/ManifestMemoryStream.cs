﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
namespace DHXDownloadManager
{ 
    /// <summary>
    /// MemoryStream implementation of ManifestStream
    /// </summary>
    [System.Serializable]
    [DataContract]
    public class ManifestMemoryStream : ManifestStream
    {
        public ManifestMemoryStream()
            : base()
        {

        }

        public ManifestMemoryStream(string url, Flags flag)
            : base(url, flag)
        {

        }
        override protected Stream CreateStream()
        {
            Stream stream = new MemoryStream();
            return stream;
        }
    }
}