using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xunit;

#if NET7_0_OR_GREATER
namespace MigraDoc.Tests.Experimental.CreateOnRequest
{
    public class Paragraph
    {
        public string Text { get; set; } = "";
        public Paragraph()
        {

        }
        public Paragraph AddText(string text)
        {
            Text += text;
            return this;
        }

        public Paragraph AddNewLine()
        {
            Text += "\n";
            return this;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public interface ISectionInterface<in T> where T : ISectionInterface<T>
    {
        Collection<Paragraph> Paragraphs { get; set; }

        Paragraph AddParagraph(Paragraph paragraph);

        string ToString();

        bool Equals(object o);

        static abstract bool operator ==(T c1, T c2);

        static abstract bool operator !=(T? c1, T? c2);
    }

    public class SectionCreatorImpl : ISectionInterface<Section>
    {
        public Collection<Paragraph> Paragraphs
        {
            get => Create().Paragraphs;
            set => Create().Paragraphs = value;
        }
        private Document Doc { get; set; }

        public SectionCreatorImpl(Document doc)
        {
            this.Doc = doc;
        }
        public Paragraph AddParagraph(Paragraph paragraph)
        {
            return Create().AddParagraph(paragraph);
        }
        public override string ToString()
        {
            return Create().ToString();
        }

        private Section Create()
        {
            Section section = new Section();
            this.Doc.Sections.Add(section);
            this.Doc._sectionCreator = null!;
            return section;
        }
        public override bool Equals(object? o)
        {
            if (o == null)
            {
                return this.Doc.Sections.Count == 0;
            }
            return false;
        }

        static Boolean ISectionInterface<Section>.operator ==(Section? c1, Section? c2)
        {
            return c2 is null;
        }

        static Boolean ISectionInterface<Section>.operator !=(Section? c1, Section? c2)
        {
            return true;
        }

        protected bool Equals(SectionCreatorImpl other)
        {
            return Doc.Equals(other.Doc);
        }

        public override int GetHashCode()
        {
            return Doc.GetHashCode();
        }

        public static bool operator ==(SectionCreatorImpl c1, object c2)
        {
            return c1.Equals(c2);
        }
        public static bool operator !=(SectionCreatorImpl c1, object c2)
        {
            return !c1.Equals(c2);
        }
    }

    public class Section : ISectionInterface<Section>
    {
        protected bool Equals(Section other)
        {
            return true;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Section)obj);
        }

        public override int GetHashCode()
        {
            return Paragraphs.GetHashCode();
        }

        public Collection<Paragraph> Paragraphs { get; set; }
        public Section()
        {
            Paragraphs = new Collection<Paragraph>();
        }

        public Paragraph AddParagraph(Paragraph paragraph)
        {
            Paragraphs.Add(paragraph);
            return paragraph;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Paragraph paragraph in Paragraphs)
            {
                stringBuilder.Append(paragraph.ToString());
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }

        static Boolean ISectionInterface<Section>.operator ==(Section? c1, Section? c2)
        {
            Console.WriteLine("==");
            return true;
        }

        static Boolean ISectionInterface<Section>.operator !=(Section? c1, Section? c2)
        {
            Console.WriteLine("!=");
            return true;
        }

        public static Boolean operator ==(Section? c1, Section? c2)
        {
            return true;
        }

        public static Boolean operator !=(Section? c1, Section? c2)
        {
            return true;
        }

        //static Boolean SectionInterface.operator ==(SectionInterface c1, SectionInterface c2)
        //{
        //    if (c2 is null)
        //        return true;
        //    return false;
        //}

        //static Boolean SectionInterface.operator !=(SectionInterface c1, SectionInterface c2)
        //{
        //    return !(c1 == c2);
        //}
    }

    public class LastSectionAccessor
    {
        private Document Doc { get; set; }
        public LastSectionAccessor(Document doc)
        {
            this.Doc = doc;
        }

        public Section Get()
        {
            if (Doc.Sections.Count == 0)
            {
                Doc.AddSection(new Section());
            }
            return Doc.Sections.Last();
        }
    }

    public class Document
    {
        public string Name { get; set; }
        public Collection<Section> Sections { get; set; }

        public Document() :
            this("Document")
        {
        }

        public Document(string name)
        {
            this.Name = name;
            _sectionCreator = new SectionCreatorImpl(this);
            this.Sections = new Collection<Section>();
        }

        public Section AddSection(Section section)
        {
            this.Sections.Add(section);
            return section;
        }

        public ISectionInterface<Section> _sectionCreator;

        public ISectionInterface<Section> LastSection
        {
            get
            {
                if (_sectionCreator == null)
                {
                    return Sections.Last();
                }
                return _sectionCreator;
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Document: " + Name + "\n");
            foreach (Section section in Sections)
            {
                stringBuilder.Append("<section>\n");
                stringBuilder.Append(section.ToString());
                //stringBuilder.Append("\n");
                stringBuilder.Append("</section>");
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }
    }

    public class DocumentApp
    {
        [Fact]
        static void TestMain()
        {
            Document doc = new Document();

            //Section section = new Section();
            //doc.AddSection(section);
            if (doc.Sections.Count == 0)
            {
                Console.WriteLine("No sections yet");
            }

            if (doc.LastSection.Equals(null!))
            {
                Console.WriteLine("No sections yet");
            }
            else
            {
                Console.WriteLine("Sections available");
            }
            if (doc.LastSection == null!)
            {
                Console.WriteLine("No sections yet");
            }
            else
            {
                Console.WriteLine("Sections available");
            }

            if (doc.LastSection is null) throw null!;

            doc.LastSection.AddParagraph(new Paragraph())  // <=======
                .AddText("Edwin")
                .AddNewLine()
                .AddText("Hoholzstr. 72")
                .AddNewLine()
                .AddText("53229 Bonn");

            doc.LastSection.AddParagraph(new Paragraph())
                .AddText("Henry")
                .AddNewLine()
                .AddText("Hoholzstr. 72")
                .AddNewLine()
                .AddText("53229 Bonn");

            Console.WriteLine(doc.ToString());
        }
    }
}
#endif
