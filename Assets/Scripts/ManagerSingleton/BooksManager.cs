using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BooksManager : MonoBehaviour
{
  public static BooksManager Instance { get; private set; }
  public const string BOOK_DIRECTORY = "/books";

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else
    {
      Destroy(gameObject);
    }
  }

  public bool DeleteBook(string bookName)
  {
    try
    {
      var dir = new DirectoryInfo($"{Application.persistentDataPath}{BOOK_DIRECTORY}/{bookName}/");
      dir.Delete(true);
      return true;
    }
    catch (System.Exception e)
    {
      Debug.LogError(e);
      return false;
    }
  }

  public string[] GetBookNames()
  {
    string path = $"{Application.persistentDataPath}{BOOK_DIRECTORY}";
    System.IO.FileInfo file = new System.IO.FileInfo(path + "/zzz.zip");
    file.Directory.Create();
    return Directory.GetDirectories(path);
  }

  public List<string> GetChapterList(string bookName)
  {
    string contents = File.ReadAllText($"{Application.persistentDataPath}{BOOK_DIRECTORY}/{bookName}/chapters.json");
    var jarray = JArray.Parse(contents);

    return jarray.ToObject<List<string>>();
  }

  public string GetChapter(string bookName, string chapterName)
  {
    return File.ReadAllText($"{Application.persistentDataPath}{BOOK_DIRECTORY}/{bookName}/{chapterName}");
  }
}