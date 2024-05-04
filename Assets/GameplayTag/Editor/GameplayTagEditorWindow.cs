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
            if(_serializedObject.isEditingMultipleObjects) return;
            
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
            // method 1: （无法检查到子节点的变化）
            // _tagContainer.TrackPropertyValue(_serializedObject.FindProperty(RootPropPath), RefreshTagContainerView);
            // method 2: （检查子节点变化有效，但可能会检测到无关数据的变化。可用事件来替代 Track触发界面更新）
            _tagContainer.TrackSerializedObjectValue(_serializedObject, RefreshTagContainerView);

            RefreshTagContainerView(_serializedObject);
        }
        
        private void OnAddTagClicked()
        {
            Debug.Log($"Add gameplay tag: {_inputTag.value}");
            AddTag(_inputTag.value);
        }
        
        private void RefreshTagContainerView(SerializedObject serializedObject)
        {
            // 遍历
            static void Traverse(SerializedProperty nodeProperty, Action<SerializedProperty> visitor)
            {
                if (nodeProperty == null) return;
                
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
                // 展示标签
                var tagDepth = obj.FindPropertyRelative("depth").intValue;
                var tagName = obj.FindPropertyRelative("name").stringValue;
                if (tagDepth > 0)
                {
                    tagName = tagName.Split('.')[tagDepth];
                    var prefix = "";
                    for (int i = 0; i < tagDepth; i++)
                        prefix += "┗━━";      // 制表符
                    tagName = prefix + tagName;
                }
                element.Q<Label>().text = tagName;

                void ClickDelete()
                {
                    Debug.Log($"Delete gameplay tag: {tagDepth}-{tagName}");
                    var hash = new GameplayTagHash()
                    {
                        hash0 = GetTagHashAtDepth(obj, 0),
                        hash1 = GetTagHashAtDepth(obj, 1),
                        hash2 = GetTagHashAtDepth(obj, 2),
                        hash3 = GetTagHashAtDepth(obj, 3),
                    };
                    RemoveTag(hash);
                }
                element.Q<Button>().clickable.clicked += ClickDelete;
                _tagContainer.Add(element);
            }
            
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

        private void RemoveTag(GameplayTagHash tagHash)
        {
            var length = tagHash.Length;
            
            SerializedProperty currentProperty = _serializedObject.FindProperty(RootPropPath);
            int depth = 0;
            bool hasDesiredNodeAtDepth;
            do
            {
                hasDesiredNodeAtDepth = false;
                var subNodesPropArray = currentProperty.FindPropertyRelative(SubNodesPropPath);
                if(!subNodesPropArray.isArray || subNodesPropArray.arraySize < 1) break;
                for (int i = 0; i < subNodesPropArray.arraySize; i++)
                {
                    var nodeProp = subNodesPropArray.GetArrayElementAtIndex(i);
                    if (GetTagHashAtDepth(nodeProp, depth) != tagHash[depth]) continue;
                    
                    // 移除
                    if (depth + 1 == length)
                    {
                        subNodesPropArray.DeleteArrayElementAtIndex(i);
                        subNodesPropArray.serializedObject.ApplyModifiedProperties();
                        return;
                    }
                    currentProperty = nodeProp;
                    hasDesiredNodeAtDepth = true;
                    break;
                }
                depth++;
            } while (depth < length && hasDesiredNodeAtDepth);
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
