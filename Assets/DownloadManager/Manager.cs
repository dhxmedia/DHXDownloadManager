﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DHXDownloadManager
{ 
    public class Manager<T> : IDownloadManager where T : IDownloadEngine, new()
    {
        public bool Verbose;
        public event OnDownloadedAddedDelegate OnDownloadAdded;
	    public event OnDownloadingStartDelegate OnDownloadingStart;
	    public event OnDownloadingEndDelegate OnDownloadingEnd;

	    bool _Processing = false;
	    int _DownloadingCount = 0;

        List<Manifest> _HighRequests = new List<Manifest>();
        List<Manifest> _LowRequests = new List<Manifest>();
	    public int MaxDownloadCount = 2;

        T _DownloadEngine;
        List<Manifest> _ActiveDownloads = new List<Manifest>();

        public Manager()
        {
            _DownloadEngine = new T();
            _DownloadEngine.OnEngineDownloadFailed += OnEngineDownloadFailed;
            _DownloadEngine.OnEngineDownloadFinished += OnEngineDownloadFinished;
        }

        void _StartDownload(Manifest request)
	    {
            if (Verbose)
                Debug.Log("DownloadManager::_StartDownload: " + request.URL);
            _DownloadEngine.PerformDownload(request);
	    }

        void Retry(Manifest metadata)
        {
            if (Verbose)
                Debug.Log("DownloadManager::Retry: " + metadata.URL + ":" + _DownloadingCount + ":" + metadata.Attempts);
            metadata.OnAbort -= request_OnAbort;
            metadata.__Retry();
            _ActiveDownloads.Remove(metadata);
            _DownloadingCount--;
            AddDownload(metadata);
        }

        void ClearDownload(Manifest metadata)
        {
            if (Verbose)
                Debug.Log("DownloadManager::ClearDownload: " + metadata.URL + ":" + _DownloadingCount + ":" + metadata.IsActive);

            _ActiveDownloads.Remove(metadata);
            metadata.__ClearManifest();
            _DownloadingCount--;
	    }


	    void StartNextDownload()
        {
            if (Verbose)
                Debug.Log("DownloadManager::StartNextDownload: " + _LowRequests.Count + ":" + _HighRequests.Count + ":" + _DownloadingCount);
            if (_LowRequests.Count > 0 || _HighRequests.Count > 0)
		    {
                if (_DownloadingCount < MaxDownloadCount)
                {
                    Manifest request = null;
                    if (_HighRequests.Count > 0)
                    {
                        request = _HighRequests[0];
                        _HighRequests.RemoveAt(0);
                    }
                    else
                    {
                        request = _LowRequests[0];
                        _LowRequests.RemoveAt(0);
                    }
                    if (request.Status == Manifest.StatusFlags.Queued || request.Status == Manifest.StatusFlags.None)
                    {
                        _DownloadingCount++;
                        _ActiveDownloads.Add(request);
                        request.OnAbort += request_OnAbort;
			            _StartDownload(request);
                    }
                    else
                    {
                        StartNextDownload();
                    }
                }
		    }
		    else
		    {
                // Done when downloading _DownloadingCount hits 0
                if (_DownloadingCount <= 0)
                {
                    if (Verbose)
			            Debug.Log ("End DL");
                    if (_ActiveDownloads.Count != 0)
                        throw new System.Exception("ActiveDownloads.Count != 0");
			        if(OnDownloadingEnd != null)
			        {
				        OnDownloadingEnd();
			        }
			
			        _Processing = false;
                }
		    }
	    }

        void request_OnAbort(Manifest obj)
        {
            if (Verbose)
                Debug.Log("DownloadManager::request_OnAbort: " + obj.URL);
            _AbortRequest(obj);
        }

        public void AddDownload(Manifest metadata)
        {

            if (metadata.IsActive == false)
            {

                if (Verbose)
                    Debug.Log("DownloadManager::AddDownload: " + metadata.URL);
                try
                {
                    // Queue the manifest, and start it if we're not downloading it
                    // Or wait until we're done our next download if we are
                    metadata.__Queue(); 
                    if (metadata.Priority == Manifest.QueuePriority.Low)
                        _LowRequests.Add(metadata);
                    else
                        _HighRequests.Add(metadata);
                    if (_Processing == false)
                    {
                        _Processing = true;
                        if (Verbose)
                            Debug.Log("Start DL Queue " + _Processing.ToString());

                        if (OnDownloadingStart != null)
                        {
                            OnDownloadingStart();
                        }
                        if (OnDownloadAdded != null)
                        {
                            OnDownloadAdded(metadata);
                        }
                        StartNextDownload();
                    }
                    else
                    {
                        if (OnDownloadAdded != null)
                        {
                            OnDownloadAdded(metadata);
                        }
                        StartNextDownload();
                    }

                }
                catch(System.Exception e)
                {
                    Debug.LogError("Failed to AddDownload " + metadata.URL + ". " + e);
                }
                finally { }
            }

        }

        void _AbortRequest(Manifest manifest)
        {
            if (Verbose)
                Debug.Log("DownloadManager::request_OnAbort: " + manifest.URL);
            ClearDownload(manifest);
            _DownloadEngine.Abort(manifest);
            StartNextDownload();
        }
        
        void OnEngineDownloadFailed(Manifest manifest)
        {
            

            if (Verbose)
                Debug.Log("DownloadManager::OnEngineDownloadFailed: " + manifest.URL + ":" + _DownloadingCount);

            /// fail & retry
            if (manifest.HasFlag(Manifest.Flags.InfiniteRetry) == false)
                manifest.Attempts--;

            if (Verbose)
                Debug.Log(manifest.URL + ": DL Failed " + manifest.Attempts + " left");

            // Only do this part if it hasn't been done already
            if (manifest.IsActive)
            {
                
                if (manifest.Attempts <= 0)
                {
                    // Abort the download if we're out of attempts
                    manifest.Abort();
                }
                else
                {
                    // Retry the download if we have enough attempts left
                    Retry(manifest);
                }
            }
        }

        void OnEngineDownloadFinished(Manifest manifest)
        {
            if (Verbose)
                Debug.Log("DownloadManager::OnEngineDownloadFinished: " + manifest.URL);
            /// finish
            manifest.__Finish();
            ClearDownload(manifest);
            StartNextDownload();
        }

        public void Tick(float dt)
        {
            for(int i = 0; i < _ActiveDownloads.Count; i++)
            {
                _ActiveDownloads[i].Tick(dt);
            }
        }
    }

}