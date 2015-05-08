﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections;
using System.Collections.Generic;
using System;
namespace DHXDownloadManager
{
    /// <summary>
    /// Aggregates Manifest progress into a single point
    /// </summary>
    public class Progress
    {
        
	    bool _Downloading = false;
        HashSet<Manifest> _Manifests = new HashSet<Manifest>();

	    public delegate void OnPercentChangeDelegate(float percent, int totalBytesDownloaded, int totalBytes);
        public event OnPercentChangeDelegate OnPercentChange;

        int _TotalBytes = 0;
        int _DownloadedBytes = 0;

        public int DownloadedBytes { get { return _DownloadedBytes; } }
        public int TotalBytes { get { return _TotalBytes; } }

	    // Use this for initialization
        public Progress()
        {

	    }
	
	
        public void AddDownload(Manifest metadata)
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
		    foreach(Manifest manifest in _Manifests)
            {
                _TotalBytes += manifest.TotalBytesSize;
                _DownloadedBytes += manifest.BytesDownloaded;
            }

            float percent = (float)_DownloadedBytes / (float)_TotalBytes;

            if (OnPercentChange != null)
                OnPercentChange(percent, _DownloadedBytes, _TotalBytes);
	    }

        public void Clear()
        {
            _Downloading = false;
            foreach (Manifest manifest in _Manifests)
            {
                manifest.OnDownloadedBytes -= OnDownloadedBytes;
            }
            _Manifests.Clear();
        }

    }
}