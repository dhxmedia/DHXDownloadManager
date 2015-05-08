﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using UnityEngine;
using System.Collections;
namespace DHXDownloadManager
{ 
#if USE_BESTHTTP
    public class ManagerSleep : MonoBehaviour {
	    ManagerBestHTTP _Manager;
	    // Use this for initialization
	    void Start () {
            _Manager = GetComponent<ManagerBestHTTP>();
		    _Manager.OnDownloadingStart += OnDownloadingStart;
		    _Manager.OnDownloadingEnd += OnDownloadingEnd;
	    }

	    void OnDownloadingStart()
	    {
		    Screen.sleepTimeout = SleepTimeout.NeverSleep;
	    }

	    void OnDownloadingEnd()
	    {
		    Screen.sleepTimeout = SleepTimeout.SystemSetting;
	    }
    }
#else

    public class DownloadManagerSleep : MonoBehaviour
    {
        void Start()
        {
            throw new System.DllNotFoundException("Not yet implemented for WWW. Please use DownloadEngineWWW or download BestHTTP and #define USE_BESTHTTP.");
        }
    }
#endif
}