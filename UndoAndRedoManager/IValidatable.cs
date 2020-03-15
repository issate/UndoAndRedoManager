

namespace UndoAndRedoManager
{
    public interface IValidatable<T>
    {
        bool IsValid();
    }
}
