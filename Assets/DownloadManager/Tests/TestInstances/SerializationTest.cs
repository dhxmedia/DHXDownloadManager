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
    /// Test that when we serialize and deserialize, the resulting ManifestList
    /// is equivalent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SerializationTest<T> : Test<T> where T : IDownloadEngine, new()
    {
        int succeed = -1;

        public SerializationTest(Tests<T> parent) : base(parent) { }

        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();
            ManifestList<ManifestURLSort> list = new ManifestList<ManifestURLSort>();
            Manifest metadata = new ManifestMemoryStream("https://s3.amazonaws.com/piko_public/Test.png", ManifestFileStream.Flags.None);
            list.AddOrFind(ref metadata);
            metadata.POSTFieldKVP["SerializationTest01"] = "SerializationTest01";
            metadata.POSTFieldKVP["SerializationTest02"] = "SerializationTest02";
            string fileName = System.IO.Path.Combine(Application.persistentDataPath, "serialize_test.bin");
            list.Write(fileName);

            yield return new WaitForSeconds(1.0f);
            ManifestList<ManifestURLSort> list2 = new ManifestList<ManifestURLSort>();
            list2.Read(fileName);

            bool equals = list.Equals(list2);
            if (equals)
                succeed = 0;
            Finish();
            if (succeed == 0)
                Success();
            else
                Fail();

            yield return null;
        }

    }
}