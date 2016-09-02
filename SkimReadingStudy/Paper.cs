using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SkimReadingStudy
{
    // constructs representations of "papers"
    // each "paper" has a certain number of "pages", which have their own class
    class Paper
    {
        private List<Page> listOfPages = new List<Page>();  // holds a list of all of the pages in the paper
        private int currentPageNumDisplayed;                // keeps track of the page currently displayed

        // constructor - add a page to the list for each page in the paper and set the current page displayed to 1
        public Paper(String nameOfPaper, int numOfPages)
        {
            for (int i = 1; i <= numOfPages; i++)
            {
                listOfPages.Add(new Page(nameOfPaper, i));
            }
            currentPageNumDisplayed = 1;
        }

        // return the view ranges of the page to display on the HyperBraille
        public OrderedDictionary GetViewRangesOfPage(int pageNum)
        {
            Page pageToDisplay = listOfPages.ElementAt(pageNum - 1);                  // get the page from the list
            OrderedDictionary viewRangesToReturn = pageToDisplay.GetViewRanges();     // get the view ranges for this page
            currentPageNumDisplayed = pageNum;     // update the current page number displayed
            return viewRangesToReturn;             // return the view ranges for the new page to be displayed
        }

        // return the current page displayed on the device
        public int GetCurrentPageDisplayed()
        {
            return currentPageNumDisplayed;
        }

        // return the total number of pages for the paper displayed on the device
        public int GetTotalNumberOfPages()
        {
            return listOfPages.Count;
        }

        // display the previous page of the paper
        public OrderedDictionary FlipToPreviousPage()
        {
            currentPageNumDisplayed--;
            return GetViewRangesOfPage(currentPageNumDisplayed);
        }

        // display the next page of the paper
        public OrderedDictionary FlipToNextPage()
        {
            currentPageNumDisplayed++;
            return GetViewRangesOfPage(currentPageNumDisplayed);
        }
         
        // return the content selected via the title of the content as a string
        public String GetContent(String title)
        {
            return listOfPages.ElementAt(currentPageNumDisplayed-1).GetNamesAndText()[title];
        }
    }
}
