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
    /// Checks that the status of two GroupLedgers is finished on completed download
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class GroupLedgerTest01<T> : Test<T> where T : IDownloadEngine, new()
    {

        public GroupLedgerTest01(Tests<T> parent) : base(parent) { }
        int downloadCount = 0;
        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();

            Manifest metadata1 = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test.png", ManifestFileStream.Flags.None);
            Manifest metadata2 = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test2.png", ManifestFileStream.Flags.None);
            Manifest metadata3 = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test3.png", ManifestFileStream.Flags.None);
            List<Manifest> manifests = new List<Manifest>();
            manifests.Add(metadata1);
            manifests.Add(metadata2);
            manifests.Add(metadata3);

            Group group = _Parent._GroupLedger.AddGetGroup("test");
            group.OnDownloadGroupStartDownload += group_OnDownloadGroupStartDownload;
            group.OnDownloadGroupEndDownload += group_OnDownloadGroupEndDownload;
            _Parent._GroupLedger.AddDownloads(group, manifests);
            for (int i = 0; i < manifests.Count; i++)
            {
                Manifest manifest = manifests[i];
                _Parent._Manager.AddDownload(manifest);
                manifests[i] = manifest;
            }



            metadata1 = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test.png", ManifestFileStream.Flags.None);
            metadata2 = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test2.png", ManifestFileStream.Flags.None);
            metadata3 = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test3.png", ManifestFileStream.Flags.None);
            metadata3 = new ManifestFileStream("httpsf://s3.amazonaws.com/piko_public/Test4.png", ManifestFileStream.Flags.None);
            manifests = new List<Manifest>();
            manifests.Add(metadata1);
            manifests.Add(metadata2);
            manifests.Add(metadata3);

            group = _Parent._GroupLedger.AddGetGroup("_test");
            group.OnDownloadGroupStartDownload += group_OnDownloadGroupStartDownload;
            group.OnDownloadGroupEndDownload += group_OnDownloadGroupEndDownload;
            _Parent._GroupLedger.AddDownloads(group, manifests);
            for (int i = 0; i < manifests.Count; i++)
            {
                Manifest manifest = manifests[i];
                _Parent._Manager.AddDownload(manifest);
                manifests[i] = manifest;
            }

            while (downloadCount != 0)
                yield return new WaitForFixedUpdate();
            Finish();
            Success();
            _Parent._GroupLedger.Write();

            group = _Parent._GroupLedger.AddGetGroup("test");
            group.OnDownloadGroupStartDownload -= group_OnDownloadGroupStartDownload;
            group.OnDownloadGroupEndDownload -= group_OnDownloadGroupEndDownload;


            group = _Parent._GroupLedger.AddGetGroup("_test");
            group.OnDownloadGroupStartDownload -= group_OnDownloadGroupStartDownload;
            group.OnDownloadGroupEndDownload -= group_OnDownloadGroupEndDownload;

            yield return null;
        }

        void group_OnDownloadGroupEndDownload(Group obj)
        {
            downloadCount--;
            Debug.Log("group_OnDownloadGroupEndDownload " + obj.Key + " " + downloadCount);
        }

        void group_OnDownloadGroupStartDownload(Group obj)
        {
            downloadCount++;
            Debug.Log("group_OnDownloadGroupStartDownload " + obj.Key + " " + downloadCount);
        }

    }
}