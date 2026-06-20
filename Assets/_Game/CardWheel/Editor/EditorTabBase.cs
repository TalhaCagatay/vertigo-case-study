using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Vertigo.CardWheel.Data.Rewards;

namespace Vertigo.CardWheel.Editor
{
    public abstract class EditorTabBase<T> : IEditorTab where T : ScriptableObject
    {
        protected List<T>         _assetList     = new List<T>();
        protected string[]        _assetNames    = System.Array.Empty<string>();
        protected int             _selectedIndex = -1;
        protected ConfigBuffer<T> _buffer;
        protected Vector2         _selectionScrollPos = Vector2.zero;
        protected Vector2         _editorScrollPos    = Vector2.zero;

        protected abstract string AssetTypeLabel       { get; }
        protected abstract string CreateDialogTitle    { get; }
        protected abstract string CreateDialogPathRoot { get; }
        protected abstract string CreateDialogFileName { get; }

        public bool IsDirty => _buffer != null && _buffer.IsDirty;

        public void OnGUI(EditorWindow hostWindow)
        {
            RefreshAssetList();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(220));
            DrawAssetListPanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginVertical();
            DrawEditorPanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void RefreshAssetList()
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            _assetList = guids.Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(a => a.name).ToList();

            _assetNames = _assetList.Select(a => a.name).ToArray();
        }

        protected virtual void DrawAssetListPanel()
        {
            EditorGUILayout.LabelField($"{AssetTypeLabel}s", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("New", GUILayout.Width(50)))
            {
                if (HandleUnsavedBeforeSwitch())
                {
                    CreateNewAsset();
                }
            }

            if (GUILayout.Button("Duplicate", GUILayout.Width(65)))
            {
                if (HandleUnsavedBeforeSwitch())
                {
                    DuplicateSelectedAsset();
                }
            }

            if (GUILayout.Button("Delete", GUILayout.Width(55)))
            {
                DeleteSelectedAsset();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            _selectionScrollPos = EditorGUILayout.BeginScrollView(_selectionScrollPos);
            var newIndex = GUILayout.SelectionGrid(_selectedIndex, _assetNames, 1);
            if (newIndex != _selectedIndex)
            {
                if (HandleUnsavedBeforeSwitch())
                {
                    SelectAsset(newIndex);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        protected virtual bool HandleUnsavedBeforeSwitch()
        {
            if (_buffer != null && _buffer.IsDirty)
            {
                var choice = EditorUtility.DisplayDialogComplex
                    (
                     "Unsaved Changes",
                     "You have unsaved changes. What would you like to do?",
                     "Apply",
                     "Cancel",
                     "Discard"
                    );

                switch (choice)
                {
                    case 0:
                        Apply();
                        return true;
                    case 1:
                        return false;
                    case 2:
                        Revert();
                        return true;
                }
            }

            return true;
        }

        protected virtual void SelectAsset(int index)
        {
            _editingName = null;
            _nameChanged = false;

            if (index < 0 || index >= _assetList.Count)
            {
                _buffer?.Dispose();
                _buffer        = null;
                _selectedIndex = -1;
                return;
            }

            _buffer?.Dispose();
            _selectedIndex = index;
            var asset = _assetList[index];
            var path  = AssetDatabase.GetAssetPath(asset);
            _buffer = new ConfigBuffer<T>(asset, path);
        }

        protected virtual void CreateNewAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject
                (
                 CreateDialogTitle,
                 CreateDialogFileName,
                 "asset",
                 $"Choose a location for the new {AssetTypeLabel}",
                 CreateDialogPathRoot
                );

            if (string.IsNullOrEmpty(path)) return;

            var template = ScriptableObject.CreateInstance<T>();
            template.name = System.IO.Path.GetFileNameWithoutExtension(path);

            _buffer?.Dispose();
            _buffer = new ConfigBuffer<T>(template);
            _buffer.SetAssetPath(path);
            _selectedIndex = -1;
        }

        protected virtual void DuplicateSelectedAsset()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _assetList.Count) return;

            var source     = _assetList[_selectedIndex];
            var sourcePath = AssetDatabase.GetAssetPath(source);

            var directory = System.IO.Path.GetDirectoryName(sourcePath);
            var baseName  = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
            var extension = System.IO.Path.GetExtension(sourcePath);

            var destPath = System.IO.Path.Combine(directory, $"{baseName}_Copy{extension}");
            var counter  = 1;
            while (System.IO.File.Exists(destPath))
            {
                destPath = System.IO.Path.Combine(directory, $"{baseName}_Copy_{counter}{extension}");
                counter++;
            }

            if (!AssetDatabase.CopyAsset(sourcePath, destPath))
            {
                Debug.LogError($"Failed to duplicate asset: {sourcePath} -> {destPath}");
                return;
            }

            AssetDatabase.Refresh();
            RefreshAssetList();

            var newAsset = AssetDatabase.LoadAssetAtPath<T>(destPath);
            var newIndex = _assetList.IndexOf(newAsset);
            if (newIndex >= 0)
            {
                SelectAsset(newIndex);
            }
        }

        protected virtual void DeleteSelectedAsset()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _assetList.Count) return;

