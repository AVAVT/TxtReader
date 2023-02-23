using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public interface IWordDatabaseService
{
  Task Download();
  IReadOnlyDictionary<char, bool> WordDatabase { get; }
}

public class WordDatabaseService : IWordDatabaseService
{
  const string REMOTE_DB_PATH =
      "https://script.google.com/macros/s/AKfycby_XU2MCAbMZhYpLgIVGPTaylYmUkiXIpwjUPSN-DinB1ukTUkpQh27lG6HXvX13sEn1Q/exec";

  const string FILENAME = "/words.dat";
  static readonly string FilePath = Application.persistentDataPath + FILENAME;

  Dictionary<char, bool> _wordDatabase;
  public IReadOnlyDictionary<char, bool> WordDatabase => _wordDatabase;

  public WordDatabaseService()
  {
    _wordDatabase = Load();
  }

  static Dictionary<char, bool> Load() => PersistenceService.LoadBinaryData<Dictionary<char, bool>>(FilePath);

  public async Task Download()
  {
    try
    {
      using var request = UnityWebRequest.Get(REMOTE_DB_PATH);
      await request.SendWebRequest();

      var json = request.downloadHandler.text;
      Debug.Log(json);
      var datum = JsonConvert.DeserializeObject<string[][]>(json);

      _wordDatabase = datum.ToDictionary(data => data[0][0], data => true);

      PersistenceService.SaveBinaryData(FilePath, Application.persistentDataPath, _wordDatabase);
    }
    catch (Exception e)
    {
      Debug.LogException(e);
    }
  }
}