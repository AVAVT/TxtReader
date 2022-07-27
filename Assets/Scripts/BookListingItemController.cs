using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BookListingItemController : MonoBehaviour
{
  public TMP_Text bookNameText;
  string bookName;
  string bookDir;
  BookListController listController;
  public void InitializeWithBook(BookListController listController, string bookDir, string bookName)
  {
    this.listController = listController;
    this.bookName = bookName;
    this.bookDir = bookDir;
    bookNameText.text = bookDir + bookName;
  }

  public void OnClick()
  {
    PlayerPrefs.SetString(PlayerPrefKeys.CURRENT_BOOK_DIR_PREF, bookDir);
    PlayerPrefs.SetString(PlayerPrefKeys.CURRENT_BOOK_PREF, bookName);
    PlayerPrefs.SetInt(PlayerPrefKeys.CURRENT_SCROLL_PREF, 1);
    SceneManager.LoadScene(1);
  }

  public void OnDeleteClick()
  {
    listController.ConfirmDeleteBook(bookDir, bookName);
  }
}