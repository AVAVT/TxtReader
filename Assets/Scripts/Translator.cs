using System;
using System.Linq;
using UnityEngine;
using AngleSharp;
using TMPro;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using AngleSharp.Dom;

public class Translator : MonoBehaviour
{
  public TMP_Text translationText;

  char current;
  public async Task Translate(char c)
  {
    try
    {
      if (c == current) return;
      current = c;

      if (PlayerPrefs.HasKey(PlayerPrefKeys.TRANSLATE_PREFIX + c))
      {
        translationText.text = PlayerPrefs.GetString(PlayerPrefKeys.TRANSLATE_PREFIX + c);
        return;
      }

      translationText.text = c.ToString();

      var config = Configuration.Default.WithDefaultLoader();
      var address = "https://hvdic.thivien.net/whv/" + c;
      var context = BrowsingContext.New(config);
      var document = await context.OpenAsync(address);

      if (c != current) return;

      var cellSelector = ".hvres";
      var cells = document.QuerySelectorAll(cellSelector);
      var content = cells.Select((m, index) =>
      {
        if (index == 0)
        {
          return string.Join("\n", m.QuerySelectorAll(".hvres-meaning")?.Take(2).Select(n => Regex.Replace(ExtractText(n).Split("\n")[0].Trim(), @"\s{2,}", "\n")) ?? new List<string>() { });
        }

        var title = m.QuerySelectorAll(".hvres-definition > *")?.FirstOrDefault()?.TextContent ?? "";
        var detail = string.Join("\n", m.QuerySelectorAll(".hvres-details > *").Select((n, i) =>
        {
          var body = Regex.Replace(ExtractText(n).Trim(), @"\s{2,}", "\n");
          if (i % 2 == 0)
          {
            body = "\n<color=#33ff33><i>" + body + "</i></color>";
          }
          return body;
        }) ?? new List<string>() { });

        if (string.IsNullOrEmpty(title.Trim() + detail.Trim())) return "";

        return "-----\n<b>" + title.Trim() + "</b>\n\n" + detail.Trim();
      });

      translationText.text = c + " " + string.Join("\n", content.Where(x => !string.IsNullOrWhiteSpace(x)));
      PlayerPrefs.SetString(PlayerPrefKeys.TRANSLATE_PREFIX + c, translationText.text);
    }
    catch (Exception e)
    {
      translationText.text = "<color=#ff0000>" + e.Message + "\n" + e.StackTrace + "</color>";
      Debug.LogError(e);
    }
  }

  static string ExtractText(IElement elem)
  {
    //elem.Text;
    //elem.TextContent

    string str = elem.InnerHtml;
    str = Regex.Replace(str, "\\s", " ");
    str = str.Replace("&nbsp;", " ");
    str = str.Replace("<br>", "\n");
    str = Regex.Replace(str, "<[^>]+>", "");  // remove elements
    str = Regex.Replace(str, " +", " ");      // many spaces are one space
    str = Regex.Replace(str, " *\n *", "\n"); // “one <br> two” -> “one\ntwo”
    str = str.Replace("&gt;", ">");
    str = str.Replace("&lt;", "<");
    str = str.Replace("&quot;", "\"");
    str = str.Replace("&apos;", "'");
    str = str.Replace("&amp;", "&");
    // any other html &entity; possible :o(
    str = str.Trim(' '); // leave leading and trailsing \n (<br>)s
    return str;
  }
}
