using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BooksManager : MonoBehaviour
{
  public static BooksManager Instance { get; private set; }
  public const string BOOK_DIRECTORY = "/books/";
  public const string BOOK_CN_DIRECTORY = "/books-cn/";

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

  public bool DeleteBook(string directory, string bookName)
  {
    try
    {
      var dir = new DirectoryInfo($"{Application.persistentDataPath}{directory}{bookName}/");
      dir.Delete(true);
      return true;
    }
    catch (System.Exception e)
    {
      Debug.LogError(e);
      return false;
    }
  }

  public List<(string directory, string bookName)> GetBookNames()
  {
    List<(string directory, string bookName)> result = new List<(string directory, string bookName)>();

    string path = $"{Application.persistentDataPath}{BOOK_DIRECTORY}";
    System.IO.FileInfo file = new System.IO.FileInfo(path + "/zzz.zip");
    file.Directory.Create();
    result.AddRange(Directory.GetDirectories(path).Select(bookName =>
    {
      var parts = bookName.Split('/');
      return (BOOK_DIRECTORY, parts[parts.Length - 1]);
    }));

    string path2 = $"{Application.persistentDataPath}{BOOK_CN_DIRECTORY}";
    System.IO.FileInfo file2 = new System.IO.FileInfo(path2 + "/zzz.zip");
    file2.Directory.Create();
    result.AddRange(Directory.GetDirectories(path2).Select(bookName =>
    {
      var parts = bookName.Split('/');
      return (BOOK_CN_DIRECTORY, parts[parts.Length - 1]);
    }));

    return result;
  }

  public List<string> GetChapterList(string directory, string bookName)
  {
    string contents = File.ReadAllText($"{Application.persistentDataPath}{directory}{bookName}/chapters.json");
    var jarray = JArray.Parse(contents);

    return jarray.ToObject<List<string>>();
  }

  public string GetChapter(string directory, string bookName, string chapterName)
  {
    return File.ReadAllText($"{Application.persistentDataPath}{directory}{bookName}/{chapterName}");
  }
}