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

    var books = BooksManager.Instance.GetBookNames();
    foreach (var s in books)
    {
      var newButton = Instantiate(buttonPrefab, contentList).GetComponent<BookListingItemController>();
      newButton.InitializeWithBook(this, s.directory, s.bookName);
    }
  }

  public void ConfirmDeleteBook(string bookDir, string bookName)
  {
    deleteCanvasController.ShowConfirmDelete(bookName, (isYes) =>
    {
      if (isYes) DeleteBook(bookDir, bookName);
    });
  }
  public void DeleteBook(string bookDir, string bookName)
  {
    BooksManager.Instance.DeleteBook(bookDir, bookName);
    RefreshBookList();
  }
}
