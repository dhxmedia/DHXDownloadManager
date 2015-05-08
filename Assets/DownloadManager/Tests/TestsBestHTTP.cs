﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
namespace DHXDownloadManager.Tests
{ 
#if USE_BESTHTTP
    public class TestsBestHTTP : Tests<DHXDownloadManager.DownloadEngineBestHTTP> {

    }
#else

    public class TestsBestHTTP : MonoBehaviour
    {
        void Start()
        {
            throw new System.DllNotFoundException("Using empty DownloadManagerTestsBestHTTP. Please use DownloadEngineWWW or download BestHTTP and #define USE_BESTHTTP.");
        }
    }
#endif
}