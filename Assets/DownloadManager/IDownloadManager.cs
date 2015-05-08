﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DHXDownloadManager
{ 
    public delegate void OnProgressUpdateDelegate(Manifest metadata, List<byte[]> bytes);
    public delegate void OnDownloadedStartedDelegate(Manifest metadata);
    public delegate void OnDownloadedFinishedDelegate(Manifest metadata);
    public delegate void OnDownloadedAddedDelegate(Manifest metadata);
    public delegate void OnDownloadingEndDelegate();
    public delegate void OnDownloadingStartDelegate();

    public interface IDownloadManager
    {
        event OnDownloadedAddedDelegate OnDownloadAdded;
        event OnDownloadingStartDelegate OnDownloadingStart;
        event OnDownloadingEndDelegate OnDownloadingEnd;
        void AddDownload(ref Manifest metadata);
    }
}