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

  char current;

  string CurrentFilePath => $"{Application.persistentDataPath}/translations/{current}.dat";
  public async Task Translate(char c)
  {
    try
    {
      if (c == current) return;
      current = c;

      string pathToNewFolder = $"{Application.persistentDataPath}/translations/";
      DirectoryInfo directory = Directory.CreateDirectory(pathToNewFolder);

      var hasLocalTranslation = File.Exists(CurrentFilePath);
      refreshTranslationButton.SetActive(hasLocalTranslation);

      if (hasLocalTranslation)
        translationText.text = File.ReadAllText(CurrentFilePath);
      else
        await RemoteTranslate();
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

  // public void RemoteTranslate2()
  // {
  //   var address = "https://hvdic.thivien.net/whv/" + current;

  //   HtmlWeb web = new HtmlWeb();

  //   var htmlDoc = web.Load(address);

  //   var content = htmlDoc.DocumentNode.SelectNodes("//div[@class[contains(.,'hvres')]]").Select((m, index) =>
  //   {
  //     if (index == 0)
  //     {
  //       return string.Join("\n", m.SelectNodes("//div[@class[contains(.,'hvres-meaning')]]").Take(2).Select((n, i) =>
  //         {
  //           if (i == 0) return n.InnerText.Split("Tổng nét")[0].Trim();
  //           return n.InnerText.Split("Âm Nôm")[0].Trim();
  //         }) ?? new List<string>() { });
  //     }

  //     var title = m.SelectSingleNode("//div[@class[contains(.,'hvres-definition')]]")?.FirstChild?.InnerText ?? "";
  //     var detail = string.Join("\n", m.SelectSingleNode("//div[@class[contains(.,'hvres-details')]]").ChildNodes.Select((n, i) =>
  //     {
  //       var body = n.InnerText.Trim();
  //       if (i % 2 == 0)
  //       {
  //         body = "\n<color=#33ff33><i>" + body + "</i></color>";
  //       }
  //       return body;
  //     }) ?? new List<string>() { });

  //     if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(detail)) return null;

  //     return "-----\n<b>" + title.Trim() + "</b>\n" + detail.Trim();
  //   });

  //   var translationContent = current + " " + Regex.Replace(string.Join("\n", content), "\\s*\n\\s*", "\n");
  //   translationText.text = translationContent;
  // }

  public async Task RemoteTranslate()
  {
    var c = current;
    try
    {
      var filePath = $"{Application.persistentDataPath}/translations/{c}.dat";

      translationText.text = c.ToString();

      var config = Configuration.Default.WithDefaultLoader();
      var address = "https://hvdic.thivien.net/whv/" + c;
      var context = BrowsingContext.New(config);
      var document = await context.OpenAsync(address);

      if (c != current) return;

      var cellSelector = ".hvres";
      var cells = document.QuerySelectorAll(cellSelector);

      StringBuilder builder = new StringBuilder(current + " ");

      var first = false;
      var second = false;
      foreach (var cell in cells)
      {
        if (!first)
        {
          first = true;
          var meanings = cell.QuerySelectorAll(".hvres-meaning");
          if (meanings == null) continue;
          builder.AppendLine(meanings[0].TextContent.Split("Tổng nét")[0].Trim());
          builder.AppendLine(meanings[1].TextContent.Split("Âm Nôm")[0].Trim());
          continue;
        }

        if (!second)
        {
          second = true;
          continue;
        }

        var title = cell.QuerySelector(".hvres-definition > *")?.TextContent ?? "";
        var details = cell.QuerySelectorAll(".hvres-details > *");

        if (string.IsNullOrWhiteSpace(title) && details == null) continue;

        builder.Append("-----\n<b>");
        builder.Append(title.Trim());
        builder.AppendLine("</b>");

        for (int i = 0; i < details.Length; i++)
        {
          var body = details[i].TextContent.Trim();
          if (i % 2 == 0)
          {
            body = "\n<color=#33ff33><i>" + body + "</i></color>";
          }
          builder.AppendLine(body);
        }
      }

      var translationContent = Regex.Replace(builder.ToString(), "\\s*\n\\s*", "\n");
      translationText.text = translationContent;
      File.WriteAllText(CurrentFilePath, translationContent);
    }
    catch (Exception e)
    {
      translationText.text = "<color=#ff0000>" + e.Message + "\n" + e.StackTrace + "</color>";
      Debug.LogError(e);
    }
  }

  static string ExtractText(IElement elem)
  {
    string str = elem.InnerHtml;
    str = Regex.Replace(str, "\\s", " ");
    str = str.Replace("<br>", "\n");
    str = Regex.Replace(str, "<[^>]+>", "");
    return str;
  }
}
