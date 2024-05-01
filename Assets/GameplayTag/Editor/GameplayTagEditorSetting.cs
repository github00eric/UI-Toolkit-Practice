// using System.Collections;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
//
// namespace EGF.Editor
// {
//     public class GameplayTagEditorSetting : ScriptableObject
//     {
//         #region Editor Setting
//         
//         static GameplayTagEditorSetting FindSettings(){
//             var guids = AssetDatabase.FindAssets($"t:{nameof(GameplayTagEditorSetting)}");
//             if (guids.Length > 1) {
//                 Debug.LogWarning($"Found multiple settings files, using the first.");
//             }
//
//             switch (guids.Length) {
//                 case 0:
//                     return null;
//                 default:
//                     var path = AssetDatabase.GUIDToAssetPath(guids[0]);
//                     return AssetDatabase.LoadAssetAtPath<GameplayTagEditorSetting>(path);
//             }
//         }
//
//         internal static GameplayTagEditorSetting GetOrCreateSettings() {
//             var settings = FindSettings();
//             if (settings == null) {
//                 settings = ScriptableObject.CreateInstance<GameplayTagEditorSetting>();
//                 AssetDatabase.CreateAsset(settings, "Assets");
//                 AssetDatabase.SaveAssets();
//             }
//             return settings;
//         }
//
//         internal static SerializedObject GetSerializedSettings() {
//             return new SerializedObject(GetOrCreateSettings());
//         }
//         
//         #endregion
//     }
// }
