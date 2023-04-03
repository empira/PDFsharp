namespace PdfSharp.Pdf
{
    /// <summary>
    /// Provides methods to handle keys that may contain a PdfArray or a single PdfItem.
    /// </summary>
    public class ArrayOrSingleItemHelper
    {
        /// <summary>
        /// Initializes ArrayOrSingleItemHelper with PdfDictionary.DictionaryElements to work with.
        /// </summary>
        public ArrayOrSingleItemHelper(PdfDictionary.DictionaryElements elements)
        {
            _elements = elements;
            _dictionary = elements.Owner;
        }

        /// <summary>
        /// Adds a PdfItem to the given key.
        /// Creates a PdfArray containing the items, if needed.
        /// </summary>
        /// <param name="key">The key in the dictionary to work with.</param>
        /// <param name="value">The PdfItem to add.</param>
        /// <param name="prepend">True, if value shall be prepended instead of appended.</param>
        public void Add(string key, PdfItem value, bool prepend = false)
        {
            var obj = _elements[key];

            var array = obj as PdfArray;

            // If key is not yet set or key contains array without elements, assign value directly to key.
            if (obj is null || array is not null && array.Elements.Count == 0)
            {
                _elements[key] = value;
                return;
            }

            // If not yet existing, create an array and assign the current directly assigned obj.
            if (array is null)
            {
                array = new PdfArray(_dictionary._document, obj);
                _elements[key] = array;
            }

            // Add value to the array.
            if (prepend)
                array.Elements.Insert(0, value);
            else
                array.Elements.Add(value);
        }


        /// <summary>
        /// Gets all PdfItems saved in the given key.
        /// </summary>
        /// <param name="key">The key in the dictionary to work with.</param>
        public IEnumerable<PdfItem> GetAll(string key)
        {
            var obj = _elements[key];

            if (obj is PdfArray array)
            {
                foreach (var item in array.Elements)
                    yield return item;
            }
            else if (obj is not null)
                yield return obj;
        }


        IEnumerable<PdfItem> Get(string key, Func<PdfItem, bool> predicate)
        {
            return GetAll(key).Where(predicate);
        }

        /// <summary>
        /// Gets the PdfItem(s) of type T saved in the given key, that match a predicate.
        /// </summary>
        /// <param name="key">The key in the dictionary to work with.</param>
        /// <param name="predicate">The predicate, that shall be true for the desired item(s).</param>
        public IEnumerable<T> Get<T>(string key, Func<T, bool> predicate) where T : PdfItem
        {
            return Get(key, x => x is T xT && predicate(xT)).Cast<T>();
        }

        /// <summary>
        /// Gets the PdfItem(s) of type T saved in the given key, that are equal to value.
        /// </summary>
        /// <param name="key">The key in the dictionary to work with.</param>
        /// <param name="value">The value, the desired item(s) shall be equal to.</param>
        // Allows to call Equals with object.
        public IEnumerable<T> Get<T>(string key, object value) where T : PdfItem
        {
            return Get<T>(key, x => x.Equals(value));
        }

        /// <summary>
        /// Gets the PdfItem(s) of type T saved in the given key, that are equal to value.
        /// </summary>
        /// <param name="key">The key in the dictionary to work with.</param>
        /// <param name="value">The value, the desired item(s) shall be equal to.</param>
        // Allows to omit the type parameter in the call.
        public IEnumerable<T> Get<T>(string key, T value) where T : PdfItem
        {
            return Get<T>(key, x => x.Equals(value));
        }


        /// <summary>
        /// Returns true if the given key contains a PdfItem of type T matching a predicate.
        /// </summary>
        /// <param name="key">The key in the dictionary to work with.</param>
        /// <param name="predicate">The predicate, that shall be true for the desired item(s).</param>
        public bool Contains<T>(string key, Func<T, bool> predicate) where T : PdfItem
        {
            return Get(key, x => x is T xT && predicate(xT)).Any();
        }

        /// <summary>
        /// Returns true if the given key contains a PdfItem of type T, that is equal to value.
        /// </summary>
        /// <param name="key">The key in the dictionary to work with.</param>
        /// <param name="value">The value, the desired item(s) shall be equal to.</param>
        // Allows to call Equals with object.
        public bool Contains<T>(string key, object value) where T : PdfItem
        {
            return Contains<T>(key, x => x.Equals(value));
        }

        /// <summary>
        /// Returns true if the given key contains a PdfItem of type T, that is equal to value.
        /// </summary>
        /// <param name="key">The key in the dictionary to work with.</param>
        /// <param name="value">The value, the desired item(s) shall be equal to.</param>
        // Allows to omit the type parameter in the call.
        public bool Contains<T>(string key, T value) where T : PdfItem
        {
            return Contains<T>(key, x => x.Equals(value));
        }


        bool Remove(string key, Func<PdfItem, bool> predicate)
        {
            var obj = _elements[key];

            if (obj is null)
                return false;

            // On array, iterate its elements...
            if (obj is PdfArray array)
            {
                var itemsRemoved = false;
                var arrayElements = array.Elements;

                for (var i = 0; i < arrayElements.Count; i++)
                {
                    var item = arrayElements[i];
                    // Remove matching elements.
                    if (predicate(item))
                    {
                        itemsRemoved = true;
                        arrayElements.RemoveAt(i--);
                    }
                }

                // If array is empty now, remove the key.
                if (arrayElements.Count == 0)
                    _elements.Remove(key);
                // If array contains only one element now, assign it directly to the key.
                else if (arrayElements.Count == 1)
                    _elements[key] = arrayElements[0];

                return itemsRemoved;
            }

            // ...otherwise check obj itself.
            if (predicate(obj))
                return _elements.Remove(key);

            return false;
        }

        /// <summary>
        /// Removes the PdfItem(s) of type T saved in the given key, that match a predicate.
        /// Removes the PdfArray, if no longer needed.
        /// Returns true if items were removed.
        /// </summary>
        /// <param name="key">The key in the dictionary to work with.</param>
        /// <param name="predicate">The predicate, that shall be true for the desired item(s).</param>
        public bool Remove<T>(string key, Func<T, bool> predicate) where T : PdfItem
        {
            return Remove(key, x => x is T xT && predicate(xT));
        }

        /// <summary>
        /// Removes the PdfItem(s) of type T saved in the given key, that are equal to value.
        /// Removes the PdfArray, if no longer needed.
        /// Returns true if items were removed.
        /// </summary>
        /// <param name="key">The key in the dictionary to work with.</param>
        /// <param name="value">The value, the desired item(s) shall be equal to.</param>
        // Allows to call Equals with object.
        public bool Remove<T>(string key, object value) where T : PdfItem
        {
            return Remove<T>(key, x => x.Equals(value));
        }

        /// <summary>
        /// Removes the PdfItem(s) of type T saved in the given key, that are equal to value.
        /// Removes the PdfArray, if no longer needed.
        /// Returns true if items were removed.
        /// </summary>
        /// <param name="key">The key in the dictionary to work with.</param>
        /// <param name="value">The value, the desired item(s) shall be equal to.</param>
        // Allows to omit the type parameter in the call.
        public bool Remove<T>(string key, T value) where T : PdfItem
        {
            return Remove<T>(key, x => x.Equals(value));
        }

        readonly PdfDictionary _dictionary;
        readonly PdfDictionary.DictionaryElements _elements;
    }
}
