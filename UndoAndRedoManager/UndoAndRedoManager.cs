
namespace UndoAndRedoManager
{
    using System;
    using System.Collections.Generic;

    public class UndoAndRedoManager<T> where T : IComparable<T>, IValidatable<T>
    {
        private IList<T> theList;
        private int start;
        private int end;
        private int current;

        private const int InitCurrent = -1;
        private const int DefaultCapacity = 100;

        /// <summary>
        /// Get whether there are any elements behind the current element
        /// </summary>
        public bool CanBack => this.current > this.start;

        /// <summary>
        /// Get whether there are any elements after the current element
        /// </summary>
        public bool CanForword => this.current >= 0 && this.current < this.end;

        /// <summary>
        /// Get the valid element length
        /// </summary>
        public int ValidElementLength => this.current == InitCurrent ? 0 : this.end - this.start + 1;

        /// <summary>
        /// Occurs when the CanBack property changed
        /// </summary>
        public event EventHandler<bool> CanBackChange;

        /// <summary>
        /// Occurs when the CanForword property changed
        /// </summary>
        public event EventHandler<bool> CanForwordChange;

        /// <summary>
        /// The construction
        /// </summary>
        /// <param name="length"> 
        /// The length of internal stack. Zero for an unlimit size, greater than zero for a limit size, default is 100
        /// </param>
        public UndoAndRedoManager(int length = DefaultCapacity)
        {
            this.theList = length == 0 ? (IList<T>)new List<T>(DefaultCapacity) : new T[length];
            Clear();
        }

        /// <summary>
        /// Try to insert an element next to the current pointer
        /// </summary>
        /// <param name="obj"> The element object to be inserted </param>
        /// <returns>
        /// True if the insert action succeeded
        /// False if the input element is null, or is invalid, or is the same with current element in stack, all of which will fail the insert action
        /// </returns>
        public bool TryInsert(T obj)
        {
            var res = obj != null && obj.IsValid() && (this.current == InitCurrent || this.theList.Count == 0 || obj.CompareTo(this.theList[this.current % this.theList.Count]) != 0);
            if (res)
            {
                var oldCanBack = this.CanBack;
                var oldCanForword = this.CanForword;

                if (this.theList.IsReadOnly)
                {
                    this.theList[++this.current % this.theList.Count] = obj;
                    if (this.current - this.start == this.theList.Count)
                    {
                        this.start++;
                    }
                }
                else
                {
                    if (this.current == this.theList.Count - 1)
                    {
                        this.theList.Add(obj);
                        this.current++;
                    }
                    else
                    {
                        this.theList[++this.current] = obj;
                    }
                }
                this.end = this.current;

                if (oldCanBack != this.CanBack) { this.CanBackChange?.Invoke(this, this.CanBack); }
                if (oldCanForword != this.CanForword) { this.CanForwordChange?.Invoke(this, this.CanForword); }
            }

            return res;
        }

        /// <summary>
        /// Try back / undo an element
        /// </summary>
        /// <param name="obj"> The previous element if there is a valid one, otherwise default value of T </param>
        /// <returns> True if the trying succeeded, otherwise false </returns>
        public bool TryBack(out T obj)
        {
            if (this.CanBack)
            {
                var oldCanForword = this.CanForword;

                obj = this.theList[--this.current % this.theList.Count];

                if (!this.CanBack) { this.CanBackChange?.Invoke(this, false); }
                if (oldCanForword != this.CanForword) { this.CanForwordChange?.Invoke(this, this.CanForword); }

                return true;
            }
            else
            {
                obj = default(T);
                return false;
            }
        }

        /// <summary>
        /// Try forward / redo an element
        /// </summary>
        /// <param name="obj"> The next element if there is a valid one, otherwise default value of T </param>
        /// <returns> True if the trying succeeded, otherwise false </returns>
        public bool TryForword(out T obj)
        {
            if (this.CanForword)
            {
                var oldCanBack = this.CanBack;

                obj = this.theList[++this.current % this.theList.Count];

                if (oldCanBack != this.CanBack) { this.CanBackChange?.Invoke(this, this.CanBack); }
                if (!this.CanForword) { this.CanForwordChange?.Invoke(this, false); }

                return true;
            }
            else
            {
                obj = default(T);
                return false;
            }
        }

        /// <summary>
        /// Reset all pointers to no valid elements state
        /// </summary>
        public void Clear()
        {
            this.start = 0;
            this.end = 0;
            this.current = InitCurrent;
        }
    }
}
