﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.Linq;
namespace DHXDownloadManager.Tests
{
    /// <summary>
    /// Checks that DownloadManifests are cleaned up when Destroyed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class LedgerTest02<T> : Test<T> where T : IDownloadEngine, new()
    {

        public LedgerTest02(Tests<T> parent) : base(parent) { }

        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();
            _Parent._Ledger.Clear();

            Manifest metadata = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test.png", ManifestFileStream.Flags.None);

            int succeed = -1;
            metadata.OnDownloadStarted += (m) => succeed = -1;
            metadata.OnDownloadFinished += (m) => succeed = 0;
            metadata.OnDownloadFailed += (m) => succeed = 1;
            _Parent._Ledger.AddDownload(ref metadata);
            _Parent._Manager.AddDownload(ref metadata);
            while (succeed == -1)
            {
                yield return null;
            }

            metadata.Destroy();

            Finish();
            
            string realSaveLocation, tmpSaveLocation;
            ManifestFileStream.GetPaths((ManifestFileStream)metadata, out realSaveLocation, out tmpSaveLocation);

            if (metadata.Status == Manifest.StatusFlags.Destroyed && System.IO.File.Exists(realSaveLocation) == false)
                Success();
            else
                Fail();


            yield return null;
        }
    }
}