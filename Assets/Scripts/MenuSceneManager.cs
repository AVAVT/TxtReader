using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSceneManager : MonoBehaviour
{
  public BookListController bookListController;
  public DownloadService downloadService;
  public CommonProgressBar commonProgressBar;
  public InputField urlInputField;
  public Button downloadButton;
  public void Start()
  {
    Services<IWordDatabaseService>.Bind(new WordDatabaseService());
#if UNITY_ANDROID && !UNITY_EDITOR
    ApplicationChrome.statusBarState = ApplicationChrome.States.TranslucentOverContent;
    ApplicationChrome.statusBarColor = ApplicationChrome.navigationBarColor = 0xff222222;
#endif

    if (PlayerPrefs.HasKey(PlayerPrefKeys.CURRENT_BOOK_PREF))
    {
      var bookName = PlayerPrefs.GetString(PlayerPrefKeys.CURRENT_BOOK_PREF);
      var path = $"{Application.persistentDataPath}{BooksManager.BOOK_DIRECTORY}{bookName}/";
      if (Directory.Exists(path)) SceneManager.LoadScene(1);
    }
    else
    {
      urlInputField.text = PlayerPrefs.GetString(PlayerPrefKeys.SERVER_URL_PREF, "http://192.168.0.148");
    }
  }

  public void DownloadBooks()
  {
    PlayerPrefs.SetString(PlayerPrefKeys.SERVER_URL_PREF, urlInputField.text.Trim());
    var url = $"{urlInputField.text.Trim()}:6969";
    ChangeProgressBarVisibility(true);
    downloadService.DownloadFrom(
      url,
      () =>
      {
        bookListController.RefreshBookList();
        ChangeProgressBarVisibility(false);
      },
      (error) =>
      {
        Debug.Log(error);
        ChangeProgressBarVisibility(false);
      },
      (ratio) => commonProgressBar.UpdateProgress(ratio)
    );
  }

  public void SyncWordDb()
  {
    Services<IWordDatabaseService>.Get().Download();
  }

  void ChangeProgressBarVisibility(bool isVisible)
  {
    urlInputField.gameObject.SetActive(!isVisible);
    downloadButton.gameObject.SetActive(!isVisible);
    commonProgressBar.gameObject.SetActive(isVisible);
  }
}