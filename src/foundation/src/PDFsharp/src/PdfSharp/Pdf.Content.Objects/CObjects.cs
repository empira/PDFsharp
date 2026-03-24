// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;
using System.Text;
using PdfSharp.Internal;

// v7.0.0 TODO

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Content.Objects // TODO: split into single files
{
    /// <summary>
    /// Base class for all PDF content stream objects.
    /// </summary>
    public abstract class CObject : ICloneable  // #FILE: CObject.cs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CObject"/> class.
        /// </summary>
        protected CObject()
        { }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        object ICloneable.Clone() => Copy();

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public CObject Clone() => Copy();

        /// <summary>
        /// Implements the copy mechanism. Must be overridden in derived classes.
        /// </summary>
        protected virtual CObject Copy() => (CObject)MemberwiseClone();

        /// <summary>
        /// Must be overridden in a derived class and writes the object to a content writer.
        /// </summary>
        internal abstract void WriteObject(ContentWriter writer);
    }
}

namespace PdfSharp.Pdf.Content.Objects  // TODO: split into single files
{
    /// <summary>
    /// Represents a comment in a PDF content stream.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Text) + "})")]
    public class CComment : CObject
    {
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public new CComment Clone() => (CComment)Copy();

        ///// <summary>
        ///// Implements the copy mechanism of this class.
        ///// </summary>
        //protected override CObject Copy() => base.Copy();

        /// <summary>
        /// Gets or sets the comment text.
        /// </summary>
        public string Text
        {
            get => _text ?? "";
            set => _text = value;
        }

        string? _text;

        /// <summary>
        /// Returns a string that represents the current comment.
        /// </summary>
        public override string ToString() => "% " + _text;

        internal override void WriteObject(ContentWriter writer) => writer.WriteLineRaw(ToString());
    }
}

namespace PdfSharp.Pdf.Content.Objects
{
    /// <summary>
    /// Represents a sequence of objects in a PDF content stream.
    /// </summary>
    [DebuggerDisplay("(count={" + nameof(Count) + "})")]
    public class CSequence : CObject, IList<CObject>
    {
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public new CSequence Clone() => (CSequence)Copy();

        /// <summary>
        /// Implements the copy mechanism of this class.
        /// </summary>
        protected override CObject Copy()
        {
            var clone = (CSequence)base.Copy();
            clone._items.Clear();
            for (int idx = 0; idx < _items.Count; idx++)
                clone._items.Add(_items[idx].Clone());
            return clone;
        }

        /// <summary>
        /// Adds the specified sequence.
        /// </summary>
        /// <param name="sequence">The sequence.</param>
        public void Add(CSequence sequence)
        {
            int count = sequence.Count;
            for (int idx = 0; idx < count; idx++)
                _items.Add(sequence[idx]);
        }

        #region IList Members

        /// <summary>
        /// Adds the specified value add the end of the sequence.
        /// </summary>
        public void Add(CObject value) => _items.Add(value);

        /// <summary>
        /// Removes all elements from the sequence.
        /// </summary>
        public void Clear() => _items.Clear();

        /// <summary>
        /// Determines whether the specified value is in the sequence.
        /// </summary>
        public bool Contains(CObject value) => _items.Contains(value);

        /// <summary>
        /// Returns the index of the specified value in the sequence or -1, if no such value is in the sequence.
        /// </summary>
        public int IndexOf(CObject value) => _items.IndexOf(value);

        /// <summary>
        /// Inserts the specified value in the sequence.
        /// </summary>
        public void Insert(int index, CObject value) => _items.Insert(index, value);

        /// <summary>
        /// Removes the specified value from the sequence.
        /// </summary>
        public bool Remove(CObject value) => _items.Remove(value);

        /// <summary>
        /// Removes the value at the specified index from the sequence.
        /// </summary>
        public void RemoveAt(int index) => _items.RemoveAt(index);

        /// <summary>
        /// Gets or sets a CObject at the specified index.
        /// </summary>
        /// <value></value>
        public CObject this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }
        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the sequence to the specified array.
        /// </summary>
        public void CopyTo(CObject[] array, int index) => _items.CopyTo(array, index);

