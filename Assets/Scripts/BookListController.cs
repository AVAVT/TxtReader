using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BookListController : MonoBehaviour
{
  public RectTransform contentList;
  public GameObject buttonPrefab;
  public DeleteCanvasController deleteCanvasController;
  void Start()
  {
    RefreshBookList();
  }

  public void RefreshBookList()
  {
    contentList.DestroyAllChildren();

    string[] books = BooksManager.Instance.GetBookNames();
    foreach (var s in books)
    {
      var newButton = Instantiate(buttonPrefab, contentList).GetComponent<BookListingItemController>();
      var parts = s.Split('/');
      newButton.InitializeWithBookName(this, parts[parts.Length - 1]);
    }
  }

  public void ConfirmDeleteBook(string bookName)
  {
    deleteCanvasController.ShowConfirmDelete(bookName, (isYes) =>
    {
      if (isYes) DeleteBook(bookName);
    });
  }
  public void DeleteBook(string bookName)
  {
    BooksManager.Instance.DeleteBook(bookName);
    RefreshBookList();
  }
}
