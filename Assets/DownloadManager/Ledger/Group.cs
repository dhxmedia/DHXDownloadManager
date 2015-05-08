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
    /// Group is the collection of DownloadManifests
    /// Status is dependent on the status of the collected Manifests
    /// </summary>
    [System.Serializable]
    [DataContract]
    public class Group : System.IComparable<Group>
    {

        [DataMember]
        public string Key;

        /// <summary>
        /// The linked IDs from the DownloadLedger
        /// </summary>
        [DataMember]
        List<int> _ManifestIDs = new List<int>();

        [field: System.NonSerialized]
        List<Manifest> _Manifests = new List<Manifest>();
        public List<Manifest> Manifests { get { return _Manifests; } }

        [field: System.NonSerialized]
        public event System.Action<Group> OnDownloadGroupStartDownload;

        [field: System.NonSerialized]
        public event System.Action<Group> OnDownloadGroupEndDownload;

        [field: System.NonSerialized]
        public event System.Action<Group, StatusFlags> OnStatusChanged;

        [field: System.NonSerialized]
        Progress _GroupProgress = new Progress();

        public Progress GroupProgress { get { return _GroupProgress; } protected set { _GroupProgress = value; } }

        public enum StatusFlags
        {
            Default, // no downloads started
            Incomplete, // downloads started, but not finished (or some have failed)
            Complete, // all downloads have completed successfully
            Destroyed // we're deleting the group of Manifests
        }

        [DataMember]
        public StatusFlags Status;

        /// <summary>
        /// If we're in the process of downloading something already
        /// </summary>
        bool _Downloading = false;
        public bool Downloading { get { return _Downloading; } }

        public void Reinit()
        {
            _Manifests = new List<Manifest>();
            _GroupProgress = new Progress();
        }

        public Group()
        {
            _Manifests = new List<Manifest>();
            _GroupProgress = new Progress();
        }

        /// <summary>
        /// Deletes the DownloadGroup and any associated DownloadManifests.
        /// </summary>
        public void Destroy()
        {
            for (int i = 0; i < _Manifests.Count; i++)
                _Manifests[i].Destroy();
        }

        /// <summary>
        /// Total amount of Manifest IDs contained
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return _ManifestIDs.Count;
        }

        /// <summary>
        /// Get ID by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int Get(int index)
        {
            return _ManifestIDs[index];
        }

        /// <summary>
        /// Whether it contains the Ledger ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Contains(int id)
        {
            return _ManifestIDs.Contains(id);
        }

        /// <summary>
        /// Use for adding IDs to the list of DownloadManifests being listened to.
        /// </summary>
        /// <param name="id">The ID associated with the DownloadManifest</param>
        public void Add(int id)
        {
            if (Contains(id) == false)
            {
                _ManifestIDs.Add(id);
            }
        }

        /// <summary>
        /// Listen to a DownloadManifest instances' Status changes
        /// </summary>
        /// <param name="manifest">Download Manifest to Listen to</param>
        public void ListenTo(Manifest manifest)
        {
            if (_Manifests.Contains(manifest) == false)
            {
                _Manifests.Add(manifest);
                manifest.OnStatusChanged += manifest_OnStatusChanged;
                if (_Manifests.Count == _ManifestIDs.Count)
                    RefreshStatus();
            }
        }

        void manifest_OnStatusChanged(Manifest obj)
        {
            RefreshStatus();
        }

        /// <summary>
        /// For every DownloadManifest that we listen to, 
        /// check if it's status to figure out what our status is
        /// </summary>
        void RefreshStatus()
        {
            StatusFlags oldStatus = Status;
            Status = StatusFlags.Default;
            bool completed = true;
            bool downloading = false;
            for (int i = 0; i < _Manifests.Count; i++)
            {
                Manifest manifest = _Manifests[i];
                if (manifest.Status == Manifest.StatusFlags.Failed
                    || manifest.Status == Manifest.StatusFlags.Destroyed)
                {
                    completed = false;
                    if (manifest.Status == Manifest.StatusFlags.Destroyed && (Status == StatusFlags.Default || Status == StatusFlags.Destroyed))
                        Status = StatusFlags.Destroyed;
                    else
                        Status = StatusFlags.Incomplete;
                }
                else if (manifest.Status == Manifest.StatusFlags.None)
                {
                    completed = false;
                }
                else if (manifest.Status == Manifest.StatusFlags.Downloading || manifest.Status == Manifest.StatusFlags.Queued)
                {
                    completed = false;
                    downloading = true;
                    Status = StatusFlags.Incomplete;
                }
            }

            if (completed)
            {
                Status = StatusFlags.Complete;
            }

            if (_Downloading && !downloading)
            {
                if (OnDownloadGroupEndDownload != null)
                    OnDownloadGroupEndDownload(this);
            }
            else if(!_Downloading && downloading)
            {
                _GroupProgress.Clear();

                for (int i = 0; i < _Manifests.Count; i++)
                    _GroupProgress.AddDownload(_Manifests[i]);

                if (OnDownloadGroupStartDownload != null)
                    OnDownloadGroupStartDownload(this);
            }

            _Downloading = downloading;
            if(Status != oldStatus)
            { 
                if (OnStatusChanged != null)
                    OnStatusChanged(this, oldStatus);
            }
            
        }

        /// <summary>
        /// DownloadGroups are sorted by key
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Group other)
        {
            return Key.CompareTo(other.Key);
        }

    }

}