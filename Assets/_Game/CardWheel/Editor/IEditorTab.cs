using UnityEditor;

namespace Vertigo.CardWheel.Editor
{
    public interface IEditorTab
    {
        bool IsDirty { get; }
        void OnGUI(EditorWindow hostWindow);
        void Apply();
        void Revert();
    }
}