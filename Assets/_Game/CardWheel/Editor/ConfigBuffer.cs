using UnityEditor;
using UnityEngine;

namespace Vertigo.CardWheel.Editor
{
    public class ConfigBuffer<T> where T : ScriptableObject
    {
        public T Original { get; private set; }

        public T Working { get; private set; }

        public SerializedObject SerializedWorking { get; private set; }

        public SerializedObject SerializedOriginal { get; private set; }

        public string AssetPath { get; private set; }

        public bool IsDirty { get; private set; }

        public ConfigBuffer(T original, string assetPath)
        {
            Original           = original;
            AssetPath          = assetPath;
            Working            = Object.Instantiate(original);
            Working.name       = original.name;
            Working.hideFlags  = HideFlags.DontSaveInEditor;
            SerializedWorking  = new SerializedObject(Working);
            SerializedOriginal = new SerializedObject(Original);
            IsDirty            = false;
        }

        public ConfigBuffer(T template)
        {
            Original           = null;
            AssetPath          = null;
            Working            = Object.Instantiate(template);
            Working.name       = template.name;
            Working.hideFlags  = HideFlags.DontSaveInEditor;
            SerializedWorking  = new SerializedObject(Working);
            SerializedOriginal = new SerializedObject(Working);
            IsDirty            = true;
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        public void Apply()
        {
            SerializedWorking.ApplyModifiedProperties();
            SerializedOriginal?.ApplyModifiedProperties();

            if (Original != null)
            {
                EditorUtility.CopySerialized(Working, Original);
                EditorUtility.SetDirty(Original);
                AssetDatabase.SaveAssetIfDirty(Original);
            }
            else
            {
                Original = ScriptableObject.CreateInstance<T>();
                EditorUtility.CopySerialized(Working, Original);
                Original.name = Working.name;
                AssetDatabase.CreateAsset(Original, AssetPath);
                EditorUtility.SetDirty(Original);
                AssetDatabase.SaveAssetIfDirty(Original);
                SerializedOriginal = new SerializedObject(Original);
            }

            IsDirty = false;
            AssetDatabase.Refresh();
        }

        public void Revert()
        {
            if (Original != null)
            {
                Object.DestroyImmediate(Working);
                Working           = Object.Instantiate(Original);
                Working.name      = Original.name;
                Working.hideFlags = HideFlags.DontSaveInEditor;
                SerializedWorking = new SerializedObject(Working);
                IsDirty           = false;
            }
        }

        public void RefreshFromDisk()
        {
            if (Original != null && !string.IsNullOrEmpty(AssetPath))
            {
                Original           = AssetDatabase.LoadAssetAtPath<T>(AssetPath);
                SerializedOriginal = new SerializedObject(Original);
            }
        }

        public void SetAssetPath(string path)
        {
            AssetPath = path;
        }

        public void Dispose()
        {
            if (Working != null)
            {
                Object.DestroyImmediate(Working);
                Working = null;
            }

            SerializedWorking?.Dispose();
            SerializedWorking = null;
            SerializedOriginal?.Dispose();
            SerializedOriginal = null;
            Original           = null;
        }
    }
}