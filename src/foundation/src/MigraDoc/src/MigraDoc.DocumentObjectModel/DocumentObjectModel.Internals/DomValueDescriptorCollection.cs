// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Collections;

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// A collection that manages ValueDescriptors.
    /// </summary>
    public class ValueDescriptorCollection : IEnumerable<ValueDescriptor>
    {
        /// <summary>
        /// Gets the count of ValueDescriptors.
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// Adds the specified ValueDescriptor.
        /// </summary>
        public void Add(ValueDescriptor vd)
        {
            _dictionary.Add(vd.ValueName, vd);
            _list.Add(vd);
        }

        /// <summary>
        /// Gets the <see cref="MigraDoc.DocumentObjectModel.Internals.ValueDescriptor"/> at the specified index.
        /// </summary>
        public ValueDescriptor this[int index]
            => _list[index];

        /// <summary>
        /// Gets the <see cref="MigraDoc.DocumentObjectModel.Internals.ValueDescriptor"/> with the specified name.
        /// </summary>
        public ValueDescriptor this[string name]
            => _dictionary[name];

        /// <summary>
        /// Determines whether this value descriptor collection contains a value with the specified name.
        /// </summary>
        public bool HasName(string name)
            => _dictionary.TryGetValue(name, out _);

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<ValueDescriptor> GetEnumerator()
            => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// The list of all value descriptors.
        /// </summary>
        readonly List<ValueDescriptor> _list = new();

        /// <summary>
        /// The dictionary with all value descriptors by name.
        /// </summary>
        readonly Dictionary<string, ValueDescriptor> _dictionary = new(StringComparer.OrdinalIgnoreCase);
    }
}
