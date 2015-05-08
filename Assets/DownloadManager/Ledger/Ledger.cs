﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
namespace DHXDownloadManager
{ 
    /// <summary>
    /// Record of Manifests
    /// </summary>
    public class Ledger : MonoBehaviour {

        public GameObject Manager;
        public IDownloadManager _Manager;

        /// <summary>
        /// Whether we should use XML or binary
        /// </summary>
        public bool Debug;
        public event System.Action<Manifest> OnAddedToLedger;

        ManifestList<ManifestURLSort> _URLList = new ManifestList<ManifestURLSort>();
        ManifestList<ManifestIDSort> _IDList = new ManifestList<ManifestIDSort>();

        string _FileName;
        string _DebugFileName;

        /// <summary>
        /// Timer used for delayed writing
        /// </summary>
        System.Timers.Timer _WriteTimer = new System.Timers.Timer(2000);

	    // Use this for initialization
	    void Awake () {
            MonoBehaviour[] components = Manager.GetComponents<MonoBehaviour>();
            for (int i = 0; i < components.Length; i++)
            {
                if(components[i] is IDownloadManager)
                {
                    _Manager = components[i] as IDownloadManager;
                    break;
                }
            }
            _URLList.Debug = Debug;
            _IDList.Debug = Debug;
            _FileName = string.Concat(Application.persistentDataPath + "/", "ledger.bin");
            _DebugFileName = string.Concat(Application.persistentDataPath + "/", "ledger.xml");
            _WriteTimer.AutoReset = false;
            _WriteTimer.Elapsed += t_Elapsed;
	    }

        void Start()
        {
            Read();
            Restore();
        }
	
        void OnDestroy()
        {
            _WriteTimer.Elapsed -= t_Elapsed;
            Write();
        }
        public void AddDownload(ref Manifest manifest)
        {
            string url = manifest.URL;
            // Only add a download to IDList if it doesn't have an ID already
            _URLList.AddOrFind(ref manifest);
            manifest.URL = url;
            if (manifest.ID < 0)
            {
                manifest.ID = _IDList.Count();
                bool isNew = _IDList.AddOrFind(ref manifest);
                if (isNew)
                {
                    manifest.OnStatusChanged += manifest_OnStatusChanged;
                }
                if (OnAddedToLedger != null)
                    OnAddedToLedger(manifest);
            }
        }

        /// <summary>
        /// Event callback when we've had a change in Manifest Status
        /// </summary>
        /// <param name="obj"></param>
        void manifest_OnStatusChanged(Manifest obj)
        {
            // Don't immediately write to disk (so we can batch writes)
            if (_WriteTimer.Enabled == false)
            {
                _WriteTimer.Start();
            }
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _WriteTimer.Stop();
            Write();
        }

        public void Clear()
        {
            _URLList.Clear();
            _IDList.Clear();
        }

        public void Read()
        {
            Clear();
            if(Debug)
            {
                _IDList.Read(_DebugFileName);
            }
            else
            {
                _IDList.Read(_FileName);
            }
            for (int i = 0; i < _IDList.Count(); i++)
            {
                Manifest manifest = _IDList.Get(i);
                // force status to None if it thinks we're downloading
                if (manifest.IsActive)
                    manifest.__Retry();
                _URLList.AddOrFind(ref manifest);

                manifest.OnStatusChanged += manifest_OnStatusChanged;
                if (OnAddedToLedger != null)
                    OnAddedToLedger(manifest);
            }
        }

        public void Write()
        {
            if(Debug)
            {
                _IDList.Write(_DebugFileName);
            }
            else
            {
                _IDList.Write(_FileName);
            }
        }

        /// <summary>
        /// For each Manifest recorded, try re downloading the incomplete ones
        /// </summary>
        public void Restore()
        {

            for (int i = 0; i < _IDList.Count(); i++)
            {
                Manifest m = _IDList.Get(i);
                if ((m.Status == Manifest.StatusFlags.Failed) || (m.Status == Manifest.StatusFlags.Downloading) || (m.Status == Manifest.StatusFlags.Queued) || (m.Status == Manifest.StatusFlags.None))
                {
                    Manifest oldManifest = m;
                    _Manager.AddDownload(ref m);
                    if (m != oldManifest)
                        throw new System.Exception("DownloadLedger: This shouldn't happen. There was probably a mix of ledger and non ledger downloads. Currently unhandled");

                }
            }
        }

        /// <summary>
        /// Get Manifest by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Manifest Get(int index)
        {
            return _IDList.Get(index);
        }

        /// <summary>
        /// Total count of IDs contained
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return _IDList.Count();
        }
    }
}