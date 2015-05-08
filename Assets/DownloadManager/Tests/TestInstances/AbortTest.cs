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
    /// Tests whether a DownloadManifest can abort
    /// </summary>
    class AbortTest<T> : Test<T> where T : IDownloadEngine, new()
    {
        public AbortTest(Tests<T> parent) : base(parent) { }

        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();

            Manifest metadata = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test.png", 0);
            int succeed = -1;
            metadata.OnDownloadStarted += (m) => succeed = -1;
            metadata.OnDownloadFinished += (m) => succeed = 1;
            metadata.OnDownloadFailed += (m) => succeed = 0;
            _Parent._Manager.AddDownload(ref metadata);
            metadata.Abort();


            while (succeed == -1)
            {
                yield return null;
            }
            Finish();
            if (succeed == 0)
                Success();
            else
                Fail();

            yield return null;
        }
    }
}