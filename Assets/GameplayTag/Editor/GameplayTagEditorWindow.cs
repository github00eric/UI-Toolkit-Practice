using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EGF.Editor
{
    public class GameplayTagEditorWindow : EditorWindow
    {
        private VisualElement _tagContainer;
        private ToolbarButton _toolbarButton;
        private TextField _inputTag;

        private SerializedObject _serializedObject;
        private GameplayTagData _data;

        private const string RootPropPath = "rootNode";
        private const string SubNodesPropPath = "subNodes";
        
        [MenuItem("Tools/GameplayTagsEditor")]
        public static void OpenWindow() {
            GameplayTagEditorWindow wnd = GetWindow<GameplayTagEditorWindow>();
            wnd.titleContent = new GUIContent("Gameplay Tag Editor");
            wnd.minSize = new Vector2(250, 400);
        }
        
        private void CreateGUI()
        {
            var data = GameplayTagEditorUtils.GameplayTagData;
            var editorSetting = GameplayTagEditorSetting.GetOrCreateSettings();
            if (!editorSetting.CheckSettingReady()) return;

            _serializedObject = new SerializedObject(data);
            _data = data;
            // editor window root
            VisualElement root = rootVisualElement;
            editorSetting.tagEditorXml.CloneTree(root);
            _toolbarButton = root.Q<ToolbarButton>();
            _inputTag = root.Q<TextField>();
            _toolbarButton.clickable.clicked += OnAddTagClicked;
            _inputTag.value = "new tag here";
            // tag Container
            _tagContainer = new VisualElement();
            root.Add(_tagContainer);

            _tagContainer.Unbind();
            _tagContainer.TrackPropertyValue(_serializedObject.FindProperty(RootPropPath), RefreshTagView);

            RefreshTagView(_serializedObject.FindProperty(RootPropPath));
        }
        
        private void OnAddTagClicked()
        {
            Debug.Log($"Tag Changed. {_inputTag.value}");
            AddTag(_inputTag.value);
        }
        
        private void RefreshTagView(SerializedProperty serializedProperty)
        {
            // 遍历
            static void Traverse(SerializedProperty nodeProperty, Action<SerializedProperty> visitor)
            {
                if (nodeProperty == null) return;
                
                Debug.Log($"Invoke {nodeProperty.propertyPath}");
                visitor.Invoke(nodeProperty);
                var subNodes = nodeProperty.FindPropertyRelative(SubNodesPropPath);
                if (!subNodes.isArray) return;
                for (var i = 0; i < subNodes.arraySize; i++)
                {
                    var nodeProp = subNodes.GetArrayElementAtIndex(i);
                    Traverse(nodeProp, visitor);
                }
            }
            // 遍历操作
            void Visitor(SerializedProperty obj)
            {
                VisualElement element = new VisualElement();
                GameplayTagEditorSetting.GetOrCreateSettings().tagDataElementXml.CloneTree(element);
                element.Q<Label>().text = obj.FindPropertyRelative("name").stringValue;
                _tagContainer.Add(element);
            }
            
            Debug.Log($"Refresh Tag View: {serializedProperty.propertyPath}");
            _tagContainer.Clear();
            // 注意 rootNode 本身不能参与
            var subNodes = _serializedObject.FindProperty($"{RootPropPath}.{SubNodesPropPath}");
            if (!subNodes.isArray) return;
            for (var i = 0; i < subNodes.arraySize; i++)
            {
                var nodeProp = subNodes.GetArrayElementAtIndex(i);
                Traverse(nodeProp, Visitor);
            }
        }
        
        private void AddTag(string newTag)
        {
            var tagHash = GameplayTagUtils.GetTagHashFromString(newTag);
            var length = tagHash.Length;

            SerializedProperty currentProperty = _serializedObject.FindProperty(RootPropPath);
            for (var depth = 0; depth < length; depth++)
            {
                var hasDesiredNodeAtDepth = false;
                var subNodes = currentProperty.FindPropertyRelative(SubNodesPropPath);
                // 当前深度下是否已存在所需节点
                for (int i = 0; i < subNodes.arraySize; i++)
                {
                    var nodeProp = subNodes.GetArrayElementAtIndex(i);
                    if(nodeProp.managedReferenceValue != null && GetTagHashAtDepth(nodeProp,depth) != tagHash[depth]) continue;
                    
                    hasDesiredNodeAtDepth = true;
                    currentProperty = nodeProp;
                    break;
                }
                
                if (hasDesiredNodeAtDepth) continue;
                {
                    // 创建当前深度所需的节点
                    var node = GTagTrieNode.CreateFromTag(tagHash, newTag, depth);
                    subNodes = currentProperty.FindPropertyRelative(SubNodesPropPath);

                    var adding = AppendArrayElement(subNodes);
                    adding.managedReferenceValue = node;
                    _serializedObject.ApplyModifiedProperties();
                    currentProperty = adding;
                }
            }

            _serializedObject.ApplyModifiedProperties();
        }
        
        SerializedProperty AppendArrayElement(SerializedProperty arrayProperty) {
            arrayProperty.InsertArrayElementAtIndex(arrayProperty.arraySize);
            return arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1);
        }
        
        int GetTagHashAtDepth(SerializedProperty property, int depth)
        {
            const string hashProp0 = "hash0";
            const string hashProp1 = "hash1";
            const string hashProp2 = "hash2";
            const string hashProp3 = "hash3";

            var hashProp = property.FindPropertyRelative("hash");
            
            int result = depth switch
            {
                0 => hashProp.FindPropertyRelative(hashProp0).intValue,
                1 => hashProp.FindPropertyRelative(hashProp1).intValue,
                2 => hashProp.FindPropertyRelative(hashProp2).intValue,
                3 => hashProp.FindPropertyRelative(hashProp3).intValue,
                _ => 0
            };

            return result;
        }
    }
}
