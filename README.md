# DHXDownloadManager
## What is this used for?
DHXDownloadManager is used as a central place to collate downloads. Instead of having requests in various parts of your program, it provides a central interface for downloading. 
## Why is it cool?
* Multiple download engines so you're not locked down to a particular library. Supports Unitys WWW class and BestHTTP and is easy to extend to others.
* Saving to file automatically using the relative path in the URL is supported
* A stored list of downloaded files to keep track of what has been downloaded and the state it's in
* Groups of downloaded files to support a grouped state. Useful for download packs.
* Restarting failed downloads on app restart
* Progress callbacks for individual files and groups of files
* Comes with a set of unit tests
## Sample code
```
// this will save to Application.persistentDataPath/piko_public/Test.png
Manifest manifest = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test.png", ManifestFileStream.Flags.None);
DownloadManager.AddDownload(manifest);
```