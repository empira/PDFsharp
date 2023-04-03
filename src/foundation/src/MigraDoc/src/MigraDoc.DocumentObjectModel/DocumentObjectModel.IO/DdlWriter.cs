// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Text;

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// Represents the MigraDoc DDL writer.
    /// </summary>
    public sealed class DdlWriter
    {
        /// <summary>
        /// Initializes a new instance of the DdlWriter class with the specified Stream.
        /// </summary>
        public DdlWriter(Stream stream)
        {
            _writer = new StreamWriter(stream);
            _serializer = new Serializer(_writer);
        }

        /// <summary>
        /// Initializes a new instance of the DdlWriter class with the specified filename.
        /// </summary>
        public DdlWriter(string filename)
        {
            _writer = new StreamWriter(filename, false, Encoding.UTF8);
            _serializer = new Serializer(_writer);
        }

        /// <summary>
        /// Initializes a new instance of the DdlWriter class with the specified TextWriter.
        /// </summary>
        public DdlWriter(TextWriter writer)
        {
            _serializer = new Serializer(writer);
        }

        /// <summary>
        /// Closes the underlying serializer and text writer.
        /// </summary>
        public void Close()
        {
            _serializer = null!;

            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }
        }

        /// <summary>
        /// Flushes the underlying TextWriter.
        /// </summary>
        public void Flush() 
            => _serializer.Flush();

        /// <summary>
        /// Gets or sets the indentation for the DDL file.
        /// </summary>
        public int Indent
        {
            get => _serializer.Indent;
            set => _serializer.Indent = value;
        }

        /// <summary>
        /// Gets or sets the initial indentation for the DDL file.
        /// </summary>
        public int InitialIndent
        {
            get => _serializer.InitialIndent;
            set => _serializer.InitialIndent = value;
        }

        /// <summary>
        /// Writes the specified DocumentObject to DDL.
        /// </summary>
        public void WriteDocument(DocumentObject documentObject)
        {
            documentObject.Serialize(_serializer);
            _serializer.Flush();
        }

        /// <summary>
        /// Writes the specified DocumentObjectCollection to DDL.
        /// </summary>
        public void WriteDocument(DocumentObjectCollection documentObjectContainer)
        {
            documentObjectContainer.Serialize(_serializer);
            _serializer.Flush();
        }

        /// <summary>
        /// Writes a DocumentObject type object to string.
        /// </summary>
        public static string WriteToString(DocumentObject docObject) 
            => WriteToString(docObject, 2, 0);

        /// <summary>
        /// Writes a DocumentObject type object to string. Indent a new block by indent characters.
        /// </summary>
        public static string WriteToString(DocumentObject docObject, int indent) 
            => WriteToString(docObject, indent, 0);

        /// <summary>
        /// Writes a DocumentObject type object to string. Indent a new block by indent + initialIndent characters.
        /// </summary>
        public static string WriteToString(DocumentObject docObject, int indent, int initialIndent)
        {
            StringBuilder strBuilder = new();
            StringWriter? writer = null;
            DdlWriter? wrt = null;
            try
            {
                writer = new StringWriter(strBuilder);

                wrt = new DdlWriter(writer)
                {
                    Indent = indent,
                    InitialIndent = initialIndent
                };
                wrt.WriteDocument(docObject);
                wrt.Close();
            }
            finally
            {
                wrt?.Close();
                writer?.Close();
            }
            return strBuilder.ToString();
        }

        /// <summary>
        /// Writes a DocumentObjectCollection type object to string.
        /// </summary>
        public static string WriteToString(DocumentObjectCollection docObjectContainer) 
            => WriteToString(docObjectContainer, 2, 0);

        /// <summary>
        /// Writes a DocumentObjectCollection type object to string. Indent a new block by _indent characters.
        /// </summary>
        public static string WriteToString(DocumentObjectCollection docObjectContainer, int indent) 
            => WriteToString(docObjectContainer, indent, 0);

        /// <summary>
        /// Writes a DocumentObjectCollection type object to string. Indent a new block by
        /// indent + initialIndent characters.
        /// </summary>
        public static string WriteToString(DocumentObjectCollection docObjectContainer, int indent, int initialIndent)
        {
            StringBuilder strBuilder = new();
            StringWriter? writer = null;
            DdlWriter? wrt = null;
            try
            {
                writer = new StringWriter(strBuilder);

                wrt = new DdlWriter(writer)
                {
                    Indent = indent,
                    InitialIndent = initialIndent
                };
                wrt.WriteDocument(docObjectContainer);
                wrt.Close();
            }
            finally
            {
                wrt?.Close();
                writer?.Close();
            }
            return strBuilder.ToString();
        }

        /// <summary>
        /// Writes a document object to a DDL file.
        /// </summary>
        public static void WriteToFile(DocumentObject docObject, string filename) 
            => WriteToFile(docObject, filename, 2, 0);

        /// <summary>
        /// Writes a document object to a DDL file. Indent a new block by the specified number of characters.
        /// </summary>
        public static void WriteToFile(DocumentObject docObject, string filename, int indent) 
            => WriteToFile(docObject, filename, indent, 0);

        /// <summary>
        /// Writes a DocumentObject type object to a DDL file. Indent a new block by indent + initialIndent characters.
        /// </summary>
        public static void WriteToFile(DocumentObject docObject, string filename, int indent, int initialIndent)
        {
            DdlWriter? wrt = null;
            try
            {
                wrt = new DdlWriter(filename)
                {
                    Indent = indent,
                    InitialIndent = initialIndent
                };
                wrt.WriteDocument(docObject);
            }
            finally
            {
                wrt?.Close();
            }
        }

        /// <summary>
        /// Writes a DocumentObjectCollection type object to a DDL file.
        /// </summary>
        public static void WriteToFile(DocumentObjectCollection docObjectContainer, string filename) 
            => WriteToFile(docObjectContainer, filename, 2, 0);

        /// <summary>
        /// Writes a DocumentObjectCollection type object to a DDL file. Indent a new block by
        /// indent + initialIndent characters.
        /// </summary>
        public static void WriteToFile(DocumentObjectCollection docObjectContainer, string filename, int indent) 
            => WriteToFile(docObjectContainer, filename, indent, 0);

        /// <summary>
        /// Writes a DocumentObjectCollection type object to a DDL file. Indent a new block by
        /// indent + initialIndent characters.
        /// </summary>
        public static void WriteToFile(DocumentObjectCollection docObjectContainer, string filename, int indent, int initialIndent)
        {
            DdlWriter? wrt = null;
            try
            {
                wrt = new DdlWriter(filename)
                {
                    Indent = indent,
                    InitialIndent = initialIndent
                };
                wrt.WriteDocument(docObjectContainer);
            }
            finally
            {
                wrt?.Close();
            }
        }

        StreamWriter? _writer;
        Serializer _serializer;
    }
}
