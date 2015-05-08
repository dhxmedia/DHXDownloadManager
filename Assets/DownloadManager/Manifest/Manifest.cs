﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
namespace DHXDownloadManager
{

    /// <summary>
    /// The description of a download
    /// </summary>
    [System.Serializable, XmlInclude(typeof(ManifestMemoryStream)), XmlInclude(typeof(ManifestFileStream)), XmlInclude(typeof(ManifestStream))]
    [DataContract]
    public class Manifest {

        [field: System.NonSerialized]
        public event OnProgressUpdateDelegate OnProgressUpdate;

        [field: System.NonSerialized]
        public event DHXDownloadManager.OnDownloadedFinishedDelegate OnDownloadFailed;

        [field: System.NonSerialized]
        public event DHXDownloadManager.OnDownloadedFinishedDelegate OnDownloadFinished;

        [field: System.NonSerialized]
        public event DHXDownloadManager.OnDownloadedStartedDelegate OnDownloadStarted;

        [field: System.NonSerialized]
        public event DHXDownloadManager.OnDownloadedFinishedDelegate OnDownloadRetry;

        [field: System.NonSerialized]
        public event System.Action OnFinalizeMetadata;

        [field: System.NonSerialized]
        public event System.Action<Manifest> OnDestroyed;

        [field: System.NonSerialized]
        public event System.Action<Manifest> OnStatusChanged;

        [field: System.NonSerialized]
        public event System.Action<Manifest> OnAbort;

        [field: System.NonSerialized]
        public event System.Action<Manifest> OnQueued;

        [field: System.NonSerialized]
        public event System.Action<Manifest, int, int> OnDownloadedBytes;

        public bool IsActive { get { return (Status == StatusFlags.Queued || Status == StatusFlags.Downloading); } }

        [System.Flags]
        public enum Flags
        {
            None = 0,
            InfiniteRetry = (1 << 1)
        }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public Flags Flag { get; set; }

        [DataMember]
        public string URL { get; set; }

        [field: System.NonSerialized]
        protected object _EngineInstance;
        public object EngineInstance { get { return _EngineInstance; } set { _EngineInstance = value; } }

        [field: System.NonSerialized]
        protected object _Tag;
        public object Tag { get { return _Tag; } set { _Tag = value; } }

        [DataMember]
        public float LastPingTime { get; set; }

        public float AbortTime = 10.0f;

        public enum StatusFlags
        {
            None = 0,
            Downloading,
            Finished,
            Failed,
            Destroyed,
            Queued
        }

        [DataMember]
        public StatusFlags Status { get; private set; }

        [DataMember]
        public int Attempts { get; set; }

        [DataMember]
        public string RelativePath { get; set; }

        int _ResponseCode;
        [DataMember]
        public int ResponseCode { get { return _ResponseCode; } set {_ResponseCode = value; } }

        /// <summary>
        /// POST Fields
        /// </summary>
        [DataMember]
        Dictionary<string, string> _POSTFieldKVP = new Dictionary<string,string>();
        public Dictionary<string, string> POSTFieldKVP
        {
            get
            {
                return _POSTFieldKVP;
            }
        }

        [field: System.NonSerialized]
        protected int _BytesDownloaded;
        public int BytesDownloaded { get { return _BytesDownloaded; } }

        [field: System.NonSerialized]
        protected int _TotalBytesSize;
        public int TotalBytesSize { get { return _TotalBytesSize; } }

        ManifestErrors _Errors;
        public ManifestErrors Errors { get { return _Errors; } }

        public enum QueuePriority
        {
            Low,
            High
        }

        QueuePriority _Priority;
        public QueuePriority Priority { get { return _Priority; } set { _Priority = value; } }

        public Manifest()
        {
            ID = -1;
            Flag = Flags.None;
            LastPingTime = 5.0f;
            Attempts = 2;
            ResponseCode = -1;
            GenerateAssetPath();
        }

        public Manifest(string url, Flags flag)
        {
            ID = -1;
            Flag = flag;
            URL = url;
            LastPingTime = 5.0f;
            Attempts = 2;
            ResponseCode = -1;
            GenerateAssetPath();
        }

        /// <summary>
        /// Signals that the request has started
        /// </summary>
        virtual public void __Start()
        {
            _Errors = 0;
            SetStatus(StatusFlags.Downloading);
            ResponseCode = -1;
            if (OnDownloadStarted != null)
                OnDownloadStarted(this);
        }