        /// <summary>
        /// Gets the number of elements contained in the sequence.
        /// </summary>
        public int Count => _items.Count;

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the sequence.
        /// </summary>
        public IEnumerator<CObject> GetEnumerator() => _items.GetEnumerator();

        #endregion

        /// <summary>
        /// Converts the sequence to a PDF content stream.
        /// </summary>
        public string ToContent(ContentWriterOptions? options = null)
        {
#if true
            options ??= new();
            //Stream stream = new MemoryStream();
            ContentWriter writer = new(options);
            WriteObject(writer);
            //writer.Close(false);
            var result = writer.ToString();
            return result;

            //stream.Position = 0;
            //int count = (int)stream.Length;
            //byte[] bytes = new byte[count];
            //var readBytes = stream.Read(bytes, 0, count);
            //Debug.Assert(readBytes == count);
            //stream.Close();
            //return bytes;
#else
            options ??= new();
            Stream stream = new MemoryStream();
            ContentWriter writer = new(stream, options);
            WriteObject(writer);
            writer.Close(false);

            stream.Position = 0;
            int count = (int)stream.Length;
            byte[] bytes = new byte[count];
            var readBytes = stream.Read(bytes, 0, count);
            Debug.Assert(readBytes == count);
            stream.Close();
            return bytes;
#endif
        }

        /// <summary>
        /// Returns a string containing all elements of the sequence.
        /// </summary>
        public override string ToString()
        {
            var s = new StringBuilder();

            for (int idx = 0; idx < _items.Count; idx++)
            {
                // Add spaces except for first item.
                if (idx > 0)
                    s.Append(' ');
                s.Append(_items[idx]);
            }
            return s.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal override void WriteObject(ContentWriter writer)
        {
            for (int idx = 0; idx < _items.Count; idx++)
                _items[idx].WriteObject(writer);
        }

        #region IList<CObject> Members

        int IList<CObject>.IndexOf(CObject item) => _items.IndexOf(item);

        void IList<CObject>.Insert(int index, CObject item) => _items.Insert(index, item);

        void IList<CObject>.RemoveAt(int index) => _items.RemoveAt(index);

        CObject IList<CObject>.this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        #endregion

        #region ICollection<CObject> Members

        void ICollection<CObject>.Add(CObject item) => Add(item);

        void ICollection<CObject>.Clear() => Clear();

        bool ICollection<CObject>.Contains(CObject item) => Contains(item);

        void ICollection<CObject>.CopyTo(CObject[] array, int arrayIndex)
        {
            if (array == null!)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(array));

            if (_items.Count > array.Length - arrayIndex)
                throw new ArgumentException("The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.");

            for (int idx = arrayIndex; idx < _items.Count; idx++)
                array[idx] = _items[idx];
        }

        int ICollection<CObject>.Count => _items.Count;

        bool ICollection<CObject>.IsReadOnly => false;

        bool ICollection<CObject>.Remove(CObject item) => _items.Remove(item);

        #endregion

        #region IEnumerable<CObject> Members

        IEnumerator<CObject> IEnumerable<CObject>.GetEnumerator() => _items.GetEnumerator();

        #endregion

        readonly List<CObject> _items = [];
    }
}

namespace PdfSharp.Pdf.Content.Objects
{
    /// <summary>
    /// Represents the base class for numerical objects in a PDF content stream.
    /// </summary>
    public abstract class CNumber : CObject
    {
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public new CNumber Clone() => (CNumber)Copy();

        ///// <summary>
        ///// Implements the copy mechanism of this class.
        ///// </summary>
        //protected override CObject Copy() => base.Copy();

        public double DoubleValue => GetDoubleValue();

        protected abstract double GetDoubleValue();
    }
}