            var asset = _assetList[_selectedIndex];
            var path  = AssetDatabase.GetAssetPath(asset);
            var confirmed = EditorUtility.DisplayDialog
                (
                 "Delete Asset",
                 $"Are you sure you want to delete '{asset.name}'?\n\nThis cannot be undone.",
                 "Delete",
                 "Cancel"
                );

            if (!confirmed) return;

            _buffer?.Dispose();
            _buffer = null;

            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
            _selectedIndex = -1;
            RefreshAssetList();
        }

        private string _editingName;
        private bool   _nameChanged;

        protected virtual void DrawEditorPanel()
        {
            if (_buffer == null)
            {
                EditorGUILayout.HelpBox($"Select or create a {AssetTypeLabel} to edit.", MessageType.Info);
                return;
            }

            // Sync the editable name once when a buffer is freshly selected
            if (_editingName == null || _editingName != _buffer.Working.name)
            {
                _editingName = _buffer.Working.name;
                _nameChanged = false;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(42));
            EditorGUI.BeginChangeCheck();
            _editingName = EditorGUILayout.TextField(_editingName);
            if (EditorGUI.EndChangeCheck())
            {
                _buffer.Working.name = _editingName;
                _nameChanged         = true;
                _buffer.MarkDirty();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();

            _editorScrollPos = EditorGUILayout.BeginScrollView(_editorScrollPos);
            DrawAssetFields();
            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                _buffer.MarkDirty();
            }

            EditorGUILayout.Space(8);

            DrawApplyRevertButtons();
        }

        protected virtual void DrawAssetFields()
        {
            var so = _buffer.SerializedWorking;
            so.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Asset", _buffer.Original, typeof(ARewardDefinition), false);
            EditorGUI.EndDisabledGroup();

            var prop = so.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    var isScriptField = prop.name == "m_Script";
                    if (isScriptField) EditorGUI.BeginDisabledGroup(true);

                    EditorGUILayout.PropertyField(prop, true);

                    if (isScriptField) EditorGUI.EndDisabledGroup();
                }
                while (prop.NextVisible(false));
            }

            so.ApplyModifiedProperties();
        }

        protected virtual void DrawApplyRevertButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = _buffer.IsDirty;
            if (GUILayout.Button("Apply", GUILayout.Height(30)))
            {
                Apply();
            }

            GUI.enabled = _buffer.IsDirty;
            if (GUILayout.Button("Revert", GUILayout.Height(30)))
            {
                Revert();
            }

            GUI.enabled = true;

            if (_buffer.IsDirty)
            {
                var style = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.yellow } };
                EditorGUILayout.LabelField("● Unsaved changes", style, GUILayout.Width(120));
            }

            EditorGUILayout.EndHorizontal();
        }

        public virtual void Apply()
        {
            if (_buffer == null) return;

            var needsRename = _nameChanged && _buffer.Original != null;
            var newName     = _editingName;

            _buffer.Apply();

            if (needsRename && _buffer.Original != null)
            {
                var currentPath = AssetDatabase.GetAssetPath(_buffer.Original);
                var result      = AssetDatabase.RenameAsset(currentPath, newName);
                if (!string.IsNullOrEmpty(result))
                {
                    Debug.LogError($"Failed to rename '{currentPath}' to '{newName}': {result}");
                }
                else
                {
                    _buffer.Working.name = newName;
                }
            }

            _nameChanged = false;

            RefreshAssetList();

            if (_buffer?.Original != null)
            {
                var path  = AssetDatabase.GetAssetPath(_buffer.Original);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                _selectedIndex = _assetList.IndexOf(asset);
                _editingName   = asset != null ? asset.name : _editingName;
            }
        }

        public virtual void Revert()
        {
            _buffer?.Revert();
            if (_buffer?.Working != null)
            {
                _editingName = _buffer.Working.name;
            }

            _nameChanged = false;
        }

        public virtual void Dispose()
        {
            _buffer?.Dispose();
            _buffer = null;
        }
    }
}