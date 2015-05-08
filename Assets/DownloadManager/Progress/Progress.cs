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

        public event System.Action<Progress> OnProgressStart;
        public event System.Action<Progress> OnProgressEnd;
        int _DownloadingCount = 0;
        int _TotalBytes = 0;
        int _DownloadedBytes = 0;

        public int DownloadedBytes { get { return _DownloadedBytes; } }
        public int TotalBytes { get { return _TotalBytes; } }

	    // Use this for initialization
        public Progress()
        {

	    }
	
	    void OnDownloadingStart()
	    {
		    _Downloading = true;
            if (OnPercentChange != null)
                OnPercentChange(0, 0, 0);
            if (OnProgressStart != null)
                OnProgressStart(this);
	    }
	
	    void OnDownloadingEnd()
	    {
            Clear();
            if (OnPercentChange != null)
                OnPercentChange(1.0f, _DownloadedBytes, _TotalBytes);
            if (OnProgressEnd != null)
                OnProgressEnd(this);
	    }

        public void AddDownload(Manifest metadata)
	    {
            if (_Manifests.Contains(metadata) == false)
            {
                if (metadata.Status != Manifest.StatusFlags.Destroyed
                    && metadata.Status != Manifest.StatusFlags.Finished
                    && metadata.Status != Manifest.StatusFlags.Failed)
                {
                    _Manifests.Add(metadata);
                    metadata.OnDownloadedBytes += OnDownloadedBytes;
                    metadata.OnStatusChanged += metadata_OnStatusChanged;
                    metadata_OnStatusChanged(metadata, true);
                }
            }
	    }

        void metadata_OnStatusChanged(Manifest obj, bool firstLoad)
        {

            if (obj.Status == Manifest.StatusFlags.Destroyed
                || obj.Status == Manifest.StatusFlags.Finished
                || obj.Status == Manifest.StatusFlags.Failed)
            {
                _DownloadingCount--;
            }

            // This was something that was downloading, but is now set to None status (used in Retry)
            if(firstLoad == false)
            {
                if (obj.Status == Manifest.StatusFlags.None)
                {
                    _DownloadingCount--;
                }
            }
            if (obj.Status == Manifest.StatusFlags.Queued)
            {
                _DownloadingCount++;
                if (_Downloading == false)
                    OnDownloadingStart();
            }
            if (_DownloadingCount <= 0)
            {
                if (_Downloading == true)
                    OnDownloadingEnd();
            }
        }

        void metadata_OnStatusChanged(Manifest obj)
        {
            metadata_OnStatusChanged(obj, false);
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
                manifest.OnStatusChanged -= metadata_OnStatusChanged;
                manifest.OnDownloadedBytes -= OnDownloadedBytes;
            }
            _Manifests.Clear();
        }

    }
}