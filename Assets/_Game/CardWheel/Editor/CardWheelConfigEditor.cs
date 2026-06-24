using UnityEditor;
using UnityEngine;

namespace Vertigo.CardWheel.Editor
{
    public class CardWheelConfigEditor : EditorWindow
    {
        private enum Tab
        {
            WheelTiers,
            ZoneMapping,
            Slices
        }

        private Tab     _currentTab = Tab.WheelTiers;
        private Vector2 _scrollPosition;

        private WheelTierConfigEditorTab  _wheelTierTab;
        private ZoneWheelMappingEditorTab _zoneMappingTab;
        private WheelSliceDefinitionEditorTab _wheelSlicesTab;

        [MenuItem("Vertigo/CardWheel/Config Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<CardWheelConfigEditor>("CardWheel Config Editor");
            window.minSize = new Vector2(500, 400);
        }

        private void OnEnable()
        {
            _wheelTierTab   = new WheelTierConfigEditorTab();
            _zoneMappingTab = new ZoneWheelMappingEditorTab();
            _wheelSlicesTab     = new WheelSliceDefinitionEditorTab();
        }

        private void OnDisable()
        {
            _wheelTierTab.Dispose();
            _zoneMappingTab.Dispose();
            _wheelSlicesTab.Dispose();
        }

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.Space(4);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            switch (_currentTab)
            {
                case Tab.WheelTiers:
                    _wheelTierTab.OnGUI(this);
                    break;
                case Tab.ZoneMapping:
                    _zoneMappingTab.OnGUI(this);
                    break;
                case Tab.Slices:
                    _wheelSlicesTab.OnGUI(this);
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            var tabNames = System.Enum.GetNames(typeof(Tab));
            var tabIndex = (int)_currentTab;

            var newTabIndex = GUILayout.Toolbar(tabIndex, tabNames, GUILayout.Height(30));
            if (newTabIndex != tabIndex)
            {
                if (TrySwitchTab((Tab)newTabIndex))
                {
                    _currentTab = (Tab)newTabIndex;
                }
            }
        }

        private bool TrySwitchTab(Tab newTab)
        {
            IEditorTab current = GetCurrentTab();
            if (current != null && current.IsDirty)
            {
                var choice = EditorUtility.DisplayDialogComplex
                    (
                     "Unsaved Changes",
                     $"You have unsaved changes in the {_currentTab} tab. What would you like to do?",
                     "Apply & Switch",
                     "Cancel",
                     "Discard & Switch"
                    );

                switch (choice)
                {
                    case 0:
                        current.Apply();
                        return true;
                    case 1:
                        return false;
                    case 2:
                        current.Revert();
                        return true;
                }
            }

            return true;
        }

        private IEditorTab GetCurrentTab()
        {
            return _currentTab switch
            {
                Tab.WheelTiers  => _wheelTierTab,
                Tab.ZoneMapping => _zoneMappingTab,
                Tab.Slices     => _wheelSlicesTab,
                _               => null
            };
        }
    }
}