        /// <summary>
        /// Clear all events, restore it to a state where it can be 
        /// redownloaded
        /// </summary>
        virtual public void __ClearManifest()
        {
            Attempts = 2;
            OnProgressUpdate = null;
            OnDownloadFailed = null;
            OnDownloadFinished = null;
            OnDownloadStarted = null;
            OnDownloadRetry = null;
            OnDownloadedBytes = null;
            OnQueued = null;
            OnAbort = null;
            if (OnFinalizeMetadata != null)
                OnFinalizeMetadata();
        }

        /// <summary>
        /// Put us in a state where we can be retried
        /// </summary>
        virtual public void __Retry()
        {
            SetStatus(StatusFlags.None);
            if (OnDownloadRetry != null)
                OnDownloadRetry(this);
        }

        /// <summary>
        /// Stop attempting a download immediately
        /// </summary>
        virtual public void Abort()
        {
            SetError(ManifestErrors.Aborted);
            Attempts = -1;
            SetStatus(StatusFlags.Failed);

            DHXDownloadManager.OnDownloadedFinishedDelegate dg = OnDownloadFailed;
            System.Action<Manifest> abort = OnAbort;
            if (abort != null)
                abort(this);
            if(IsActive)
                __ClearManifest();
            if (dg != null)
                dg(this);
        }


        /// <summary>
        /// Destroy anything we've done. 
        /// If downloading, stop immediately. 
        /// </summary>
        public virtual void Destroy()
        {
            if (IsActive)
                Abort();
            SetStatus(StatusFlags.Destroyed);
            if (OnDestroyed != null)
                OnDestroyed(this);
        }

        /// <summary>
        /// Update byte counts from the DownloadEngine implementation
        /// </summary>
        /// <param name="bytesDownloaded"></param>
        /// <param name="bytesTotal"></param>
        virtual public void __UpdateBytes(int bytesDownloaded, int bytesTotal)
        {
            _BytesDownloaded = bytesDownloaded;
            _TotalBytesSize = bytesTotal;
            if (OnDownloadedBytes != null)
                OnDownloadedBytes(this, bytesDownloaded, bytesTotal);
        }

        /// <summary>
        /// Pass any bytes we've received
        /// </summary>
        /// <param name="bytes"></param>
        virtual public void __Update(List<byte[]> bytes)
        {
            if (OnProgressUpdate != null)
                OnProgressUpdate(this, bytes);
        }

        virtual public void __Finish()
        {
            // if we failed in a child class, don't say its a success
            if(Status != StatusFlags.Failed)
                SetStatus(StatusFlags.Finished);

            if (OnDownloadFinished != null)
                OnDownloadFinished(this);
        }

        public void __Queue()
        {
            SetStatus(Manifest.StatusFlags.Queued);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Manifest p = obj as Manifest;
            if ((System.Object)p == null)
            {
                return false;
            }

            if (ID != p.ID)
                return false;

            if (Flag != p.Flag)
                return false;

            if (URL != p.URL)
                return false;

            if (EngineInstance != null || p.EngineInstance != null)
            {
                if (EngineInstance == null || p.EngineInstance == null)
                    return false;

                if (EngineInstance.Equals(p.EngineInstance) == false)
                    return false;
            }

            if (ID != p.ID)
                return false;

            foreach (KeyValuePair<string, string> kvp in _POSTFieldKVP)
            {
                if (p._POSTFieldKVP.ContainsKey(kvp.Key) == false || p._POSTFieldKVP[kvp.Key] != kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sets status and broadcasts if different
        /// </summary>
        /// <param name="status"></param>
        protected void SetStatus(StatusFlags status)
        {
            if(Status != status)
            { 
                Status = status;
                if (OnStatusChanged != null)
                    OnStatusChanged(this);
            }
        }

        public bool HasFlag(Flags flag)
        {
            return ((Flag & flag) != 0);
        }

        /// <summary>
        /// Takes the URL and gets its relative path
        /// </summary>
        public void GenerateAssetPath()
        {
            if (string.IsNullOrEmpty(URL) == false)
            {
                System.Uri uri = new System.Uri(URL);
                RelativePath = System.Uri.UnescapeDataString(uri.AbsolutePath);
            }
        }

        public void Ping()
        {
            LastPingTime = AbortTime;
        }

        public void Tick(float dt)
        {
            LastPingTime -= dt;
            if (LastPingTime < 0 && IsActive)
            {
                SetError(ManifestErrors.TimeOut);
                Abort();
            }
        }

        public void SetError(ManifestErrors error)
        {
            _Errors |= error;
        }
    }
}