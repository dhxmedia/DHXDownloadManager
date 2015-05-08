﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
namespace DHXDownloadManager
{
    /// <summary>
    /// The component to attach to a downloadmanager to track overall progress.
    /// There only really needs to be one instance of this in a scene
    /// </summary>
    public class ProgressComponent : MonoBehaviour
    {
        public GameObject DownloadManager;
        IDownloadManager _DownloadManager;
        ManagerProgress _Progress;

        public Progress.OnPercentChangeDelegate OnPercentChange;
        // Use this for initialization
        void Awake()
        {
            MonoBehaviour[] components = DownloadManager.GetComponents<MonoBehaviour>();
            for(int i = 0; i < components.Length; i++)
            {
                if(components[i] is IDownloadManager)
                {
                    _DownloadManager = (IDownloadManager)components[i];
                    _Progress = new ManagerProgress(_DownloadManager);
                    _Progress.OnPercentChange += _Progress_OnPercentChange;
                    break;
                }
            }
        }

        void _Progress_OnPercentChange(float percent, int totalBytesDownloaded, int totalBytes)
        {
            if (OnPercentChange != null)
                OnPercentChange(percent, totalBytesDownloaded, totalBytes);
        }

    }
}