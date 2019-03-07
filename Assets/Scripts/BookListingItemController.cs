using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BookListingItemController : MonoBehaviour
{
    public TMP_Text bookNameText;
    string bookName;
    BookListController listController;
    public void InitializeWithBookName(BookListController listController, string bookName)
    {
        this.listController = listController;
        this.bookName = bookName;
        bookNameText.text = bookName;
    }

    public void OnClick()
    {
        PlayerPrefs.SetString(PlayerPrefKeys.CURRENT_BOOK_PREF, bookName);
        PlayerPrefs.SetInt(PlayerPrefKeys.CURRENT_SCROLL_PREF, 1);
        SceneManager.LoadScene(1);
    }

    public void OnDeleteClick()
    {
        listController.ConfirmDeleteBook(bookName);
    }
}