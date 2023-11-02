// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Collections;
using System.ComponentModel;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// A Borders collection represents the eight border objects used for paragraphs, tables etc.
    /// </summary>
    public class Borders : DocumentObject //, IEnumerable<Border>
    {
        /// <summary>
        /// Initializes a new instance of the Borders class.
        /// </summary>
        public Borders()
        {
            BaseValues = new BordersValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Borders class with the specified parent.
        /// </summary>
        internal Borders(DocumentObject parent) : base(parent)
        {
            BaseValues = new BordersValues(this);
        }

        /// <summary>
        /// Determines whether a particular border exists.
        /// </summary>
        public bool HasBorder(BorderType type)
        {
            if (!Enum.IsDefined(typeof(BorderType), type))
                throw new /*InvalidEnum*/ArgumentException(DomSR.InvalidEnumValue(type), "type");

            return GetBorder(type) is not null;
        }

        /// <summary>
        /// Gets the border of the specified border type,
        /// or null if this border object does not yet exist.
        /// </summary>
        public Border? GetBorder(BorderType type)
        {
            switch (type)
            {
                case BorderType.Bottom:
                    return Values.Bottom;
                case BorderType.DiagonalDown:
                    return Values.DiagonalDown;
                case BorderType.DiagonalUp:
                    return Values.DiagonalUp;
#pragma warning disable CS0618
                case BorderType.Horizontal: // Not used in MigraDoc 1.2.
                case BorderType.Vertical:   // Not used in MigraDoc 1.2.
#pragma warning restore CS0618
                    return null;
                case BorderType.Left:
                    return Values.Left;
                case BorderType.Right:
                    return Values.Right;
                case BorderType.Top:
                    return Values.Top;
            }

            if (!Enum.IsDefined(typeof(BorderType), type))
                throw new InvalidEnumArgumentException(DomSR.InvalidEnumValue(type));
            return null;
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Borders Clone()
            => (Borders)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var borders = (Borders)base.DeepCopy();
            if (borders.Values.Top != null)
            {
                borders.Values.Top = borders.Values.Top.Clone();
                borders.Values.Top.Parent = borders;
            }

            if (borders.Values.Left != null)
            {
                borders.Values.Left = borders.Values.Left.Clone();
                borders.Values.Left.Parent = borders;
            }

            if (borders.Values.Right != null)
            {
                borders.Values.Right = borders.Values.Right.Clone();
                borders.Values.Right.Parent = borders;
            }

            if (borders.Values.Bottom != null)
            {
                borders.Values.Bottom = borders.Values.Bottom.Clone();
                borders.Values.Bottom.Parent = borders;
            }

            if (borders.Values.DiagonalUp != null)
            {
                borders.Values.DiagonalUp = borders.Values.DiagonalUp.Clone();
                borders.Values.DiagonalUp.Parent = borders;
            }

            if (borders.Values.DiagonalDown != null)
            {
                borders.Values.DiagonalDown = borders.Values.DiagonalDown.Clone();
                borders.Values.DiagonalDown.Parent = borders;
            }

            return borders;
        }

        ///// <summary>
        ///// Gets an enumerator for the borders object.
        ///// </summary>
        //public IEnumerator<Border> GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //    //var ht = new Dictionary<string, Border?>
        //    //{
        //    //    { "Top", Values.Top },
        //    //    { "Left", Values.Left },
        //    //    { "Bottom", Values.Bottom },
        //    //    { "Right", Values.Right },
        //    //    { "DiagonalUp", Values.DiagonalUp },
        //    //    { "DiagonalDown", Values.DiagonalDown }
        //    //};
        //    //return new BorderEnumerator(ht);
        //}

        /// <summary>
        /// Clears all Border objects from the collection. Additionally, 'Borders = null'
        /// is written to the DDL stream when serialized.
        /// </summary>
        public void ClearAll()
            => BordersCleared = true;

        /// <summary>
        /// Gets or sets the top border.
        /// </summary>
        public Border Top
        {
            get => Values.Top ??= new Border(this);
            set
            {
                SetParent(value);
                Values.Top = value;
            }
        }

        /// <summary>
        /// Gets or sets the left border.
        /// </summary>
        public Border Left
        {
            get => Values.Left ??= new Border(this);
            set
            {
                SetParent(value);
                Values.Left = value;
            }
        }

        /// <summary>
        /// Gets or sets the bottom border.
        /// </summary>
        public Border Bottom
        {
            get => Values.Bottom ??= new Border(this);
            set
            {
                SetParent(value);
                Values.Bottom = value;
            }
        }

        /// <summary>
        /// Gets or sets the right border.
        /// </summary>
        public Border Right
        {
            get => Values.Right ??= new Border(this);
            set
            {
                SetParent(value);
                Values.Right = value;
            }
        }

        /// <summary>
        /// Gets or sets the diagonal up border.
        /// </summary>
        public Border DiagonalUp
        {
            get => Values.DiagonalUp ??= new Border(this);
            set
            {
                SetParent(value);
                Values.DiagonalUp = value;
            }
        }

        /// <summary>
        /// Gets or sets the diagonal down border.
        /// </summary>
        public Border DiagonalDown
        {
            get => Values.DiagonalDown ??= new Border(this);
            set
            {
                SetParent(value);
                Values.DiagonalDown = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the borders are visible.
        /// </summary>
        public bool Visible
        {
            get => Values.Visible ?? false;
            set => Values.Visible = value;
        }

        /// <summary>
        /// Gets or sets the line style of the borders.
        /// </summary>
        public BorderStyle Style
        {
            get => Values.Style ?? BorderStyle.None;
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets or sets the standard width of the borders.
        /// </summary>
        public Unit Width
        {
            get => Values.Width ?? Unit.Empty;
            set => Values.Width = value;
        }

        /// <summary>
        /// Gets or sets the color of the borders.
        /// </summary>
        public Color Color
        {
            get => Values.Color ?? Color.Empty;
            set => Values.Color = value;
        }

        /// <summary>
        /// Gets or sets the distance between text and the top border.
        /// </summary>
        public Unit DistanceFromTop
        {
            get => Values.DistanceFromTop ?? Unit.Empty;
            set => Values.DistanceFromTop = value;
        }

        /// <summary>
        /// Gets or sets the distance between text and the bottom border.
        /// </summary>
        public Unit DistanceFromBottom
        {
            get => Values.DistanceFromBottom ?? Unit.Empty;
            set => Values.DistanceFromBottom = value;
        }

        /// <summary>
        /// Gets or sets the distance between text and the left border.
        /// </summary>
        public Unit DistanceFromLeft
        {
            get => Values.DistanceFromLeft ?? Unit.Empty;
            set => Values.DistanceFromLeft = value;
        }

        /// <summary>
        /// Gets or sets the distance between text and the right border.
        /// </summary>
        public Unit DistanceFromRight
        {
            get => Values.DistanceFromRight ?? Unit.Empty;
            set => Values.DistanceFromRight = value;
        }

        /// <summary>
        /// Sets the distance to all four borders to the specified value.
        /// </summary>
        public Unit Distance
        {
            set
            {
                DistanceFromTop = value;
                DistanceFromBottom = value;
                DistanceFromLeft = value;
                DistanceFromRight = value;
            }
        }

        /// <summary>
        /// Gets the information if the collection is marked as cleared. Additionally, 'Borders = null'
        /// is written to the DDL stream when serialized.
        /// </summary>
        public bool BordersCleared
        {
            get => Values.BordersCleared ?? false;
            set => Values.BordersCleared = value;
        }

        /// <summary>
        /// Converts Borders into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
            => Serialize(serializer, null);

        /// <summary>
        /// Converts Borders into DDL.
        /// </summary>
        internal void Serialize(Serializer serializer, Borders? refBorders)
        {
            if (BordersCleared)
                serializer.WriteLine("Borders = null");

            int pos = serializer.BeginContent("Borders");

            if (Values.Visible is not null &&
                (refBorders == null || refBorders.Values.Visible is null || Visible != refBorders.Visible))
                serializer.WriteSimpleAttribute("Visible", Visible);

            if (Values.Style is not null && (refBorders == null || (Style != refBorders.Style)))
                serializer.WriteSimpleAttribute("Style", Style);

            //if (Values.Width is not null && (refBorders == null || (Values.Width.Value != refBorders.Values.Width)))
            if (!Values.Width.IsValueNullOrEmpty() && (refBorders == null || (Values.Width!.Value != refBorders.Values.Width)))
                serializer.WriteSimpleAttribute("Width", Width);

            //if (Values.Color is not null && (refBorders == null || ((Color.Argb != refBorders.Color.Argb))))
            if (!Values.Color.IsValueNullOrEmpty() && (refBorders == null || ((Color.Argb != refBorders.Color.Argb))))
                serializer.WriteSimpleAttribute("Color", Color);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            //if (Values.DistanceFromTop is not null &&
            if (!Values.DistanceFromTop.IsValueNullOrEmpty() &&
                (refBorders == null || (DistanceFromTop.Point != refBorders.DistanceFromTop.Point)))
                serializer.WriteSimpleAttribute("DistanceFromTop", DistanceFromTop);

            //if (Values.DistanceFromBottom is not null &&
            if (!Values.DistanceFromBottom.IsValueNullOrEmpty() &&
                (refBorders == null || (DistanceFromBottom.Point != refBorders.DistanceFromBottom.Point)))
                serializer.WriteSimpleAttribute("DistanceFromBottom", DistanceFromBottom);

            //if (Values.DistanceFromLeft is not null &&
            if (!Values.DistanceFromLeft.IsValueNullOrEmpty() &&
                (refBorders == null || (DistanceFromLeft.Point != refBorders.DistanceFromLeft.Point)))
                serializer.WriteSimpleAttribute("DistanceFromLeft", DistanceFromLeft);

            //if (Values.DistanceFromRight is not null &&
            if (!Values.DistanceFromRight.IsValueNullOrEmpty() &&
                (refBorders == null || (DistanceFromRight.Point != refBorders.DistanceFromRight.Point)))
                serializer.WriteSimpleAttribute("DistanceFromRight", DistanceFromRight);
            // ReSharper restore CompareOfFloatsByEqualityOperator

            //if (!IsNull("Top"))
            if (!Values.Top.IsValueNullOrEmpty())
                Values.Top!.Serialize(serializer, "Top", null);

            //if (!IsNull("Left"))
            if (!Values.Left.IsValueNullOrEmpty())
                Values.Left!.Serialize(serializer, "Left", null);

            //if (!IsNull("Bottom"))
            if (!Values.Bottom.IsValueNullOrEmpty())
                Values.Bottom!.Serialize(serializer, "Bottom", null);

            //if (!IsNull("Right"))
            if (!Values.Right.IsValueNullOrEmpty())
                Values.Right!.Serialize(serializer, "Right", null);

            //if (!IsNull("DiagonalDown"))
            if (!Values.DiagonalDown.IsValueNullOrEmpty())
                Values.DiagonalDown!.Serialize(serializer, "DiagonalDown", null);

            //if (!IsNull("DiagonalUp"))
            if (!Values.DiagonalUp.IsValueNullOrEmpty())
                Values.DiagonalUp!.Serialize(serializer, "DiagonalUp", null);

            serializer.EndContent(pos);
        }

        /// <summary>
        /// Gets a name of a border.
        /// </summary>
        internal string? GetMyName(Border border)
        {
            if (border == Values.Top)
                return "Top";
            if (border == Values.Bottom)
                return "Bottom";
            if (border == Values.Left)
                return "Left";
            if (border == Values.Right)
                return "Right";
            if (border == Values.DiagonalUp)
                return "DiagonalUp";
            if (border == Values.DiagonalDown)
                return "DiagonalDown";
            return null;
        }

        //        /// <summary>
        //        /// Returns an enumerator that can iterate through the Borders.
        //        /// </summary>
        //        public class BorderEnumerator : IEnumerator<Border>
        //        {
        //#warning This class must be checked with a unit test.
        //            /// <summary>
        //            /// Creates a new BorderEnumerator.
        //            /// </summary>
        //            public BorderEnumerator(Dictionary<string, Border?> ht)
        //            {
        //                _ht = ht;
        //                _index = -1;
        //            }

        //            public void Dispose()
        //                => throw new NotImplementedException();

        //            /// <summary>
        //            /// Sets the enumerator to its initial position, which is before the first element in the border collection.
        //            /// </summary>
        //            public void Reset() => _index = -1;

        //            object IEnumerator.Current => Current;

        //            /// <summary>
        //            /// Gets the current element in the border collection.
        //            /// </summary>
        //            public Border Current
        //            {
        //                get
        //                {
        //                    IEnumerator enumerator = _ht.GetEnumerator();
        //                    enumerator.Reset();
        //                    for (int idx = 0; idx < _index + 1; idx++)
        //                        enumerator.MoveNext();
        //                    // return (((DictionaryEntry)enumerator.Current).Value as Border)!; // B_UG: May return null
        //                    return (((KeyValuePair<string, Border>)enumerator.Current).Value as Border)!;
        //                }
        //            }

        //            /// <summary>
        //            /// Advances the enumerator to the next element of the border collection.
        //            /// </summary>
        //            public bool MoveNext()
        //            {
        //                _index++;
        //                return (_index < _ht.Count);
        //            }

        //            int _index;
        //            readonly Dictionary<string, Border?> _ht;
        //        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Borders));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public BordersValues Values => (BordersValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class BordersValues : Values
        {
            internal BordersValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Border? Top { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Border? Left { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Border? Bottom { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Border? Right { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Border? DiagonalUp { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Border? DiagonalDown { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Visible { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public BorderStyle? Style { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Width { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Color? Color
            {
                get => _color;
                set => _color = DocumentObjectModel.Color.MakeNullIfEmpty(value);
            }
            Color? _color;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? DistanceFromTop { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? DistanceFromBottom { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? DistanceFromLeft { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? DistanceFromRight { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? BordersCleared { get; set; }
        }

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //    //return GetEnumerator();
        //}
    }
}