namespace PdfSharp.Pdf.Content.Objects
{
    /// <summary>
    /// Represents an integer value in a PDF content stream.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public class CInteger : CNumber
    {
        public CInteger(int value) => Value = value;

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public new CInteger Clone() => (CInteger)Copy();

        ///// <summary>
        ///// Implements the copy mechanism of this class.
        ///// </summary>
        //protected override CObject Copy() => base.Copy();

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public int Value
        {
            get => _value;
            set => _value = value;
        }
        int _value;

        protected override Double GetDoubleValue()
        {
            return Value;
        }

        /// <summary>
        /// Returns a string that represents the current value.
        /// </summary>
        public override string ToString()
            => _value.ToString(CultureInfo.InvariantCulture);

        internal override void WriteObject(ContentWriter writer)
            //=> writer.WriteRaw(ToString() + " ");
            => writer.Write(this);
    }
}

namespace PdfSharp.Pdf.Content.Objects
{
    /// <summary>
    /// Represents a real value in a PDF content stream.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public class CReal : CNumber
    {
        public CReal(double value) => Value = value;

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public new CReal Clone() => (CReal)Copy();

        ///// <summary>
        ///// Implements the copy mechanism of this class.
        ///// </summary>
        //protected override CObject Copy() => base.Copy();

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public double Value { get; set; }

        protected override Double GetDoubleValue()
        {
            return Value;
        }

        /// <summary>
        /// Returns a string that represents the current value.
        /// </summary>
        public override string ToString()
        {
            const string format = Config.SignificantDecimalPlaces1Plus9;
            return Value.ToString(format, CultureInfo.InvariantCulture);
        }

        internal override void WriteObject(ContentWriter writer)
            //=> writer.WriteRaw(ToString() + " ");
            => writer.Write(this);
    }
}

namespace PdfSharp.Pdf.Content.Objects // #FOLDER Pdf.Content.Objects/enum
{
    /// <summary>
    /// Type of the parsed string.
    /// </summary>
    public enum CStringType
    {
        /// <summary>
        /// The string has the format "(...)".
        /// </summary>
        String,

        /// <summary>
        /// The string has the format "&lt;...&gt;".
        /// </summary>
        HexString,

        /// <summary>
        /// The string is the content of a dictionary.
        /// Currently, there is no parser for dictionaries in content streams.
        /// </summary>
        Dictionary,
    }
}

namespace PdfSharp.Pdf.Content.Objects
{
    /// <summary>
    /// Represents a string value in a PDF content stream.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public class CString(string value, CStringType stringType = CStringType.String) : CObject
    {
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public new CString Clone() => (CString)Copy();

        /// <summary>
        /// Gets the value as a raw string.
        /// </summary>
        public string Value
        {
            get => field ?? "";
            set => field = value ?? throw new ArgumentNullException(nameof(value));
        } = value;

        /// <summary>
        /// Gets or sets the type of the content string.
        /// </summary>
        public CStringType CStringType
        {
            get; // => _cStringType ?? NRT.ThrowOnNull<CStringType>();
            set; // => _cStringType = value;
        } = stringType;

        //CStringType? _cStringType;
        /// <summary>
        /// Returns a string that represents the current value in a form that can be used in a
        /// PDF content stream.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            int length = Value.Length;

            switch (CStringType)
            {
                case CStringType.String:
                    sb.Append("(");
                    for (int idx = 0; idx < length; idx++)
                    {
                        char ch = Value[idx];
                        switch (ch)
                        {
                            // Reference 2.0: Table 3 — Escape sequences in literal strings / Page 25

                            case Chars.LF:
                                sb.Append(@"\n");
                                break;

                            case Chars.CR:
                                sb.Append(@"\r");
                                break;

                            case Chars.HT:
                                sb.Append(@"\t");
                                break;

                            case Chars.BS:
                                sb.Append(@"\b");
                                break;

                            case Chars.FF:
                                sb.Append(@"\f");
                                break;

                            case Chars.ParenLeft:
                                sb.Append(@"\(");
                                break;

                            case Chars.ParenRight:
                                sb.Append(@"\)");
                                break;

                            case Chars.BackSlash:
                                sb.Append(@"\\");
                                break;

                            default:
#if true_
                                // Not absolutely necessary to use octal encoding for characters less than blank.
                                if (ch < ' ')
                                {
                                    s.Append("\\");
                                    s.Append((char)(((ch >> 6) & 7) + '0'));
                                    s.Append((char)(((ch >> 3) & 7) + '0'));
                                    s.Append((char)((ch & 7) + '0'));
                                }
                                else
#endif
                                sb.Append(ch);
                                break;
                        }
                    }
                    sb.Append(')');
                    break;

                case CStringType.HexString:
                    sb.Append("<");
                    for (int ich = 0; ich < length; ich++)
                    {
                        char ch = Value[ich];
                        byte hi = (byte)((ch >> 4) + '0');
                        byte lo = (byte)((ch & 0xF) + '0');
                        sb.Append((char)(hi < ':' ? hi : hi + ('A' - ':')));
                        sb.Append((char)(lo < ':' ? lo : lo + ('A' - ':')));
                    }
                    sb.Append('>');
                    break;


                case CStringType.Dictionary:
                    sb.Append(Value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return sb.ToString();
        }

        internal override void WriteObject(ContentWriter writer)
        {
            //writer.WriteRaw(ToString());
            writer.Write(ToString());
        }
    }
}

namespace PdfSharp.Pdf.Content.Objects
{

