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
    /// Checks the Read capabilities of the ledger.
    /// Also checks that failed DownloadManifests are retried on Restore
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class LedgerTest01<T> : Test<T> where T : IDownloadEngine, new()
    {

        public LedgerTest01(Tests<T> parent) : base(parent) { }

        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();
            _Parent._Ledger.Clear();

            Manifest metadata = new ManifestMemoryStream("httpfs://s3.amazonaws.com/piko_public/Test.png", ManifestFileStream.Flags.None);

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
            succeed = -1;
            Debug.Log(metadata.Status);
            _Parent._Ledger.Write();
            _Parent._Ledger.Clear();
            _Parent._Ledger.Read();
            Manifest obj = _Parent._Ledger.Get(metadata.ID);

            obj.OnDownloadFailed += (m) => succeed = 0;
            obj.OnDownloadFinished += (m) => succeed = 1;
            _Parent._Ledger.Restore();

            while (succeed == -1)
            {
                yield return null;
            }

            Finish();
            if (succeed == 0)
                Success();
            else
                Fail();

            _Parent._Ledger.Write();

            yield return null;
        }
    }
}