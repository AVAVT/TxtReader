using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using AngleSharp;
using AngleSharp.Dom;
using TMPro;

public class Translator : MonoBehaviour
{
  public TMP_Text translationText;
  public GameObject refreshTranslationButton;

  char _currentWord;

  string CurrentFilePath => $"{Application.persistentDataPath}/translations/{_currentWord}.dat";
  public async Task Translate(char c)
  {
    try
    {
      if (c == _currentWord) return;
      _currentWord = c;

      var pathToNewFolder = $"{Application.persistentDataPath}/translations/";
      Directory.CreateDirectory(pathToNewFolder);

      var hasLocalTranslation = File.Exists(CurrentFilePath);
      refreshTranslationButton.SetActive(hasLocalTranslation);

      if (hasLocalTranslation) translationText.text = await File.ReadAllTextAsync(CurrentFilePath);
      else await RemoteTranslate();
    }
    catch (Exception e)
    {
      translationText.text = "<color=#ff0000>" + e.Message + "\n" + e.StackTrace + "</color>";
      Debug.LogError(e);
    }
  }

  public void RefreshTranslation()
  {
    RemoteTranslate().ConfigureAwait(false);
  }

  async Task RemoteTranslate()
  {
    var c = _currentWord;
    try
    {
      var filePath = CurrentFilePath;
      var starterText = "<size=150%>" + _currentWord + "</size>\n";
      
      translationText.text = starterText;

      var config = Configuration.Default.WithDefaultLoader();
      var address = "https://hvdic.thivien.net/wpy/" + c;
      var context = BrowsingContext.New(config);
      var cellSelector = ".hvres";
      var document = await context.OpenAsync(address);

      if (c != _currentWord) return;

      var builder = new StringBuilder(starterText);

      var first = false;
      var second = false;
      foreach (var cell in document.QuerySelectorAll(cellSelector))
      {
        if (!first)
        {
          first = true;
          var meanings = cell.QuerySelectorAll(".hvres-meaning");
          if (meanings.Length <= 0) continue;
          builder.AppendLine(meanings[0].TextContent.Split("Tổng nét")[0].Trim());
          builder.AppendLine(meanings[1].TextContent.Split("Âm Nôm")[0].Trim());
          continue;
        }

        if (!second)
        {
          second = true;
          continue;
        }

        var title = cell.QuerySelector(".hvres-definition > *")?.TextContent.Trim() ?? "";
        var details = cell.QuerySelectorAll(".hvres-details > *");

        if (string.IsNullOrWhiteSpace(title) && details.Length <= 0) continue;

        builder.Append("-----\n<b>");
        builder.Append(title);
        builder.AppendLine("</b>");

        var skipNext = false;
        for (var i = 0; i < details.Length; i++)
        {
          if (skipNext)
          {
            skipNext = false;
            continue;
          }
          
          var body = details[i].TextContent.Trim();
          if (i % 2 == 0)
          {
            if (body.StartsWith("Từ ghép"))
            {
              skipNext = true;
              continue;
            }
            
            body = "\n<color=#33ff33><i>" + body + "</i></color>";
          }
          builder.AppendLine(body);
        }
      }

      var translationContent = Regex.Replace(builder.ToString(), "\\s*\n\\s*", "\n");
      translationText.text = translationContent;
      await File.WriteAllTextAsync(filePath, translationContent);
    }
    catch (Exception e)
    {
      translationText.text = "<color=#ff0000>" + e.Message + "\n" + e.StackTrace + "</color>";
      Debug.LogError(e);
    }
  }

  static string ExtractText(IElement elem)
  {
    var str = elem.InnerHtml;
    str = Regex.Replace(str, "\\s", " ");
    str = str.Replace("<br>", "\n");
    str = Regex.Replace(str, "<[^>]+>", "");
    return str;
  }
}
