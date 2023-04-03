// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Collections;

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// Used to collect errors reported by the DDL parser.
    /// </summary>
    public class DdlReaderErrors : IEnumerable
    {
        /// <summary>
        /// Adds the specified DdlReaderError at the end of the error list.
        /// </summary>
        public void AddError(DdlReaderError error) => _errors.Add(error);

        /// <summary>
        /// Gets the DdlReaderError at the specified position.
        /// </summary>
        public DdlReaderError this[int index] => _errors[index];

        /// <summary>
        /// Gets the number of messages that are errors.
        /// </summary>
        public int ErrorCount
        {
            get
            {
                int count = 0;
                for (int idx = 0; idx < _errors.Count; idx++)
                {
                    if (_errors[idx].ErrorLevel == DdlErrorLevel.Error)
                        count++;
                }
                return count;
            }
        }
        readonly List<DdlReaderError> _errors = new();

        /// <summary>
        /// Returns an enumerator that iterates through the error collection.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return _errors.GetEnumerator();
        }
    }
}
