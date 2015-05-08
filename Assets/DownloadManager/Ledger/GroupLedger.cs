﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace DHXDownloadManager
{

    /// <summary>
    /// Record of Groups
    /// </summary>
    public class GroupLedger : MonoBehaviour {
        public Ledger Ledger;

        /// <summary>
        /// Whether or not to write to XML
        /// </summary>
        public bool Debug = false;

        public bool HasRead { get; private set; }
        public System.Action OnRead;

        /// <summary>
        /// Func to use for BinarySearching
        /// </summary>
        Func<Group, string> _Find = delegate(Group i) { return i.Key; };
        System.Timers.Timer _WriteTimer = new System.Timers.Timer(2000);
        List<Group> _Groups;
        string _FileName;
        string _DebugFileName;
	    // Use this for initialization
        void Awake()
        {
            _FileName = string.Concat(Application.persistentDataPath + "/", "group_ledger.bin");
            _DebugFileName = string.Concat(Application.persistentDataPath + "/", "group_ledger.xml");
            Read();
            Ledger.OnAddedToLedger += Ledger_OnAddedToLedger;
            _WriteTimer.AutoReset = false;
            _WriteTimer.Elapsed += t_Elapsed;
	    }

        void OnDestroy()
        {
            _WriteTimer.Elapsed -= t_Elapsed;
            Write();
        }


        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _WriteTimer.Stop();
            Write();
        }

        void Group_OnStatusChanged(Group obj, Group.StatusFlags flags)
        {
            // Don't immediately write to disk (so we can batch writes)
            if (_WriteTimer.Enabled == false)
            {
                _WriteTimer.Start();
            }
        }

        void Ledger_OnAddedToLedger(Manifest obj)
        {

            for(int i = 0; i < _Groups.Count; i++)
            {
                if(_Groups[i].Contains(obj.ID))
                {
                    _Groups[i].ListenTo(obj);
                }
            }
        }

        public int FindGroup(string key)
        {
            int index = _Groups.BinarySearch<Group, string>(key, _Find);
            return index;
        }

        /// <summary>
        /// If key exists, return it. Otherwise create a new one and insert
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Group AddGetGroup(string key)
        {
            int index = FindGroup(key);
            if(index >= 0)
            {
                return _Groups[index];
            }

            Group group = new Group();
            group.OnStatusChanged += Group_OnStatusChanged;
            group.Key = key;
            _Groups.Insert(~index, group);
            return group;
        }


        /// <summary>
        /// For a Group, add a List of Manifests
        /// </summary>
        /// <param name="group"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Group AddDownloads(Group group, List<Manifest> metadata)
        {
            for (int i = 0; i < metadata.Count; i++)
            {
                Manifest manifest = metadata[i];
                Ledger.AddDownload(ref manifest);
                metadata[i] = manifest;

                group.Add(manifest.ID);
                group.ListenTo(manifest);
            }

            return group;
        }

        void RemoveGroup(Group group)
        {
            if(group != null)
            {
                bool remove = _Groups.Remove(group);
                group.OnStatusChanged -= Group_OnStatusChanged;
            }

            if (_WriteTimer.Enabled == false)
            {
                _WriteTimer.Start();
            }
        }

        public void Read()
        {
            HasRead = true;
            if (Debug)
            {
                _Groups = SerialXMLReadWrite<List<Group>>.Read(_DebugFileName);
                for(int i = 0; i < _Groups.Count; i++)
                {
                    _Groups[i].Reinit();
                }
            }
            else
            {
                _Groups = SerialBinaryReadWrite<List<Group>>.Read(_FileName);
                for (int i = 0; i < _Groups.Count; i++)
                {
                    _Groups[i].Reinit();
                }
            }

            for (int i = 0; i < _Groups.Count; i++)
            {
                _Groups[i].OnStatusChanged += Group_OnStatusChanged;
            }

            if (OnRead != null)
                OnRead();
        }

        public void Write()
        {
            if(Debug)
            {
                SerialXMLReadWrite<List<Group>>.Write(_Groups, _DebugFileName);
            }
            else
            {
                SerialBinaryReadWrite<List<Group>>.Write(_Groups, _FileName);
            }
        }
        public void Clear()
        {
            _Groups = new List<Group>();
        }
    }
}