// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Text;

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// Represents a reader that provides access to DDL data.
    /// </summary>
    public class DdlReader
    {
        /// <summary>
        /// Initializes a new instance of the DdlReader class with the specified Stream.
        /// </summary>
        public DdlReader(Stream stream)
            : this(stream, null)
        { }

        /// <summary>
        /// Initializes a new instance of the DdlReader class with the specified Stream and ErrorManager2.
        /// </summary>
        public DdlReader(Stream stream, DdlReaderErrors? errors)
        {
            _errorManager = errors;
            _reader = new StreamReader(stream);
        }

        /// <summary>
        /// Initializes a new instance of the DdlReader class with the specified filename.
        /// </summary>
        public DdlReader(string filename)
            : this(filename, null)
        { }

        /// <summary>
        /// Initializes a new instance of the DdlReader class with the specified filename and ErrorManager2.
        /// </summary>
        public DdlReader(string filename, DdlReaderErrors? errors)
        {
            _fileName = filename;
            _errorManager = errors;
            _reader = new StreamReader(filename, Encoding.UTF8);
        }

        /// <summary>
        /// Initializes a new instance of the DdlReader class with the specified TextReader.
        /// </summary>
        public DdlReader(TextReader reader)
            : this(reader, null)
        { }

        /// <summary>
        /// Initializes a new instance of the DdlReader class with the specified TextReader and ErrorManager2.
        /// </summary>
        public DdlReader(TextReader reader, DdlReaderErrors? errors)
        {
            _doClose = false;
            _errorManager = errors;
            _reader = reader;
        }

        /// <summary>
        /// Closes the underlying stream or text writer.
        /// </summary>
        public void Close()
        {
            if (_doClose && _reader != null)
            {
                _reader.Close();
                _reader = null;
            }
        }

        /// <summary>
        /// Reads and returns a Document from a file or a DDL string.
        /// </summary>
        public Document ReadDocument()
        {
            if (_reader == null)
                throw new InvalidOperationException("Reader must be initialized and open to read document.");

            var ddl = _reader.ReadToEnd();

            Document document;
            if (!String.IsNullOrEmpty(_fileName))
            {
                var parser = new DdlParser(_fileName, ddl, _errorManager);
                document = parser.ParseDocument(null);
                document.Values.DdlFile = _fileName;
            }
            else
            {
                var parser = new DdlParser(ddl, _errorManager);
                document = parser.ParseDocument(null);
            }
            return document;
        }

        /// <summary>
        /// Reads and returns a DocumentObject from a file or a DDL string.
        /// </summary>
        public DocumentObject? ReadObject()
        {
            if (_reader == null)
                throw new InvalidOperationException("Reader must be initialized and open to read object.");

            var ddl = _reader.ReadToEnd();
            var parser = !String.IsNullOrEmpty(_fileName) ?
                new DdlParser(_fileName, ddl, _errorManager) :
                new DdlParser(ddl, _errorManager);
            return parser.ParseDocumentObject();
        }

        /// <summary>
        /// Reads and returns a Document from the specified file.
        /// </summary>
        public static Document DocumentFromFile(string documentFileName) //, ErrorManager2 _errorManager)
        {
            Document document;
            DdlReader? reader = null;
            try
            {
                reader = new DdlReader(documentFileName); //, _errorManager);
                document = reader.ReadDocument();
            }
            finally
            {
                reader?.Close();
            }
            return document;
        }

        /// <summary>
        /// Reads and returns a Document from the specified DDL string.
        /// </summary>
        public static Document DocumentFromString(string ddl)
        {
            StringReader? stringReader = null;
            Document document;
            DdlReader? reader = null;
            try
            {
                stringReader = new StringReader(ddl);

                reader = new DdlReader(stringReader);
                document = reader.ReadDocument();
            }
            finally
            {
                stringReader?.Close();
                reader?.Close();
            }
            return document;
        }

        /// <summary>
        /// Reads and returns a domain object from the specified file.
        /// </summary>
        public static DocumentObject? ObjectFromFile(string documentFileName, DdlReaderErrors? errors)
        {
            DdlReader? reader = null;
            DocumentObject? domObj;
            try
            {
                reader = new(documentFileName, errors);
                domObj = reader.ReadObject();
            }
            finally
            {
                reader?.Close();
            }
            return domObj;
        }

        /// <summary>
        /// Reads and returns a domain object from the specified file.
        /// </summary>
        public static DocumentObject? ObjectFromFile(string documentFileName)
            => ObjectFromFile(documentFileName, null);

        /// <summary>
        /// Reads and returns a domain object from the specified DDL string.
        /// </summary>
        public static DocumentObject? ObjectFromString(string ddl, DdlReaderErrors? errors = null)
        {
            _ = errors;
            StringReader? stringReader = null;
            DocumentObject? domObj;
            DdlReader? reader = null;
            try
            {
                stringReader = new StringReader(ddl);

                reader = new DdlReader(stringReader);
                domObj = reader.ReadObject();
            }
            finally
            {
                stringReader?.Close();
                reader?.Close();
            }
            return domObj;
        }

        readonly bool _doClose = true;
        TextReader? _reader;
        readonly DdlReaderErrors? _errorManager;
        readonly string? _fileName;
    }
}
