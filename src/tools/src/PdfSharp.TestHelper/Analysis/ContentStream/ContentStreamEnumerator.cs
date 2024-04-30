// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;
using System.Diagnostics;
using PdfSharp.Pdf;

namespace PdfSharp.TestHelper.Analysis.ContentStream
{
    /// <summary>
    /// Iterates the elements of a content stream for easier inspection.
    /// </summary>
    public class ContentStreamEnumerator : IEnumerator<string?>
    {
        public ContentStreamEnumerator(string contentStream, PdfDocument pdfDocument)
        {
            _contentStream = contentStream;
            PdfDocument = pdfDocument;
            _contentStreamLength = contentStream.Length;

            _state = new State(this);

            // As new State is initialized with a CurrentElementIndex of -1, this moves to the first element.
            MoveNext();
        }

        public PdfDocument PdfDocument { get; }

        /// <summary>
        /// Returns the current element.
        /// </summary>
        public string? Current => _state.Current;

        /// <summary>
        /// Moves to the next white space separated element inside the content stream.
        /// </summary>
        public bool MoveNext()
        {
            var cachedState = GetState();

            var elementIdx = _state.CurrentElementIndex + 1;

            // Check, if the next element is already read.
            // If not, read and save it.
            if (!_elements.TryGetValue(elementIdx, out var elementBase))
            {
                var position = _state.CurrentPosition + _state.CurrentLength; // Start after the current element.
                var skipOneNecessaryWhiteSpace = position > 0; // For the first element, there must not be a separating white space before.

                // Try to move to the beginning of the next element.
                var hasNextElement = MoveAfterWhiteSpaces(ref position, skipOneNecessaryWhiteSpace);

                // If the beginning of a next element is found create and save it. Otherwise, "close" _elements with an EndOfStreamElement.
                elementBase = hasNextElement ? CreateElement(position) : new EndOfStreamElement();

                _elements.Add(elementIdx, elementBase);
            }

            // For an element, set current and return true, but not for EndOfStreamElement.
            if (elementBase is Element element)
            {
                SetCurrent(elementIdx, element);
                return true;
            }

            // Without success, we restore the cached state.
            SetState(cachedState);
            return false;
        }

        /// <summary>
        /// Moves to the previous white space separated element inside the content stream.
        /// </summary>
        public bool MovePrevious()
        {
            // If we are at the first element, there's no other previous to it.
            if (_state.CurrentElementIndex == 0)
                return false;

            var elementIdx = _state.CurrentElementIndex - 1;

            // We expect all previous element to be read already.
            if (!_elements.TryGetValue(elementIdx, out var elementBase) || elementBase is not Element element)
                throw new KeyNotFoundException("There's an error in the implementation of ContentStreamEnumerator. " +
                                               "For all elements accessible by MovePrevious() the ElementPositions must be cached by a further call of MoveNext().");

            SetCurrent(elementIdx, element);

            return true;
        }

        /// <summary>
        /// Moves to the next white space separated element inside the content stream.
        /// </summary>
        /// <param name="steps">Moves this count of elements.</param>
        public bool MoveNext(int steps)
        {
            return Move(_ => true, steps, false, false);
        }

        /// <summary>
        /// Moves to the next white space separated element inside the content stream.
        /// </summary>
        /// <param name="check">The element must satisfy this check.</param>
        /// <param name="nextMatch">The element must not be a direct neighbor.</param>
        public bool MoveNext(Func<string, bool> check, bool nextMatch)
        {
            return Move(check, 1, nextMatch, false);
        }

        /// <summary>
        /// Moves to the next white space separated element inside the content stream.
        /// </summary>
        /// <param name="check">The element must satisfy this check.</param>
        /// <param name="steps">Moves this count of elements.</param>
        /// <param name="countMatchesOnly">Counts only elements check returns true for.</param>
        public bool MoveNext(Func<string, bool> check, int steps, bool countMatchesOnly)
        {
            return Move(check, steps, countMatchesOnly, false);
        }

        /// <summary>
        /// Moves to the previous white space separated element inside the content stream.
        /// </summary>
        /// <param name="steps">Moves this count of elements.</param>
        public bool MovePrevious(int steps)
        {
            return Move(_ => true, steps, false, true);
        }

        /// <summary>
        /// Moves to the previous white space separated element inside the content stream.
        /// </summary>
        /// <param name="check">The element must satisfy this check.</param>
        /// <param name="previousMatch">The element must not be a direct neighbor.</param>
        public bool MovePrevious(Func<string, bool> check, bool previousMatch)
        {
            return Move(check, 1, previousMatch, true);
        }

