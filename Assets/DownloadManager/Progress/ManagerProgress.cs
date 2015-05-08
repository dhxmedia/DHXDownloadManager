﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;


using System.Collections;
using System.Collections.Generic;
using System;
namespace DHXDownloadManager
{
    public class ManagerProgress
    {

        public IDownloadManager DownloadManager;

        bool _Downloading = false;
        HashSet<Manifest> _Manifests = new HashSet<Manifest>();

        public delegate void OnPercentChangeDelegate(float percent, int totalBytesDownloaded, int totalBytes);
        public event OnPercentChangeDelegate OnPercentChange;

        int _TotalBytes = 0;
        int _DownloadedBytes = 0;
        // Use this for initialization
        public ManagerProgress(IDownloadManager manager)
        {
            DownloadManager = manager;
            DownloadManager.OnDownloadAdded += OnDownloadAdded;
            DownloadManager.OnDownloadingStart += OnDownloadingStart;
            DownloadManager.OnDownloadingEnd += OnDownloadingEnd;
        }

        ~ManagerProgress()
        {
            if (DownloadManager != null)
            {
                DownloadManager.OnDownloadAdded -= OnDownloadAdded;
                DownloadManager.OnDownloadingStart -= OnDownloadingStart;
                DownloadManager.OnDownloadingEnd -= OnDownloadingEnd;
            }
        }


        void OnDownloadingStart()
        {
            _Downloading = true;
            if (OnPercentChange != null)
                OnPercentChange(0, 0, 0);
        }

        void OnDownloadingEnd()
        {
            _Downloading = false;
            _Manifests.Clear();
            if (OnPercentChange != null)
                OnPercentChange(1.0f, 0, 0);
        }

        void OnDownloadAdded(Manifest metadata)
        {
            if (_Manifests.Contains(metadata) == false)
            {
                _Manifests.Add(metadata);
                metadata.OnDownloadedBytes += OnDownloadedBytes;
            }
        }

        void OnDownloadedBytes(Manifest metadata, int _bytesDownloaded, int _bytesTotal)
        {
            _TotalBytes = 0;
            _DownloadedBytes = 0;
            foreach (Manifest manifest in _Manifests)
            {
                _TotalBytes += manifest.TotalBytesSize;
                _DownloadedBytes += manifest.BytesDownloaded;
            }
            float percent = 0;
            if(_TotalBytes!= 0)
            {
                percent = (float)_DownloadedBytes / (float)_TotalBytes;
            }
             
            if (OnPercentChange != null)
                OnPercentChange(percent, _DownloadedBytes, _TotalBytes);
        }

    }
}