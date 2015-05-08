﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
namespace DHXDownloadManager
{
    /// <summary>
    /// The Unity Component wrapper around Manager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ManagerComponent<T> : MonoBehaviour, IDownloadManager where T : IDownloadEngine, new()
    {
        Manager<T> _Manager;

        [SerializeField]
        bool _Verbose = false;
        public bool Verbose { get { return _Manager.Verbose; } set { _Manager.Verbose = value; } }

        [SerializeField]
        int _MaxDownloadCount = 2;
        public int MaxDownloadCount { get { return _Manager.MaxDownloadCount; } set { _Manager.MaxDownloadCount = value; } }

        public event OnDownloadedAddedDelegate OnDownloadAdded;
        public event OnDownloadingStartDelegate OnDownloadingStart;
        public event OnDownloadingEndDelegate OnDownloadingEnd;

        void Awake()
        {
            _Manager = new Manager<T>();
            _Manager.MaxDownloadCount = _MaxDownloadCount;
            _Manager.Verbose = _Verbose;

            _Manager.OnDownloadAdded += _OnDownloadAdded;
            _Manager.OnDownloadingEnd += _OnDownloadingEnd;
            _Manager.OnDownloadingStart += _OnDownloadingStart;
        }
 
        // Update is called once per frame
        void Update()
        {
            _Manager.Tick(Time.deltaTime);
        }

        void _OnDownloadAdded(Manifest metadata)
        {
            if (OnDownloadAdded != null)
                OnDownloadAdded(metadata);
        }

        void _OnDownloadingEnd()
        {
            if (OnDownloadingEnd != null)
                OnDownloadingEnd();
        }

        void _OnDownloadingStart()
        {
            if (OnDownloadingStart != null)
                OnDownloadingStart();
        }

        public void AddDownload(ref Manifest metadata)
        {
            _Manager.AddDownload(ref metadata);
        }

    }
}