    /// <summary>
    /// Represents a name in a PDF content stream.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Name) + "})")]
    public class CName : CObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CName"/> class.
        /// </summary>
        public CName() => _name = "/";

        /// <summary>
        /// Initializes a new instance of the <see cref="CName"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CName(string name) => Name = name;

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public new CName Clone() => (CName)Copy();

        ///// <summary>
        ///// Implements the copy mechanism of this class.
        ///// </summary>
        //protected override CObject Copy() => base.Copy();

        public Name Value => _name ?? "/";

        /// <summary>
        /// Gets or sets the name. Names must start with a slash.
        /// </summary>
        public string Name
        {
            get => _name ?? "/";
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));
                if (value[0] != '/')
                    throw new ArgumentException(PsMsgs.NameMustStartWithSlash);
                _name = value;
            }
        }
        string? _name;

        /// <summary>
        /// Returns a string that represents the current value.
        /// </summary>
        public override string? ToString() => _name;

        internal override void WriteObject(ContentWriter writer)
            => writer.Write(this);
    }
}

namespace PdfSharp.Pdf.Content.Objects
{
    /// <summary>
    /// Represents an array of objects in a PDF content stream.
    /// </summary>
    [DebuggerDisplay("(count={" + nameof(Count) + "})")]
    public class CArray : CObject, IList<CObject> // Not derived from CSequence.
    {
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public new CArray Clone() => (CArray)Copy();

        /// <summary>
        /// Implements the copy mechanism of this class.
        /// </summary>
        protected override CObject Copy()
        {
            var clone = (CArray)base.Copy();
            clone._items.Clear();
            for (int idx = 0; idx < _items.Count; idx++)
                clone._items.Add(_items[idx].Clone());
            return clone;
        }

        /// <summary>
        /// Returns a string that represents the current array.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('[');
            bool delimiter = true;
            for (int idx = 0; idx < _items.Count; idx++)
            {
#if true
                var value = _items[idx].ToString() ?? throw new InvalidOperationException("Sequence element must not be null.");
                if (!delimiter && !CLexer.IsDelimiter(value[0]))
                    sb.Append(' ');
                delimiter = CLexer.IsDelimiter(value[^1]);
#else
                // Add spaces except for first item.
                if (idx > 0)
                    sb.Append(' ');
#endif
                sb.Append(value);
            }
            sb.Append(']');

            return sb.ToString();
        }

        public void Add(CSequence sequence)
        {
            foreach (var value in sequence)
                Add(value);
        }

        #region IList Members

        /// <summary>
        /// Adds the specified value add the end of the sequence.
        /// </summary>
        public void Add(CObject value) => _items.Add(value);

        /// <summary>
        /// Removes all elements from the sequence.
        /// </summary>
        public void Clear() => _items.Clear();

