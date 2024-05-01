using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EGF
{
    public class TempTestGameplayTag : MonoBehaviour
    {
        public GameplayTagContainer tagContainer1;
        public GameplayTagContainer tagContainer2;

        public string nameAdd;
        public string nameContain;

        private GameplayTagHash hashAdd;
        private GameplayTagHash hashContain;

        [ContextMenu("Test Add")]
        public void Test1()
        {
            hashAdd = GameplayTagUtils.GetTagHashFromString(nameAdd);
            tagContainer1.AddTagRuntime(hashAdd);
            
            hashContain = GameplayTagUtils.GetTagHashFromString(nameContain);
            tagContainer2.AddTagRuntime(hashContain);
        }

        [ContextMenu("Test 1 contains 2")]
        public void Test2()
        {
            Debug.Log("Contain Hash: " + tagContainer1.Contains(hashContain));
            Debug.Log("Contain container: " + tagContainer1.Contains(tagContainer2));
            Debug.Log("Contain containerAny: " + tagContainer1.ContainsAny(tagContainer2));
        }

        [ContextMenu("Test Clear")]
        public void Test3()
        {
            tagContainer1.ClearTagRuntime();
            tagContainer2.ClearTagRuntime();
        }
    }
}