        /// <summary>
        /// Moves to the previous white space separated element inside the content stream.
        /// </summary>
        /// <param name="check">The element must satisfy this check.</param>
        /// <param name="steps">Moves this count of elements.</param>
        /// <param name="countMatchesOnly">Counts only elements check returns true for.</param>
        public bool MovePrevious(Func<string, bool> check, int steps, bool countMatchesOnly)
        {
            return Move(check, steps, countMatchesOnly, false);
        }

        /// <summary>
        /// Moves to the previous or next white space separated element inside the content stream and returns it.
        /// </summary>
        /// <param name="check">The element must satisfy this check.</param>
        /// <param name="steps">Moves this count of elements.</param>
        /// <param name="countMatchesOnly">Counts only elements check returns true for.</param>
        /// <param name="backwards">True, if checking previous elements.</param>
        public bool Move(Func<string, bool> check, int steps, bool countMatchesOnly, bool backwards)
        {
            // Cache the correct move function.
            Func<bool> nextOrPrevious;
            if (backwards)
                nextOrPrevious = MovePrevious;
            else
                nextOrPrevious = MoveNext;

            var cachedState = GetState();

            if (countMatchesOnly)
            {
                // Move x steps and count only elements satisfying check.
                var matchCount = 0;
                while (true)
                {
                    // Break, if no more elements.
                    if (!nextOrPrevious())
                        break;

                    if (check(Current!))
                    {
                        // If check satisfies, increase matchCount, but return true only if this is match number x.
                        if (++matchCount == steps)
                            return true;
                    }
                }
            }
            else
            {
                // Move x steps and count all elements.
                var stepsCount = 0;
                while (true)
                {
                    // Break, if no more elements.
                    if (!nextOrPrevious())
                        break;

                    // Increase stepsCount, but do check only, if this is element number x.
                    if (++stepsCount == steps)
                    {
                        // If check satisfies, return true
                        if (check(Current!))
                            return true;

                        // If not, there is not the desired element at the specified position.
                        break;
                    }
                }
            }

            // Without success, we restore the cached state.
            SetState(cachedState);
            return false;
        }

        /// <summary>
        /// Moves to the previous or next white space separated element inside the content stream and returns it.
        /// </summary>
        /// <param name="check">The element must satisfy this check.</param>
        /// <param name="steps">Moves this count of elements.</param>
        /// <param name="countMatchesOnly">Counts only elements check returns true for.</param>
        /// <param name="backwards">True, if checking previous elements.</param>
        /// <param name="getObject">A function to load the whole object, if the searched element is found.</param>
        /// <param name="obj">The loaded object.</param>
        public bool MoveAndGet<T>(Func<string, bool> check, int steps, bool countMatchesOnly, bool backwards, Func<(bool success, T obj)> getObject, out T? obj)
        {
            obj = default;
            var cachedState = GetState();

            if (Move(check, steps, countMatchesOnly, backwards))
            {
                var getObjectResult = getObject();
                if (getObjectResult.success)
                {
                    obj = getObjectResult.obj;
                    return true;
                }
            }

            SetState(cachedState);
            return false;
        }

        /// <summary>
        /// Gets a clone of the current state for analysis or restoring.
        /// </summary>
        public IState GetState()
        {
            return _state.Clone();
        }

        /// <summary>
        /// Restores the given state.
        /// </summary>
        public void SetState(IState state)
        {
            if (state is not State stateImplementation || stateImplementation.Enumerator != this)
                throw new ArgumentException("This state is not a valid state of this ContentStreamEnumerator.");

            _state = stateImplementation;
        }

        public void Reset()
        {
            _state = new State(this);
        }

        /// <summary>
        /// Provides functions to load text objects for further inspection.
        /// </summary>
        public GetText Text => _text ??= new GetText(this);
        GetText? _text;

        /// <summary>
        /// Provides functions to load line objects for further inspection.
        /// </summary>
        public GetLine Line => _line ??= new GetLine(this);
        GetLine? _line;

        public void Dispose()
        { }


        object? IEnumerator.Current => Current;

        bool MoveAfterWhiteSpaces(ref int position, bool moveByOneNecessaryWhiteSpace)
        {
            // If we have to move by one white space, return false, if it was not found.
            if (moveByOneNecessaryWhiteSpace)
            {
                // There is no white space after the last character.
                if (position >= _contentStreamLength)
                    return false;

                var isWhiteSpace = IsWhiteSpace(_contentStream[position]);
                if (isWhiteSpace)
                    position++;
                else
                    return false;
            }

            // Move after the last white space.
            while (true)
            {
                if (position >= _contentStreamLength)
                    break;

                var isWhiteSpace = IsWhiteSpace(_contentStream[position]);
                if (isWhiteSpace)
                    position++;
                else
                    return true;
            }

            // There is no element after these white spaces.
            return false;
        }