        /// <summary>
        /// Determines whether the specified value is in the sequence.
        /// </summary>
        public bool Contains(CObject value) => _items.Contains(value);

        /// <summary>
        /// Returns the index of the specified value in the sequence or -1, if no such value is in the sequence.
        /// </summary>
        public int IndexOf(CObject value) => _items.IndexOf(value);

        /// <summary>
        /// Inserts the specified value in the sequence.
        /// </summary>
        public void Insert(int index, CObject value) => _items.Insert(index, value);

        /// <summary>
        /// Removes the specified value from the sequence.
        /// </summary>
        public bool Remove(CObject value) => _items.Remove(value);

        /// <summary>
        /// Removes the value at the specified index from the sequence.
        /// </summary>
        public void RemoveAt(int index) => _items.RemoveAt(index);

        /// <summary>
        /// Gets or sets a CObject at the specified index.
        /// </summary>
        /// <value></value>
        public CObject this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }
        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the sequence to the specified array.
        /// </summary>
        public void CopyTo(CObject[] array, int index) => _items.CopyTo(array, index);

        /// <summary>
        /// Gets the number of elements contained in the sequence.
        /// </summary>
        public int Count => _items.Count;

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the sequence.
        /// </summary>
        public IEnumerator<CObject> GetEnumerator() => _items.GetEnumerator();

        #endregion


        internal override void WriteObject(ContentWriter writer)
            //=> writer.WriteRaw(ToString());
            => writer.Write(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region IList<CObject> Members

        int IList<CObject>.IndexOf(CObject item) => _items.IndexOf(item);

        void IList<CObject>.Insert(int index, CObject item) => _items.Insert(index, item);

        void IList<CObject>.RemoveAt(int index) => _items.RemoveAt(index);

        CObject IList<CObject>.this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        #endregion

        #region ICollection<CObject> Members

        void ICollection<CObject>.Add(CObject item) => Add(item);

        void ICollection<CObject>.Clear() => Clear();

        bool ICollection<CObject>.Contains(CObject item) => Contains(item);

        void ICollection<CObject>.CopyTo(CObject[] array, int arrayIndex)
        {
            if (array == null!)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(array));

            if (_items.Count > array.Length - arrayIndex)
                throw new ArgumentException("The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.");

            for (int idx = arrayIndex; idx < _items.Count; idx++)
                array[idx] = _items[idx];
        }

        int ICollection<CObject>.Count => _items.Count;

        bool ICollection<CObject>.IsReadOnly => false;

        bool ICollection<CObject>.Remove(CObject item) => _items.Remove(item);

        #endregion

        #region IEnumerable<CObject> Members

        IEnumerator<CObject> IEnumerable<CObject>.GetEnumerator() => _items.GetEnumerator();

        #endregion

        readonly List<CObject> _items = [];
    }
}

namespace PdfSharp.Pdf.Content.Objects
{
    /// <summary>
    /// Represents a literal part a PDF content stream.
    /// Use for the BI and ID operators.
    /// They are parsed literally.
    /// </summary>
    //[DebuggerDisplay("({Name}, operands={Operands.Count})")]
    //[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
    //[DebuggerTypeProxy(typeof(COperatorDebuggerDisplay))]
    public class CLiteral : CObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="COperator"/> class.
        /// </summary>
        public CLiteral(string literal) => Value = literal;

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public new COperator Clone() => (COperator)Copy();

        /// <summary>
        /// Gets or sets the byte string.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Returns a string that represents the current operator.
        /// </summary>
        public override string ToString()
        {
            return Value;
        }

        internal override void WriteObject(ContentWriter writer)
        {
            writer.WriteRaw(Value);
        }
    }
}

