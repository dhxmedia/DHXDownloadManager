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
    /// Tests whether a DownloadManifest can post
    /// </summary>
    class ProgressTest<T> : Test<T> where T : IDownloadEngine, new()
    {
        public ProgressTest(Tests<T> parent) : base(parent) { }
        int succeed = -1;
        int progress;
        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();

            Manifest metadata1 = new Manifest("https://s3.amazonaws.com/piko_public/Test.png", ManifestFileStream.Flags.None);
            Manifest metadata2 = new Manifest("https://s3.amazonaws.com/piko_public/Test2.png", ManifestFileStream.Flags.None);
            Manifest metadata3 = new Manifest("https://s3.amazonaws.com/piko_public/Test3.png", ManifestFileStream.Flags.None);

            Progress progress = new Progress();
            progress.AddDownload(metadata1);
            progress.AddDownload(metadata2);
            progress.AddDownload(metadata3);

            progress.OnProgressEnd += (p) => succeed = 1;
            progress.OnPercentChange += progress_OnPercentChange;
            _Parent._Manager.AddDownload(ref metadata1);
            _Parent._Manager.AddDownload(ref metadata2);
            _Parent._Manager.AddDownload(ref metadata3);

            while (succeed == -1)
            {
                yield return null;
            }

            int bytesDownloaded0 = metadata1.BytesDownloaded + metadata2.BytesDownloaded + metadata3.BytesDownloaded;
            int totalBytesDownloaded0 = metadata1.TotalBytesSize + metadata2.TotalBytesSize + metadata3.TotalBytesSize;


            Finish();
            if (bytesDownloaded0 == progress.DownloadedBytes && totalBytesDownloaded0 == progress.TotalBytes)
                Success();
            else
                Fail();

            yield return null;
        }

        void progress_OnPercentChange(float percent, int totalBytesDownloaded, int totalBytes)
        {

        }

    }
}