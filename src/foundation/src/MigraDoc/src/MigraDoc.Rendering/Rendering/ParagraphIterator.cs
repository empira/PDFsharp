// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Iterates sequentially through the elements of a paragraph.
    /// </summary>
    class ParagraphIterator
    {
        /// <summary>
        /// Initializes a paragraph iterator pointing on the given paragraph elements object.
        /// Paragraph iterators received from this paragraph iterator relate to this root node.
        /// </summary>
        /// <param name="rootNode">The root node for the paragraph iterator.</param>
        internal ParagraphIterator(ParagraphElements rootNode)
        {
            _rootNode = rootNode;
            _current = rootNode;
            //_positionIndices = new List<int>();
        }

        /// <summary>
        /// Initializes a paragraph iterator given the root node, its position in the object tree and the current object.
        /// </summary>
        /// <param name="rootNode">The node the position indices relate to.</param>
        /// <param name="current">The element the iterator shall point to.</param>
        /// <param name="indices">The position of the paragraph iterator in terms of element indices.</param>
        ParagraphIterator(ParagraphElements rootNode, DocumentObject current, List<int> indices)
        {
            _rootNode = rootNode;
            _positionIndices = indices;
            _current = current;
        }

        /// <summary>
        /// Determines whether this iterator is the first leaf of the root node.
        /// </summary>
        internal bool IsFirstLeaf
        {
            get
            {
                if (_current is not DocumentElements)
                {
                    var prevIter = GetPreviousLeaf();
                    return prevIter == null;
                }
                return false;
            }
        }

        /// <summary>
        /// Determines whether this iterator is the last leaf of the document object tree.
        /// </summary>
        internal bool IsLastLeaf
        {
            get
            {
                if (_current is not DocumentElements)
                {
                    var nextIter = GetNextLeaf();
                    return nextIter == null;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the document object this instance points to.
        /// </summary>
        internal DocumentObject Current => _current;

        /// <summary>
        /// Gets the last leaf of the document object tree.
        /// </summary>
        /// <returns>The paragraph iterator pointing to the last leaf in the document object tree.</returns>
        internal ParagraphIterator? GetLastLeaf()
        {
            if (_rootNode.Count == 0)
                return null;
            return SeekLastLeaf();
        }

        /// <summary>
        /// Gets the first leaf of the element tree.
        /// </summary>
        /// <returns>The paragraph iterator pointing to the first leaf in the element tree.</returns>
        internal ParagraphIterator? GetFirstLeaf()
        {
            if (_rootNode.Count == 0)
                return null;
            return SeekFirstLeaf();
        }

        /// <summary>
        /// Returns the next iterator in the tree pointing to a leaf.
        /// </summary>
        /// <remarks>This function is intended to receive the renderable objects of a paragraph.
        /// Thus, empty ParagraphElement objects (which are collections) don’t count as leafs.</remarks>
        internal ParagraphIterator? GetNextLeaf()
        {
            // Move up to appropriate parent element.
            var parIterator = GetParentIterator();
            if (parIterator == null)
                return null;

            int elementIndex = LastIndex;
            var parEls = (ParagraphElements)parIterator._current;
            while (elementIndex == parEls.Count - 1)
            {
                elementIndex = parIterator.LastIndex;
                parIterator = parIterator.GetParentIterator();
                if (parIterator == null)
                    break;

                parEls = (ParagraphElements)parIterator._current;
            }
            if (parIterator == null)
                return null;
            int newIndex = elementIndex + 1;
            if (newIndex >= parEls.Count)
                return null;

            var indices = new List<int>(parIterator._positionIndices) { newIndex }; //(Array_List)parIterator.positionIndices.Clone();
            var obj = GetNodeObject(parEls[newIndex] ?? NRT.ThrowOnNull<DocumentObject>());
            var iterator = new ParagraphIterator(_rootNode, obj, indices);
            return iterator.SeekFirstLeaf();
        }

        /// <summary>
        /// Gets the object a paragraph iterator shall point to.
        /// Only ParagraphElements and renderable objects are allowed.
        /// </summary>
        /// <param name="obj">The object to select the node object for.</param>
        /// <returns>The object a paragraph iterator shall point to.</returns>
        DocumentObject GetNodeObject(DocumentObject obj)
        {
            if (obj is FormattedText text)
                return text.Elements;
            if (obj is Hyperlink hyperlink)
                return hyperlink.Elements;
            return obj;
        }

        /// <summary>
        /// Returns the previous iterator to a leaf in the document object tree pointing.
        /// </summary>
        /// <returns>The previous leaf, null if none exists.</returns>
        internal ParagraphIterator? GetPreviousLeaf()
        {
            // Move up to appropriate parent element.
            var parIterator = GetParentIterator();
            if (parIterator == null)
                return null;

            int elementIndex = LastIndex;
            var parEls = (ParagraphElements)parIterator._current;
            while (elementIndex == 0)
            {
                elementIndex = parIterator.LastIndex;
                parIterator = parIterator.GetParentIterator();
                if (parIterator == null)
                    break;

                parEls = (ParagraphElements)parIterator._current;
            }
            if (parIterator == null)
                return null;

            int newIndex = elementIndex - 1;
            if (newIndex < 0)
                return null;

            //List<int> indices = new(parIterator._positionIndices) { newIndex };
            List<int> indices = [..parIterator._positionIndices, newIndex];

            var obj = GetNodeObject(parEls[newIndex] ?? NRT.ThrowOnNull<DocumentObject>());
            var iterator = new ParagraphIterator(_rootNode, obj, indices);
            return iterator.SeekLastLeaf();
        }

        ParagraphIterator SeekLastLeaf()  // TODO NOTE ReSharper has a bug here (use original code). Introducing pattern variable breaks the code. Should report that to JetBrains.
        {
#if true
            var obj = Current;
            if (obj is not ParagraphElements)
                return this;

            var indices = new List<int>(_positionIndices);
            while (obj is ParagraphElements elements)
            {
                if (elements.Count == 0)
                    return new ParagraphIterator(_rootNode, elements, indices);

                int idx = elements.Count - 1;
                indices.Add(idx);
                obj = GetNodeObject(elements[idx] ?? NRT.ThrowOnNull<DocumentObject>());
            }
            return new ParagraphIterator(_rootNode, obj, indices);
#else  // Keep as reference
            DocumentObject obj = Current;
            if (!(obj is ParagraphElements))
                return this;

            List<int> indices = new List<int>(_positionIndices);

            while (obj is ParagraphElements)
            {
                ParagraphElements parEls = (ParagraphElements)obj;
                if (((ParagraphElements)obj).Count == 0)
                    return new ParagraphIterator(_rootNode, obj, indices);

                int idx = ((ParagraphElements)obj).Count - 1;
                indices.Add(idx);
                obj = GetNodeObject(parEls[idx]);
            }
            return new ParagraphIterator(_rootNode, obj, indices);
#endif
        }

        /// <summary>
        /// Gets the leftmost leaf within the hierarchy.
        /// </summary>
        /// <returns>The searched leaf.</returns>
        ParagraphIterator SeekFirstLeaf() // TODO NOTE ReSharper has a bug here (see above).
        {
#if true
            var obj = Current;
            if (obj is not ParagraphElements)
                return this;

            var indices = new List<int>(_positionIndices);
            while (obj is ParagraphElements parEls)
            {
                if (parEls.Count == 0)
                    return new ParagraphIterator(_rootNode, obj, indices);

                indices.Add(0);
                obj = GetNodeObject(parEls[0] ?? NRT.ThrowOnNull<DocumentObject>());
            }
            return new ParagraphIterator(_rootNode, obj, indices);
#else  // Keep as reference
            DocumentObject obj = Current;
            if (!(obj is ParagraphElements))
                return this;
            List<int> indices = new List<int>(_positionIndices);

            while (obj is ParagraphElements)
            {
                ParagraphElements parEls = (ParagraphElements)obj;
                if (parEls.Count == 0)
                    return new ParagraphIterator(_rootNode, obj, indices);

                indices.Add(0);
                obj = GetNodeObject(parEls[0]);
            }
            return new ParagraphIterator(_rootNode, obj, indices);
#endif
        }

        ParagraphIterator? GetParentIterator()
        {
            if (_positionIndices.Count == 0)
                return null;

            var indices = new List<int>(_positionIndices);
            indices.RemoveAt(indices.Count - 1);
            var parent = DocumentRelations.GetParentOfType(_current, typeof(ParagraphElements))
                ?? NRT.ThrowOnNull<DocumentObject>();
            return new ParagraphIterator(_rootNode, parent, indices);
        }

        int LastIndex
        {
            get
            {
                if (_positionIndices.Count != 0)
                    return _positionIndices[^1];
                return -1;
            }
        }

        readonly ParagraphElements _rootNode;
        readonly List<int> _positionIndices = new();
        readonly DocumentObject _current;
    }
}
