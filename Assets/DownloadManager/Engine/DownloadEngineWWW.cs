﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace DHXDownloadManager
{ 
    /// <summary>
    /// UnityEngine WWW implementation of DownloadEngine
    /// </summary>
    public class DownloadEngineWWW : IDownloadEngine {
        

        public event OnDownloadedFinishedDelegate OnEngineDownloadFailed;
        public event OnDownloadedFinishedDelegate OnEngineDownloadFinished;

        GameObject DownloadHost;
        MonoBehaviour DownloadComponent;
        public DownloadEngineWWW()
        {
            DownloadHost = new GameObject();
            GameObject.DontDestroyOnLoad(DownloadHost);
            DownloadHost.name = "DownloadEngineWWW";
            DownloadComponent = DownloadHost.AddComponent<MonoBehaviour>();
        }
        
        public void PerformDownload(Manifest manifest)
        {
            DownloadComponent.StartCoroutine(_Download(manifest));
        }

        IEnumerator _Download(Manifest manifest)
        {
            manifest.__Start();
            manifest.Ping();
            WWW www = null;
            if(manifest.POSTFieldKVP.Count > 0)
            {
                WWWForm form = new WWWForm();
                foreach(KeyValuePair<string, string> kvp in manifest.POSTFieldKVP)
                {
                    form.AddField(kvp.Key, kvp.Value);

                }
                www = new WWW(manifest.URL, form);
            }
            else
            {
                www = new WWW(manifest.URL);
            }
            
            manifest.EngineInstance = www;
            while(manifest.IsActive && www.isDone == false)
            {
                if (string.IsNullOrEmpty(www.error))
                    manifest.__UpdateBytes(www.bytesDownloaded, Mathf.FloorToInt((float)www.bytesDownloaded / www.progress));
                manifest.Ping();
                yield return null;
            }

            if (manifest.IsActive && string.IsNullOrEmpty(www.error))
            {
                if(www.bytes != null)
                {
                    List<byte[]> b = new List<byte[]>();
                    b.Add(www.bytes);
                    manifest.__Update(b);
                }
                if (OnEngineDownloadFinished != null)
                    OnEngineDownloadFinished(manifest);
            }
            else
            {
                manifest.SetError(ManifestErrors.UnknownDownloadError);
                if (manifest.IsActive)
                {
                    Debug.Log(www.error);
                    int statusCode = -1;
                    int.TryParse(www.error.Substring(0, www.error.IndexOf(' ')), out statusCode);
                    manifest.ResponseCode = statusCode;
                }
                if (OnEngineDownloadFailed != null)
                    OnEngineDownloadFailed(manifest);
            }
            manifest.EngineInstance = null;
            yield return null;
        }

        public void Abort(Manifest manifest)
        {

            WWW request = (WWW)manifest.EngineInstance;

            if (request != null)
            {
                request.Dispose();
            }

            manifest.EngineInstance = null;
        }

        public void Update(float time)
        {
        }
        
    }
}