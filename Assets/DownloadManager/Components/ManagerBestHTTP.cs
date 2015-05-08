﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
namespace DHXDownloadManager
{ 
#if USE_BESTHTTP
    public class ManagerBestHTTP : ManagerComponent<DownloadEngineBestHTTP> {
    }
#else

    public class ManagerBestHTTP : MonoBehaviour
    {
        void Start()
        {
            throw new System.DllNotFoundException("Using empty DownloadManagerBestHTTP. Please use DownloadEngineWWW or download BestHTTP and #define USE_BESTHTTP.");
        }
    }
#endif
}