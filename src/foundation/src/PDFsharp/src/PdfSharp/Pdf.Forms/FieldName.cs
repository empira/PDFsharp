//// PDFsharp - A .NET library for processing PDF
//// See the LICENSE file in the solution root for more information.

//// v7.0.0 TODO review

// #pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member
//namespace PdfSharp.Pdf.Forms
//{
//    public struct FieldName // TODO: FormsCleanUp: Remove.
//    {
//        // Reference 2.0: 12.7.4.2  Field names / Page 532

//        /// <summary>
//        /// Initializes an empty name "/".
//        /// </summary>
//        public FieldName()
//        { }

//        public FieldName(string canonicName)
//        {
//            Name = new(canonicName);
//        }

//        public FieldName(Name name)
//        {
//            Name = name;
//        }

//        public Name Name { get; set; }

//        public string Value => Name.Value;

//        // TODO: Split etc.

//        public FieldName Append(FieldName name)
//        {
//            return Append(name.Value);
//        }

//        public FieldName Append(string? name)
//        {
//            if (String.IsNullOrEmpty(name))
//                return this;
//            return new(Value + "/" + name[1..]);
//        }

//        ///// <summary>
//        ///// Ensures that the name is formally correct.
//        ///// It must be a non-empty string starting with a '/'.
//        ///// </summary>
//        //public static void EnsureName(string name)
//        //{
//        //    if (String.IsNullOrEmpty(name))
//        //        throw new ArgumentNullException(nameof(name));

//        //    if (name[0] != '/')
//        //        throw new ArgumentException($"Name '{name}' must start with a slash ('/').");
//        //}

//        ///// <summary>
//        ///// Converts a string into an atomic name by adding a '/'
//        ///// as prefix to the string.
//        ///// If the specified string already starts with a '/'
//        ///// no action is taken.
//        ///// </summary>
//        //public static string MakeName(string name)
//        //{
//        //    if (String.IsNullOrEmpty(name))
//        //        return "/";
//        //    if (name[0] != '/')
//        //        return String.Concat('/', name);
//        //    return name;
//        //}

//        public bool IsEmpty => Name.IsEmpty;

//        public bool Equals(FieldName other)
//            => Name.Comparer.Compare(Name, other.Name) == 0;

//        public override bool Equals(object? obj)
//        {
//            if (obj is FieldName fieldName)
//                return Name.Comparer.Compare(this.Name, fieldName.Name) == 0;
//            return true;
//        }

//        public override int GetHashCode() => Name.GetHashCode();

//        public override String ToString()
//        {
//            return Value;
//        }

//        /// <summary>
//        /// Compares two names.
//        /// </summary>
//        public int Compare(FieldName l, FieldName r)
//            => Name.Comparer.Compare(l.Name, r.Name);

//        /// <summary>
//        /// Determines whether the two field names are equal.
//        /// </summary>
//        public static bool operator ==(FieldName l, FieldName r) =>
//            Name.Comparer.Compare(l.Name, r.Name) == 0;

//        /// <summary>
//        /// Determines whether the two field names are not equal.
//        /// </summary>
//        public static bool operator !=(FieldName l, FieldName r) => !(l == r);

//        public static readonly FieldName Empty = new();
//    }
//}
