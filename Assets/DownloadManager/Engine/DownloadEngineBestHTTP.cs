﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if USE_BESTHTTP
using BestHTTP;
using BestHTTP.Caching;
#endif
namespace DHXDownloadManager
{ 
    #if USE_BESTHTTP
    /// <summary>
    /// BestHTTP implementation of the DownloadEngine
    /// </summary>
    public class DownloadEngineBestHTTP : IDownloadEngine
    {

        public event OnDownloadedFinishedDelegate OnEngineDownloadFailed;
        public event OnDownloadedFinishedDelegate OnEngineDownloadFinished;
        
        public void PerformDownload(Manifest manifest)
        {

            manifest.__Start();
            manifest.Ping();

            string url = manifest.URL;
            string realURL = System.Uri.EscapeUriString(url);
            HTTPManager.IsCachingDisabled = true;

            HTTPMethods methodType = HTTPMethods.Get;

            if (manifest.POSTFieldKVP.Count > 0)
            {
                methodType = HTTPMethods.Post;
            }
            HTTPRequest bestHTTPRequest = new HTTPRequest(new System.Uri(realURL), methodType, OnFragmentDownloaded);
            bestHTTPRequest.UseStreaming = true;
            bestHTTPRequest.DisableRetry = true;
            bestHTTPRequest.StreamFragmentSize = 1024;
            bestHTTPRequest.Tag = manifest;
            bestHTTPRequest.SetHeader("Accept-Encoding", "deflate");
            bestHTTPRequest.OnProgress += OnProgress;
            // if the request is a POST request, add all relevant fields
    
            if (manifest.POSTFieldKVP.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in manifest.POSTFieldKVP)
                {
                    bestHTTPRequest.AddField(kvp.Key, kvp.Value);
                }
            }
            manifest.EngineInstance = bestHTTPRequest;
            HTTPManager.SendRequest(bestHTTPRequest);
        }

        public void Abort(Manifest manifest)
        {

            HTTPRequest bestHTTPRequest = (HTTPRequest)manifest.EngineInstance;

            if (bestHTTPRequest != null)
            {
                bestHTTPRequest.Tag = null;
                if (bestHTTPRequest.State == HTTPRequestStates.Processing)
                    bestHTTPRequest.Abort();
            }
            manifest.EngineInstance = null;
        }

        void OnProgress(HTTPRequest req, int downloaded, int total)
        {
            Manifest metadata = (Manifest)req.Tag;
            if(metadata != null)
            {
                metadata.__UpdateBytes(downloaded, total);
            }
        }

        void OnFragmentDownloaded(HTTPRequest req, HTTPResponse resp)
        {
            Manifest metadata = (Manifest)req.Tag;

            // HTTP requests need to have a Manifest attached to them to be valid
            // might have happened if it was an early abort, before a connection was set
            if (metadata == null)
            {
                req.Abort();
                return;
            }

            if (resp != null)
                metadata.ResponseCode = resp.StatusCode;
            if (resp == null || resp.StatusCode >= 400 || req.Exception != null || req.Response == null)
            {
                metadata.SetError(ManifestErrors.UnknownDownloadError);
                if (OnEngineDownloadFailed != null)
                    OnEngineDownloadFailed(metadata);
                metadata.EngineInstance = null;

                req.Tag = null;
                return;
            }
            metadata.Ping();

            List<byte[]> downloadedFragments = resp.GetStreamedFragments();

            if (downloadedFragments != null)
            {
                metadata.__Update(downloadedFragments);
                downloadedFragments.Clear();
            }

            if (resp.IsStreamingFinished)
            {
                metadata.EngineInstance = null;
                req.Tag = null;
                if (OnEngineDownloadFinished != null)
                    OnEngineDownloadFinished(metadata);
            }
        }

        public void Update(float time)
        {

            /*HTTPRequest bestHTTPRequest = (HTTPRequest)tag;
            DownloadManifest metadata = (DownloadManifest)(bestHTTPRequest.Tag);
            if (bestHTTPRequest.State != HTTPRequestStates.Finished
                && bestHTTPRequest.State != HTTPRequestStates.Aborted
                && bestHTTPRequest.State != HTTPRequestStates.Error)
            {
                float tDiff = Time.time - metadata.LastPingTime;
                if (tDiff > AbortTime)
                {
                    Debug.LogWarning("Aborted Request " + bestHTTPRequest.Uri);
                    bestHTTPRequest.Abort();
                }
                else
                {
                    ThinkManagerInstance.AddTask(OnTaskThink, AbortTime + ThinkOffset, bestHTTPRequest);
                }
            }*/
        }
    }
#else
    public class DownloadEngineBestHTTP : MonoBehaviour
    {
        void Start()
        {
            throw new System.DllNotFoundException("Using empty DownloadEngineBestHTTP. Please use DownloadEngineWWW or download BestHTTP and #define USE_BESTHTTP.");
        }
    }
#endif
}