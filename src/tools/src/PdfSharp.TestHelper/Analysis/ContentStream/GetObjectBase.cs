namespace PdfSharp.TestHelper.Analysis.ContentStream
{
    /// <summary>
    /// Abstract class for iterating specific elements of a content stream for easier inspection.
    /// </summary>
    public abstract class GetObjectBase<T>
    {
        protected GetObjectBase(ContentStreamEnumerator contentStreamEnumerator)
        {
            _contentStreamEnumerator = contentStreamEnumerator;
        }

        /// <summary>
        /// Moves to the previous or next T element inside the content stream and returns it.
        /// </summary>
        /// <param name="steps">Moves this count of elements.</param>
        /// <param name="countThisTypeOnly">Counts only elements of type T.</param>
        /// <param name="backwards">True, if checking previous elements.</param>
        /// <param name="obj">The loaded object.</param>
        public bool MoveAndGet(int steps, bool countThisTypeOnly, bool backwards, out T? obj)
        {
            return _contentStreamEnumerator.MoveAndGet(IdentifyingElementCheck, steps, countThisTypeOnly, backwards, GetObject, out obj);
        }

        /// <summary>
        /// Moves to the next T element inside the content stream and returns it.
        /// </summary>
        /// <param name="nextOfThisType">The element must not be a direct neighbor.</param>
        /// <param name="obj">The loaded object.</param>
        public Boolean MoveAndGetNext(bool nextOfThisType, out T? obj)
        {
            return MoveAndGet(1, nextOfThisType, false, out obj);
        }

        /// <summary>
        /// Moves to the previous T element inside the content stream and returns it.
        /// </summary>
        /// <param name="previousOfThisType">The element must not be a direct neighbor.</param>
        /// <param name="obj">The loaded object.</param>
        public Boolean MoveAndGetPrevious(bool previousOfThisType, out T? obj)
        {
            return MoveAndGet(1, previousOfThisType, true, out obj);
        }

        /// <summary>
        /// Moves to the next T element inside the content stream and returns it.
        /// </summary>
        /// <param name="steps">Moves this count of elements.</param>
        /// <param name="countThisTypeOnly">Counts only elements of type T.</param>
        /// <param name="obj">The loaded object.</param>
        public bool MoveAndGetNext(int steps, bool countThisTypeOnly, out T? obj)
        {
            return MoveAndGet(steps, countThisTypeOnly, false, out obj);
        }

        /// <summary>
        /// Moves to the previous T element inside the content stream and returns it.
        /// </summary>
        /// <param name="steps">Moves this count of elements.</param>
        /// <param name="countThisTypeOnly">Counts only elements of type T.</param>
        /// <param name="obj">The loaded object.</param>
        public bool MoveAndGetPrevious(int steps, bool countThisTypeOnly, out T? obj)
        {
            return MoveAndGet(steps, countThisTypeOnly, true, out obj);
        }

        /// <summary>
        /// Moves to the next T element inside the content stream and returns it.
        /// </summary>
        /// <param name="check">The element must satisfy this check.</param>
        /// <param name="countMatchesOnly">Counts only elements of type T check returns true for.</param>
        /// <param name="obj">The loaded object.</param>
        public bool MoveAndGetNext(Func<T, bool> check, bool countMatchesOnly, out T? obj)
        {
            return MoveAndGet(check, 1, countMatchesOnly, false, out obj);
        }

        /// <summary>
        /// Moves to the previous T element inside the content stream and returns it.
        /// </summary>
        /// <param name="check">The element must satisfy this check.</param>
        /// <param name="countMatchesOnly">Counts only elements of type T check returns true for.</param>
        /// <param name="obj">The loaded object.</param>
        public bool MoveAndGetPrevious(Func<T, bool> check, bool countMatchesOnly, out T? obj)
        {
            return MoveAndGet(check, 1, countMatchesOnly, true, out obj);
        }

        /// <summary>
        /// Moves to the next T element inside the content stream and returns it.
        /// </summary>
        /// <param name="check">The element must satisfy this check.</param>
        /// <param name="steps">Moves this count of elements.</param>
        /// <param name="countMatchesOnly">Counts only elements of type T check returns true for.</param>
        /// <param name="obj">The loaded object.</param>
        public bool MoveAndGetNext(Func<T, bool> check, int steps, bool countMatchesOnly, out T? obj)
        {
            return MoveAndGet(check, steps, countMatchesOnly, false, out obj);
        }

        /// <summary>
        /// Moves to the previous T element inside the content stream and returns it.
        /// </summary>
        /// <param name="check">The element must satisfy this check.</param>
        /// <param name="steps">Moves this count of elements.</param>
        /// <param name="countMatchesOnly">Counts only elements of type T check returns true for.</param>
        /// <param name="obj">The loaded object.</param>
        public bool MoveAndGetPrevious(Func<T, bool> check, int steps, bool countMatchesOnly, out T? obj)
        {
            return MoveAndGet(check, steps, countMatchesOnly, true, out obj);
        }

        /// <summary>
        /// Moves to the previous or next T element inside the content stream and returns it.
        /// </summary>
        /// <param name="check">The element must satisfy this check.</param>
        /// <param name="steps">Moves this count of elements.</param>
        /// <param name="countMatchesOnly">Counts only elements of type T check returns true for.</param>
        /// <param name="backwards">True, if checking previous elements.</param>
        /// <param name="obj">The loaded object.</param>
        bool MoveAndGet(Func<T, bool> check, int steps, bool countMatchesOnly, bool backwards, out T? obj)
        {
            var cachedState = _contentStreamEnumerator.GetState();

            if (countMatchesOnly)
            {
                // Move x steps and count only elements satisfying check.
                var matchCount = 0;
                while (true)
                {
                    // Break, if no more elements of this type.
                    if (!MoveAndGet(1, true, backwards, out obj))
                        break;

                    if (check(obj!))
                    {
                        // If check satisfies, increase matchCount, but return true only if this is match number x.
                        if (++matchCount == steps)
                            return true;
                    }
                }
            }
            else
            {
                var stepsBefore = steps - 1;
                // If steps - 1 is bigger than 0, try to move by them through all elements to get the direct neighbor.
                if (stepsBefore <= 0 || _contentStreamEnumerator.Move(_ => true, steps, false, backwards))
                {
                    // From the direct neighbor try to get the previous or next element, which shall be of this type.
                    if (MoveAndGet(1, false, backwards, out obj))
                    {
                        // If check satisfies, return true.
                        if (check(obj!))
                            return true;
                    }
                }
                obj = default;
            }

            // Without success, we restore the cached state.
            _contentStreamEnumerator.SetState(cachedState);
            return false;
        }

        protected abstract bool IdentifyingElementCheck(string element);

        protected abstract (bool, T?) GetObject();

        protected (bool, T?) GetObjectFailed()
        {
            return new ValueTuple<Boolean, T?>(false, default);
        }

        protected readonly ContentStreamEnumerator _contentStreamEnumerator;
    }
}
