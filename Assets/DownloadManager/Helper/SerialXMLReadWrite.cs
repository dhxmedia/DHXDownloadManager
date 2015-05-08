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
using System.Xml;
using System.Xml.Serialization;
namespace DHXDownloadManager
{
    /// <summary>
    /// Generic interface for reading/writing an object to an XML file
    /// </summary>
    public static class SerialXMLReadWrite<T>  where T : class, new(){


        public static T Read(string fileName) 
        {

            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            T variable = new T();
            try
            {
                using (FileStream reader = new FileStream(fileName, FileMode.Open))
                {
                    var ser = new DataContractSerializer(typeof(T));
                    XmlDictionaryReader xmlreader =
                XmlDictionaryReader.CreateTextReader(reader, new XmlDictionaryReaderQuotas());

                    variable = ser.ReadObject(xmlreader, true) as T; //ser.Deserialize(reader) as T;
                    xmlreader.Close();
                    reader.Close();
                    
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                UnityEngine.Debug.LogWarning("Could not read from file: " + fileName + ". " + e);
            }
            catch(System.Exception e)
            {
                UnityEngine.Debug.LogWarning("Exception: " + fileName + ". " + e);
            }
            finally
            {

            }
            return variable;
        }

        public static void Write(T variable, string fileName)
        {
            // Use a tmp file incase something goes wrong while writing
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            string filenametmp = fileName + ".tmp";
            try
            {
                using (FileStream writer = new FileStream(filenametmp, FileMode.Create))
                {
                    DataContractSerializer ser = new DataContractSerializer(typeof(T));
                    ser.WriteObject(writer, variable);
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