using System.IO;
using UnityEditor;
using UnityEngine;

namespace EGF.Editor
{
    public static class GameplayTagEditorUtils
    {
        public static GameplayTagData GameplayTagData
        {
            get
            {
                if (!GameplayTagUtils.GameplayTagData)
                    Init();
                return GameplayTagUtils.GameplayTagData;
            }
        }
        
        // 载入失败则创建
        static void Init()
        {
            if (GameplayTagUtils.GameplayTagData) return;
            
            if (!Directory.Exists(GameplayTagUtils.DataPath))
                Directory.CreateDirectory(GameplayTagUtils.DataPath);

            var newData = ScriptableObject.CreateInstance<GameplayTagData>();
            AssetDatabase.CreateAsset(newData, $"{GameplayTagUtils.DataPath}/{GameplayTagUtils.FileName}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            GameplayTagUtils.SetGameplayTagData(newData);
            GameplayTagUtils.GameplayTagData.Init();
        }
        
        // TODO：撤销和重做操作......
    }
}
