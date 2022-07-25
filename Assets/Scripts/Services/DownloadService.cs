using System;
using System.Collections;
using System.IO;
using Ionic.Zip;
using UnityEngine;
using UnityEngine.Networking;

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
    UnityWebRequest www = new UnityWebRequest(url);

    www.downloadHandler = new DownloadHandlerBuffer();
    www.SendWebRequest();

    while (!www.isDone)
    {
      OnProgress?.Invoke(www.downloadProgress);
      yield return null;
    }

    if (www.isNetworkError || www.isHttpError)
    {
      Debug.LogError(www.error);
      OnError(www.error);
    }
    else
    {
      byte[] data = www.downloadHandler.data;
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