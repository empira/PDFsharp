// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;

#pragma warning disable 1591

namespace MigraDoc.Rendering.UnitTest
{
    /// <summary>
    /// Summary description for TestParagraphIterator.
    /// </summary>
    public class TestParagraphIterator
    {
        //public TestParagraphIterator()
        //{ }

        public static string GetIterators(Paragraph paragraph)
        {
            var iter = new ParagraphIterator(paragraph.Elements);
            iter = iter.GetFirstLeaf();
            string retString = "";
            while (iter != null)
            {
                retString += "[" + iter.Current.GetType().Name + ":]";
                if (iter.Current is Text)
                    retString += ((Text)iter.Current).Content;

                iter = iter.GetNextLeaf();
            }
            return retString;
        }

        public static string GetBackIterators(Paragraph paragraph)
        {
            var iter = new ParagraphIterator(paragraph.Elements);
            iter = iter.GetLastLeaf();
            string retString = "";
            while (iter != null)
            {
                retString += "[" + iter.Current.GetType().Name + ":]";
                if (iter.Current is Text)
                    retString += ((Text)iter.Current).Content;

                iter = iter.GetPreviousLeaf();
            }
            return retString;
        }
    }
}
