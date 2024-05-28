// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Collections;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// A TabStops collection represents all TabStop objects in a paragraph.
    /// </summary>
    public class TabStops : DocumentObjectCollection, IEnumerable<TabStop>
    {
        /// <summary>
        /// Specifies the minimal spacing between two TabStop positions.
        /// </summary>
        public static readonly double TabStopPrecision = 1.5;

        /// <summary>
        /// Initializes a new instance of the TabStops class.
        /// </summary>
        public TabStops()
        {
            BaseValues = new TabStopsValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the TabStops class with the specified parent.
        /// </summary>
        internal TabStops(DocumentObject parent) : base(parent)
        {
            BaseValues = new TabStopsValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new TabStops Clone()
            => (TabStops)DeepCopy();

        /// <summary>
        /// Returns an enumerator that iterates through the tab stop collection.
        /// </summary>
        public new IEnumerator<TabStop> GetEnumerator()
            => Elements.Cast<TabStop>().GetEnumerator();

        /// <summary>
        /// Gets a TabStop by its index.
        /// </summary>
        public new TabStop this[int index]
#nullable disable
                => (base[index] as TabStop)!; // HACK // BUG: May return null TODO: TabStop? Exception?
#nullable restore

        /// <summary>
        /// Gets a TabStop by its position. Returns null if no matching tab stop can be found.
        /// Note that Removed TabStops are also taken into account.
        /// </summary>
        public TabStop? GetTabStopAt(Unit position)
        {
            int count = Count;
            for (int index = 0; index < count; index++)
            {
                var tabStop = this[index];
                if (Math.Abs(tabStop.Position.Point - position.Point) < TabStopPrecision)
                    return tabStop;
            }
            return null;
        }

        /// <summary>
        /// Returns whether a TabStop exists at the given position.
        /// Note that Removed TabStops are also taken into account.
        /// </summary>
        public bool TabStopExists(Unit position)
        {
            return GetTabStopAt(position) != null;
        }

        /// <summary>
        /// Adds a TabStop object to the collection. If a TabStop with the same position
        /// already exists, it is replaced by the new TabStop.
        /// </summary>
        public TabStop AddTabStop(TabStop tabStop)
        {
            if (tabStop == null)
                throw new ArgumentNullException(nameof(tabStop));

            if (TabStopExists(tabStop.Position))
            {
                int index = IndexOf(GetTabStopAt(tabStop.Position));
                RemoveObjectAt(index);
                InsertObject(index, tabStop);
            }
            else
            {
                int count = Count;
                for (int index = 0; index < count; index++)
                {
                    if (tabStop.Position.Point < this[index].Position.Point)
                    {
                        InsertObject(index, tabStop);
                        return tabStop;
                    }
                }
                Add(tabStop);
            }
            return tabStop;
        }

        /// <summary>
        /// Adds a TabStop object at the specified position to the collection. If a TabStop with the
        /// same position already exists, it is replaced by the new TabStop.
        /// </summary>
        public TabStop AddTabStop(Unit position)
        {
            if (TabStopExists(position))
                return GetTabStopAt(position)!;  // Because it exists.

            var tab = new TabStop(position);
            return AddTabStop(tab);
        }

        /// <summary>
        /// Adds a TabStop object to the collection and sets its alignment and leader.
        /// </summary>
        public TabStop AddTabStop(Unit position, TabAlignment alignment, TabLeader leader)
        {
            var tab = AddTabStop(position);
            tab.Alignment = alignment;
            tab.Leader = leader;
            return tab;
        }

        /// <summary>
        /// Adds a TabStop object to the collection and sets its leader.
        /// </summary>
        public TabStop AddTabStop(Unit position, TabLeader leader)
        {
            var tab = AddTabStop(position);
            tab.Leader = leader;
            return tab;
        }

        /// <summary>
        /// Adds a TabStop object to the collection and sets its alignment.
        /// </summary>
        public TabStop AddTabStop(Unit position, TabAlignment alignment)
        {
            var tab = AddTabStop(position);
            tab.Alignment = alignment;
            return tab;
        }

        /// <summary>
        /// Adds a TabStop object to the collection marked to remove the tab stop at
        /// the given position.
        /// </summary>
        public void RemoveTabStop(Unit position)
        {
            var tab = AddTabStop(position);
            tab.AddTab = false;
        }

        /// <summary>
        /// Clears all TabStop objects from the collection. Additionally, 'TabStops = null'
        /// is written to the DDL stream when serialized.
        /// </summary>
        public void ClearAll()
        {
            Clear();
            TabsCleared = true;
        }

        /// <summary>
        /// Gets the information if the collection is marked as cleared. Additionally, 'TabStops = null'
        /// is written to the DDL stream when serialized.
        /// Further setting TabsCleared to true can be used to suppress inheriting TabStops, if needed for a user interface
        /// allowing interactive editing and just-in-time calculation of the effective style based on inheritance.
        /// This concept differs from Word’s object model, which seems to retain a list of TabStop positions to remove,
        /// if inherited TabStops are removed in Word’s Style editing dialog.
        /// </summary>
        public bool TabsCleared { get; set; }

        /// <summary>
        /// Indicates whether there are TabStops defined in this collection.
        /// </summary>
        public bool DefinesTabStops => this.Cast<TabStop>().Any(t => t.AddTab);

        /// <summary>
        /// Converts TabStops into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            if (TabsCleared)
                serializer.WriteLine("TabStops = null");

            int count = Count;
            for (int index = 0; index < count; index++)
            {
                var tabStop = this[index];
                tabStop.Serialize(serializer);
            }
        }

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        public override bool IsNull()
        {
            // Only non-empty and not cleared tab stops (TabStops = null) are null.
            if (base.IsNull())
                return !TabsCleared;
            return false;
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(TabStops));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public TabStopsValues Values => (TabStopsValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class TabStopsValues : Values
        {
            internal TabStopsValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
