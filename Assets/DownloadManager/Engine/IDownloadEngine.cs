﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
namespace DHXDownloadManager
{ 
    /// <summary>
    /// The interface for a DownloadEngine
    /// Responsible for performining the conversion between Manifest to actual download
    /// </summary>
    public interface IDownloadEngine {
        event OnDownloadedFinishedDelegate OnEngineDownloadFailed;
        event OnDownloadedFinishedDelegate OnEngineDownloadFinished;
        void PerformDownload(Manifest manifest);
        void Abort(Manifest manifest);
    }
}