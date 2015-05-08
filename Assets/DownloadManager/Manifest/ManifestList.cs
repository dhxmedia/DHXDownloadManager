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
    /// Comparer that sorts by the RelativeURL
    /// </summary>
    public class ManifestURLSort : IComparer<Manifest>
    {
        public int Compare(Manifest _x, Manifest _y)
        {
            Manifest x = (Manifest)_x;
            Manifest y = (Manifest)_y;
            return x.RelativePath.CompareTo(y.RelativePath);
        }
    }

    /// <summary>
    /// Comparer that sorts by ID
    /// </summary>
    public class ManifestIDSort : IComparer<Manifest>
    {
        public int Compare(Manifest _x, Manifest _y)
        {
            Manifest x = (Manifest)_x;
            Manifest y = (Manifest)_y;
            return x.ID.CompareTo(y.ID);
        }
    }

    /// <summary>
    /// Container wrapper that sorts in a particular way.
    /// Reads/Writes to file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    [DataContract]
    public class ManifestList<T> where T : IComparer<Manifest>
    {
        public bool Debug;

        [SerializeField]
        [DataMember]
        List<Manifest> _Metadata = new List<Manifest>();

        public int Index(Manifest metadata)
        {
            int index = _Metadata.BinarySearch(metadata, (T)System.Activator.CreateInstance(typeof(T)));
            if(index < 0)
            {
                return index;
            }
            return index;
        }

        /// <summary>
        /// Total count of Manifests contained
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return _Metadata.Count;
        }

        /// <summary>
        /// Get Manifest by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Manifest Get(int index)
        {
            return _Metadata[index];
        }

        /// <summary>
        /// If it exists, return it. If it doesn't, insert it
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public bool AddOrFind(ref Manifest metadata)
        {
            bool isNew = false;
            int index = Index(metadata);
            if(index < 0)
            {
                isNew = true;
                _Metadata.Insert(~index, metadata);
            }
            else
            {
                metadata = _Metadata[index];
            }
            return isNew;
        }

        public void Remove(Manifest metadata)
        {
            int index = Index(metadata);
            if (index >= 0)
                _Metadata.Remove(metadata);
        }

        public void Read(string fileName)
        {

            if(Debug)
            {
                _Metadata = SerialXMLReadWrite<List<Manifest>>.Read(fileName);
            }
            else
            {
                _Metadata = SerialBinaryReadWrite<List<Manifest>>.Read(fileName);// new List<DownloadManifest>();
            }
           
        }

        public void Write(string fileName)
        {
            if(Debug)
            {
                SerialXMLReadWrite<List<Manifest>>.Write(_Metadata, fileName);
            }
            else
            {
                SerialBinaryReadWrite<List<Manifest>>.Write(_Metadata, fileName);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            ManifestList<T> p = obj as ManifestList<T>;
            if ((System.Object)p == null)
            {
                return false;
            }
            return Enumerable.SequenceEqual(_Metadata, p._Metadata);
        }

        public void Clear()
        {
            _Metadata.Clear();
        }
    }
}