using UnityEngine;
using System.Collections;
﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections;
using System.Collections.Generic;
namespace DHXDownloadManager
{
    [System.Flags]
    public enum ManifestErrors
    {
        None = 0,
        IOException = (1 << 0),
        Aborted = (1 << 1),
        TimeOut = (1 << 2),
        UnknownDownloadError = (1 << 29),
        Unknown = (1 << 30)
    }
}