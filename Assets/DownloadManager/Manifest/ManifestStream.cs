﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
namespace DHXDownloadManager
{
    /// <summary>
    /// Manifest that implements a Stream
    /// </summary>
    [System.Serializable]
    [DataContract]
    public class ManifestStream : Manifest 
    {
        public delegate void OnAssetSavedDelegate(Manifest metaData, Stream stream);

        [field: System.NonSerialized]
        public event OnAssetSavedDelegate OnAssetStreamSuccess;

        [field: System.NonSerialized]
        public event OnAssetSavedDelegate OnAssetStreamFailed;

        [XmlIgnoreAttribute]
        [System.NonSerialized]
        protected Stream _Stream;

        [XmlIgnoreAttribute]
        public Stream Stream { get { return _Stream; } protected set { _Stream = value; } }

        public ManifestStream()
            : base()
        {
        }

        public ManifestStream(string url, Flags flag)
            : base(url, flag)
        {
        }

        override public void __Start()
        {
            base.__Start();
            Stream = CreateStream();
        }

        override public void __Update(List<byte[]> bytes)
        {
            base.__Update(bytes);

            if (Stream != null)
            {
                for (int i = 0; i < bytes.Count; i++)
                {
                    Stream.Write(bytes[i], 0, bytes[i].Length);
                }
                Stream.Flush();
            }

        }

        override public void __Finish()
        {

            if (Stream != null)
            {
                Stream.Close();
                if (FinalizeStreamSuccess() == 0)
                {
                    if (OnAssetStreamSuccess != null)
                        OnAssetStreamSuccess(this, Stream);
                }
                else
                {
                    SetStatus(StatusFlags.Failed);

                    FinalizeStreamFailure();
                    if (OnAssetStreamFailed != null)
                        OnAssetStreamFailed(this, Stream);
                }

                CloseStream();
            }
            else
            {
                SetStatus(StatusFlags.Failed);
                CloseStream();
                FinalizeStreamFailure();
                if (OnAssetStreamFailed != null)
                    OnAssetStreamFailed(this, Stream);
                Debug.LogError("Does not contain FStream");
            }
            OnAssetStreamFailed = null;
            OnAssetStreamSuccess = null;

            base.__Finish();
        }

        override public void __Retry()
        {
            CloseStream();
            FinalizeStreamFailure();
            base.__Retry();
        }

        override public void __ClearManifest()
        {
            OnAssetStreamFailed = null;
            OnAssetStreamSuccess = null;
            base.__ClearManifest();
        }

        override public void Abort()
        {
            CloseStream();
            FinalizeStreamFailure();
            if (OnAssetStreamFailed != null)
                OnAssetStreamFailed(this, Stream);
            base.Abort();
        }



        void CloseStream()
        {
            if (Stream != null)
            {
                Stream.Close();
                Stream.Dispose();
            }
            Stream = null;
        }


        protected virtual Stream CreateStream()
        {
            return null;
        }

        /// <summary>
        /// On success, do this to the stream
        /// </summary>
        /// <returns></returns>
        virtual protected int FinalizeStreamSuccess()
        {
            return 0;
        }

        /// <summary>
        /// On failure, do this to the stream
        /// </summary>
        /// <returns></returns>
        virtual protected int FinalizeStreamFailure()
        {
            return 0;
        }

    }
}