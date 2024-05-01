using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EGF.Editor
{
    public class GameplayTagEditorWindow : EditorWindow
    {
        [MenuItem("Tools/GameplayTagsEditor")]
        public static void OpenWindow() {
            GameplayTagEditorWindow wnd = GetWindow<GameplayTagEditorWindow>();
            wnd.titleContent = new GUIContent("Gameplay Tag Editor");
            wnd.minSize = new Vector2(250, 400);
        }
        
        private void CreateGUI()
        {
            var data = GameplayTagEditorUtils.GameplayTagData;
        }
    }
}