namespace PdfSharp.Pdf.Content.Objects
{
    /// <summary>
    /// Represents an operator a PDF content stream.
    /// </summary>
    //[DebuggerDisplay("({Name}, operands={Operands.Count})")]
    [DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
    [DebuggerTypeProxy(typeof(COperatorDebuggerDisplay))]
    public class COperator : CObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="COperator"/> class.
        /// </summary>
        protected COperator() => _opCode = null!;

        internal COperator(OpCode opcode) => _opCode = opcode;

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public new COperator Clone() => (COperator)Copy();

        ///// <summary>
        ///// Implements the copy mechanism of this class.
        ///// </summary>
        //protected override CObject Copy() => base.Copy();

        /// <summary>
        /// Gets or sets the name of the operator.
        /// </summary>
        public string Name => _opCode.Name;

        /// <summary>
        /// Gets or sets the operands.
        /// </summary>
        /// <value>The operands.</value>
        public CSequence Operands => _sequence ??= [];

        CSequence? _sequence;

        /// <summary>
        /// Gets the operator description for this instance.
        /// </summary>
        public OpCode OpCode => _opCode;

        readonly OpCode _opCode;

        /// <summary>
        /// Returns a string that represents the current operator.
        /// </summary>
        public override string ToString()
        {
            if (_opCode.OpCodeName == OpCodeName.Dictionary)
                return " ";
#if true
            return Name;
#else
            var sb = new StringBuilder();
            if (_sequence != null)
            {
                int count = _sequence.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    sb.Append(_sequence[idx]);
                }
            }
            sb.Append(' ');
            sb.Append(Name);
            return sb.ToString();
#endif
        }

        internal override void WriteObject(ContentWriter writer)
        {
            if ((OpCode.Flags & OpCodeFlags.InlineImage) == 0)
            {
                if (_sequence != null)
                {
                    int count = _sequence.Count;
                    for (int idx = 0; idx < count; idx++)
                    {
                        _sequence[idx].WriteObject(writer);
                    }
                }
                writer.WriteLineRaw(_opCode.OpCodeName == OpCodeName.Dictionary
                    ? " "
                    : Name);
            }
            else
            {
                // Hack for inline images.
                // Operands of BI and ID are parsed as CLiteral.

                switch (OpCode.OpCodeName)
                {
                    case OpCodeName.BI:
                        {
                            Debug.Assert(_sequence?.Count == 1);
                            var operand = (CLiteral)_sequence[0];
                            writer.WriteLineRaw($"BI\n{operand.Value}");
                        }
                        break;

                    case OpCodeName.ID:
                        {
                            Debug.Assert(_sequence?.Count == 1);
                            var operand = (CLiteral)_sequence[0];
                            writer.WriteLineRaw($"ID {operand.Value}");
                        }
                        break;

                    case OpCodeName.EI:
                        writer.WriteLineRaw("EI");
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        #region Printing/Debugger display
        /// <summary>Function returning string that will be used to display object’s value in debugger for this type of objects.</summary>
        public static Func<COperator, string> debuggerDisplay { get; set; } = o => o.ToString(15);
        string DebuggerDisplay => debuggerDisplay(this);

        /// <summary>Prints longer version of string including name, operands list and operator description.</summary>
        /// <param name="maxOperandsStringLength">Maximal number of characters in operands portion of the string that could be displayed.
        /// If printing all operands would require greater number of characters, a string in form like "15 operands" will be put in the result instead.</param>
        public string ToString(int maxOperandsStringLength)
        {
            if (maxOperandsStringLength < 1)
                return ToString();

            const string sep = ", ";
            var operands = "";
            foreach (var op in Operands)
            {
                var os = op + sep; // This should be optimized and size should be checked before converting to string, I guess some objects may be really long...
                operands += os;
                if (operands.Length > maxOperandsStringLength + sep.Length)
                {
                    operands = Operands.Count + " operands" + sep;
                    break;
                }
            }
            if (operands.Length > 0) operands = operands.Substring(0, operands.Length - sep.Length);

            return $"{Name,-4}({operands})  # {OpCode.Description}";
        }
        #endregion

        internal class COperatorDebuggerDisplay(COperator o)
        {
            public string Name => $"{o.Name} - {o.OpCode.Description}";

            public string Postscript => o.OpCode.Postscript ?? "<none>";

            public OpCodeFlags Flags => o.OpCode.Flags;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public CObject[] Operands => o.Operands.ToArray();
        }
    }
}
