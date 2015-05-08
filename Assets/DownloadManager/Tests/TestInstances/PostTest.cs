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
    class PostTest<T> : Test<T> where T : IDownloadEngine, new()
    {
        public PostTest(Tests<T> parent) : base(parent) { }
        int succeed = -1;
        long Date;
        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();

            Manifest metadata = new ManifestMemoryStream("http://httpbin.org/post", ManifestFileStream.Flags.None);
            ((ManifestStream)(metadata)).OnAssetStreamFailed += FileStreamTest_OnAssetStreamFailed;
            ((ManifestStream)(metadata)).OnAssetStreamSuccess += FileStreamTest_OnAssetStreamSuccess;

            metadata.POSTFieldKVP["PostTest"] = "true";
            Date = System.DateTime.UtcNow.Ticks;
            metadata.POSTFieldKVP["Date"] = Date.ToString();

            _Parent._Manager.AddDownload(metadata);

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


        void FileStreamTest_OnAssetStreamSuccess(Manifest metaData, System.IO.Stream stream)
        {
            System.IO.MemoryStream ms = (System.IO.MemoryStream)stream;
            succeed = 0;
            try
            {
                string response = System.Text.Encoding.UTF8.GetString(ms.GetBuffer());
                if(response.Contains("PostTest") == false 
                    || response.Contains("Date") == false
                    || response.Contains(Date.ToString()) == false)
                {
                    succeed = 1;
                }

            }
            catch
            {
                succeed = 1;
            }
            finally
            { }
        }

        void FileStreamTest_OnAssetStreamFailed(Manifest metaData, System.IO.Stream stream)
        {
            succeed = 1;
        }
    }
}