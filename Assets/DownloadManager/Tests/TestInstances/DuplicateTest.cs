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
    /// Tests that callbacks for duplicate DownloadManifests stay the same
    /// And DownloadManifests are shared by their key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class DuplicateTest<T> : Test<T> where T : IDownloadEngine, new()
    {
        public DuplicateTest(Tests<T> parent) : base(parent) { }

        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();
            int succeed = -1;

            Manifest metadata = new Manifest("https://s3.amazonaws.com/piko_public/Test.png", 0);
            _Parent._Manager.AddDownload(metadata);
            metadata.OnDownloadFinished += (m) => succeed++;
            metadata.OnDownloadFailed += (m) => succeed++;
            Manifest oldManifest = metadata;

            metadata = new Manifest("https://s3.amazonaws.com/piko_public/Test.png", 0);
            _Parent._Manager.AddDownload(metadata);
            metadata.OnDownloadFinished += (m) => succeed++;
            metadata.OnDownloadFailed += (m) => succeed++;

            while (succeed == -1)
            {
                yield return null;
            }
            Finish();
            if (succeed == 1 && oldManifest == metadata)
                Success();
            else
                Fail();

            yield return null;
        }
    }
}