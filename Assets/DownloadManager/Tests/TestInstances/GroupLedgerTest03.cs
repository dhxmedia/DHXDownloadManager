﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.Linq;
namespace DHXDownloadManager.Tests
{

    class GroupLedgerTest03<T> : Test<T> where T : IDownloadEngine, new()
    {

        public GroupLedgerTest03(Tests<T> parent) : base(parent) { }
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

            Group group = _Parent._GroupLedger.AddGetGroup("GroupLedgerTest03");
            _Parent._GroupLedger.AddDownloads(group, manifests);
            group.OnDownloadGroupStartDownload += group_OnDownloadGroupStartDownload;
            group.OnDownloadGroupEndDownload += group_OnDownloadGroupEndDownload;
            for (int i = 0; i < manifests.Count; i++)
            {
                Manifest manifest = manifests[i];
                _Parent._Manager.AddDownload(ref manifest);
                manifests[i] = manifest;
            }


            while (downloadCount != 0 || group.Status != Group.StatusFlags.Complete)
            {
                //Debug.Log("WaitForFixedUpdate(): " + group.Status + " " + downloadCount);
                yield return new WaitForFixedUpdate();
            }
            group.Destroy();
            bool manifestDestroyed = true;
            for (int i = 0; i < manifests.Count; i++)
            {
                if(manifests[i].Status != Manifest.StatusFlags.Destroyed)
                {
                    manifestDestroyed = false;
                }
            }
            Group group2 = _Parent._GroupLedger.AddGetGroup("GroupLedgerTest03");
            if (group.Status == Group.StatusFlags.Destroyed && manifestDestroyed && group2 != group)
                Success();
            else
                Fail();
            _Parent._GroupLedger.Write();
            yield return null;
        }

        void group_OnDownloadGroupEndDownload(Group obj)
        {
            downloadCount--;
            Debug.Log("group_OnDownloadGroupEndDownload");
        }

        void group_OnDownloadGroupStartDownload(Group obj)
        {
            downloadCount++;
            Debug.Log("group_OnDownloadGroupStartDownload");
        }

    }
}