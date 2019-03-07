using System;
using System.Collections;
using System.IO;
using Ionic.Zip;
using UnityEngine;

public class DownloadService : MonoBehaviour
{
  static readonly string FILE_NAME = "books.zip";
  public void DownloadFrom(string url, Action OnDone, Action<string> OnError, Action<float> OnProgress)
  {
    StartCoroutine(
      DownloadFile(
        url,
        FILE_NAME,
        () => Unzip(FILE_NAME, OnDone, OnError),
        OnError,
        OnProgress
      )
    );
  }
  private IEnumerator DownloadFile(string url, string fileName, Action OnDone, Action<string> OnError, Action<float> OnProgress)
  {
    var www = new WWW(url);
    while (!www.isDone)
    {
      OnProgress?.Invoke(www.progress);
      yield return null;
    }

    if (!string.IsNullOrEmpty(www.error)) OnError(www.error);
    else
    {
      byte[] data = www.bytes;
      var systemExtractFile = Path.Combine(Application.persistentDataPath, fileName);
      Debug.Log(systemExtractFile);
      File.WriteAllBytes(systemExtractFile, data);
      OnDone?.Invoke();
    }
  }
  private void Unzip(string fileName, Action OnDone, Action<string> OnError, string extractPath = null)
  {
    var persistentDataPath = Application.persistentDataPath;
    try
    {
      if (extractPath == null)
      {
        extractPath = persistentDataPath;
      }
      string extractFilePath = Path.Combine(persistentDataPath, fileName).Replace("\\", "/");

      using (ZipFile zip = ZipFile.Read(extractFilePath))
      {
        zip.ExtractAll(extractPath, ExtractExistingFileAction.OverwriteSilently);
      }
      File.Delete(extractFilePath);
      OnDone?.Invoke();
    }
    catch (Exception e)
    {
      OnError?.Invoke(e.Message);
    }
  }
}