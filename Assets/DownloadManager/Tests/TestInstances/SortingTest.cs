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
    /// Tests that URLs and IDs are sorted properly when inserted 
    /// into a ManifestList
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SortingTest<T> : Test<T> where T : IDownloadEngine, new()
    {
        int succeed = -1;

        public SortingTest(Tests<T> parent) : base(parent) { }

        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();

            ManifestList<ManifestURLSort> urlsort = new ManifestList<ManifestURLSort>();
            ManifestList<ManifestIDSort> idsort = new ManifestList<ManifestIDSort>();
            System.Random r = new System.Random((int)System.DateTime.UtcNow.Ticks);
            for (int i = 0; i < 1000; i++)
            {
                int rand = r.Next();
                string id = rand.ToString("X");
                string url = string.Format("http://{0}/{0}", id);
                Manifest dlm = new Manifest(url, Manifest.Flags.None);
                dlm.ID = i;
                urlsort.AddOrFind(ref dlm);
                idsort.AddOrFind(ref dlm);
            }

            bool urlsuccess = true;
            for (int i = 1; i < urlsort.Count(); i++)
            {
                if (urlsort.Get(i).RelativePath.CompareTo(urlsort.Get(i - 1).RelativePath) < 0)
                    urlsuccess = false;
            }

            bool idsuccess = true;
            for (int i = 1; i < idsort.Count(); i++)
            {
                if (idsort.Get(i).ID.CompareTo(idsort.Get(i - 1).ID) < 0)
                    idsuccess = false;
            }

            if (urlsuccess == true && idsuccess == true)
                Success();
            else
                Fail();

            yield return null;
        }

    }
}
