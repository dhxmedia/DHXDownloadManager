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
    /// Tests whether an invalid URL throws a fail
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class StatusCodeTest<T> : Test<T> where T : IDownloadEngine, new()
    {
        public StatusCodeTest(Tests<T> parent) : base(parent) { }
        int succeed = -1;
        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();

            DHXDownloadManager.Manifest manifest = new Manifest("http://dwcmsf4e0wn6r.cloudfront.net/vids/131/mov/440/131_0002.part?Expires=1427833040&Signature=Zk3~jOdn0A7RIMIho7qf9RGg6Xcu4lSEsDijAnY3P9iDUl68puWNEeS8jiE2kSU8E8X4KuW1sPagIaeeErAlI4X~AaXpiJs3If382rJm6kCEgj7pesGmZyOYjaSgZlQyueROC3dPPhn8V9Xtmv62TDljXQbmoqs~Aik0JUpBV4U_&Key-Pair-Id=APKAI4VUXALFIQ4JTKFAf", Manifest.Flags.None);
            manifest.OnDownloadFailed += manifest_OnDownloadFailed;
            manifest.OnDownloadFinished += manifest_OnDownloadFinished;
            _Parent._Manager.AddDownload(ref manifest);
            while (succeed == -1)
                yield return new WaitForFixedUpdate();
            Finish();
            if (succeed == 1)
                Success();
            else
                Fail();

            yield return null;
        }


        void manifest_OnDownloadFinished(Manifest metadata)
        {
            Debug.Log("manifest_OnDownloadFinished");
            succeed = 0;
        }

        void manifest_OnDownloadFailed(Manifest metadata)
        {
            Debug.Log("manifest_OnDownloadFailed");
            succeed = (metadata.ResponseCode == 403) ? 1 : 0;
        }
    }

}