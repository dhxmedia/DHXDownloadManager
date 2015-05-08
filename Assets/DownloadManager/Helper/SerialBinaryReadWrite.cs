﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
namespace DHXDownloadManager
{ 
    /// <summary>
    /// Generic interface for reading/writing an object to a binary file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class SerialBinaryReadWrite<T>  where T : class, new(){


        public static T Read(string fileName) 
        {

            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            T variable = new T();
            try
            {
                using (FileStream reader = new FileStream(fileName, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    variable = formatter.Deserialize(reader) as T;
                    reader.Close();
                }
            }
            catch (System.Exception)
            {
                UnityEngine.Debug.LogWarning("Could not read from file: " + fileName);
            }
            finally
            {

            }
            return variable;
        }

        public static void Write(T variable, string fileName)
        {
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            string filenametmp = fileName + ".tmp";
            try
            {
                // Use a tmp file incase something goes wrong while writing
                using (FileStream writer = new FileStream(filenametmp, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(writer, variable);
                    writer.Close();
                }

                File.Copy(filenametmp, fileName, true);
                File.Delete(filenametmp);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("Exception: " + fileName + ". " + e);
            }
            finally
            {

            }
        }
    }
}