        bool IsWhiteSpace(char c)
        {
            return _whiteSpaceChars.Contains(c);
        }

        int GetLengthOfCurrentElement(int position)
        {
            var startPosition = position;
            var c = _contentStream[position];
            var isEscaped = false;

            switch (c)
            {
                case '(': // Literal string.
                    var openBracketCount = 1;
                    c = _contentStream[++position];

                    while (true)
                    {
                        // At the end of the stream return the rest.
                        if (position >= _contentStreamLength)
                            return position - startPosition;

                        if (!isEscaped)
                        {
                            if (c == '(')
                                openBracketCount++;
                            else if (c == ')')
                            {
                                openBracketCount--;

                                // If all open brackets are closed, get the length including the closing bracket.
                                if (openBracketCount == 0)
                                    return position + 1 - startPosition;
                            }
                            else if (c == '\\')
                                isEscaped = true; // A following bracket has not to be counted.
                        }
                        else
                            isEscaped = false; // If this char is escaped, reset isEscaped for the next char.

                        c = _contentStream[++position];
                    }
                case '<': // Hex string.
                    c = _contentStream[++position];
                    while (true)
                    {
                        // At the end of the stream return the rest.
                        if (position >= _contentStreamLength)
                            return position - startPosition;

                        // If the bracket is closed, get the length including the closing bracket.
                        if (c == '>')
                            return position + 1 - startPosition;

                        c = _contentStream[++position];
                    }
                default: // Other element.
                    while (true)
                    {
                        // At the end of the stream return the rest.
                        if (position >= _contentStreamLength)
                            return position - startPosition;

                        // Other elements are separated by white spaces.
                        if (IsWhiteSpace(c))
                            return position - startPosition;

                        // Reached the end of the string. There is no white space after the last character.
                        if (position + 1 >= _contentStreamLength)
                            return position - startPosition + 1;

                        c = _contentStream[++position];
                    }
            }
        }

        Element CreateElement(int position)
        {
            var length = GetLengthOfCurrentElement(position);
            var str = _contentStream.Substring(position, length);

            var element = new Element(position, str);
            return element;
        }

        void SetCurrent(int elementIndex, Element element)
        {
            var oldState = GetState();
            
            // Don't set state if there's no change.
            if (elementIndex == oldState.CurrentElementIndex)
            {
                Debug.Assert(element.Position == oldState.CurrentPosition && element.String == oldState.Current);
                return;
            }
            
            _state.Set(elementIndex, element);
        }

        readonly string _contentStream;
        readonly int _contentStreamLength;

        readonly Dictionary<int, ElementBase> _elements = []; // The elements found inside the content stream.
        readonly char[] _whiteSpaceChars = [' ', '\n', '\r', '\t'];

        State _state;


        class ElementBase
        { }

        /// <summary>
        /// A found element inside the content stream.
        /// </summary>
        class Element : ElementBase
        {
            /// <summary>
            /// the position inside the stream.
            /// </summary>
            public int Position { get; }

            /// <summary>
            /// The element.
            /// </summary>
            public string String { get; }

            public Element(int position, string s)
            {
                Position = position;
                String = s;
            }
        }

        /// <summary>
        /// "Closes" the _elements dictionary if the end of the stream is reached.
        /// </summary>
        class EndOfStreamElement : ElementBase
        { }

        public interface IState
        {
            /// <summary>
            /// The current position in the content stream.
            /// </summary>
            public int CurrentPosition { get; }

            /// <summary>
            /// The current index of the element in the content stream.
            /// </summary>
            public int CurrentElementIndex { get; }

            /// <summary>
            /// The current element.
            /// </summary>
            public string? Current { get; }

            /// <summary>
            /// The length of the current element.
            /// </summary>
            public int CurrentLength { get; }

            public IState Clone();
        }

        class State : IState
        {
            public int CurrentPosition { get; private set; }

            public int CurrentElementIndex { get; private set; } = -1;

            public string? Current { get; private set; }

            public int CurrentLength { get; private set; }

            public ContentStreamEnumerator Enumerator { get; }

            public State(ContentStreamEnumerator enumerator)
            {
                Enumerator = enumerator;
            }

            public void Set(int elementIndex, Element element)
            {
                CurrentElementIndex = elementIndex;
                CurrentPosition = element.Position;
                Current = element.String;
                CurrentLength = Current?.Length ?? 0;
            }

            public IState Clone()
            {
                return new State(Enumerator)
                {
                    CurrentPosition = CurrentPosition,
                    CurrentElementIndex = CurrentElementIndex,
                    Current = Current,
                    CurrentLength = CurrentLength
                };
            }
        }
    }
}
