using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EGF
{
    [Serializable]
    public partial class GameplayTagContainer
    {
        // [SerializeField] private GameplayTagData tagData;    // 可直接用 GameplayTagUtils.GameplayTagData
        // 序列化数据
        [SerializeField][SerializeReference] private GTagRuntimeTrieNode rootNode = new GTagRuntimeTrieNode();
        
        /// 检查节点是否失效需要自动移除
        private bool NeedAutoRemove(GTagRuntimeTrieNode tagNode)
        {
            return !tagNode.active && (tagNode.subNodes == null || tagNode.subNodes.Count == 0);
        }
        
        // HACK：待验证
        public void AddTagRuntime(GameplayTagHash tagHash)
        {
            if(!tagHash.IsValid) return;
            
            var length = tagHash.Length;
            GTagRuntimeTrieNode current = rootNode;
            for (var depth = 0; depth < length; depth++)
            {
                var hasDesiredNodeAtDepth = false;
                // 当前深度下是否已存在所需节点
                foreach (var node in current.subNodes)
                {
                    if (node.hash[depth] != tagHash[depth]) continue;
                    
                    hasDesiredNodeAtDepth = true;
                    current = node;
                    break;
                }

                if (hasDesiredNodeAtDepth)
                {
                    if (depth == length - 1)
                        current.active = true;
                    continue;
                }
                
                // 创建当前深度所需的节点
                var newNode = GTagRuntimeTrieNode.CreateFromTag(tagHash, depth);
                current.subNodes.Add(newNode);
                current = newNode;
            }
            
        }
        
        // HACK：待验证
        public void RemoveTagRuntime(GameplayTagHash tagHash)
        {
            if(!tagHash.IsValid) return;
            
            var length = tagHash.Length;
            int depth = 0;
            GTagRuntimeTrieNode[] cache = new GTagRuntimeTrieNode[length];
            GTagRuntimeTrieNode current = rootNode;
            bool hasDesiredNodeAtDepth;
            do
            {
                hasDesiredNodeAtDepth = false;
                if(current.subNodes == null || current.subNodes.Count == 0) break;
                foreach (var node in current.subNodes)
                {
                    if (node.hash[depth] != tagHash[depth]) continue;
                    
                    current = node;
                    hasDesiredNodeAtDepth = true;
                    break;
                }
                
                if (!hasDesiredNodeAtDepth) continue;
                cache[depth] = current;
                depth++;

            } while (depth < length && hasDesiredNodeAtDepth);

            // 找出目标才移除
            if (!hasDesiredNodeAtDepth) return;
            // 目标标记移除
            cache[depth].active = false;
            // 自动清理目标父级未标记节点
            while (depth > 0)
            {
                var checking = cache[depth];
                if (!NeedAutoRemove(checking)) break;
                    
                cache[depth - 1].subNodes.Remove(checking);
                depth--;
            }
            // 检查和移除最初节点
            if (NeedAutoRemove(cache[0]))
                rootNode.subNodes.Remove(cache[0]);
        }

        public void ClearTagRuntime()
        {
            rootNode.subNodes.Clear();
        }
        
        public bool IsEmpty()
        {
            return rootNode.subNodes.Count == 0;
        }

        /// 单个标签包含检查
        public bool Contains(GameplayTagHash tagHash)
        {
            var length = tagHash.Length;

            GTagRuntimeTrieNode current = rootNode;
            int depth = 0;
            bool hasDesiredNodeAtDepth;
            do
            {
                hasDesiredNodeAtDepth = false;
                if(current.subNodes == null || current.subNodes.Count == 0) break;
                foreach (var node in current.subNodes)
                {
                    if (node.hash[depth] != tagHash[depth]) continue;
                    
                    current = node;
                    hasDesiredNodeAtDepth = true;
                    break;
                }
                depth++;
            } while (depth < length && hasDesiredNodeAtDepth);
            
            return hasDesiredNodeAtDepth;
        }

        public bool Contains(IEnumerable<GameplayTagHash> tags)
        {
            var result = false;
            foreach (var tagHash in tags)
            {
                result = Contains(tagHash);
                if(!result)
                    break;
            }

            return result;
        }
        
        public bool Contains(GameplayTagContainer selector)
        {
            var stopTraverse = false;
            var result = false;

            void Visitor(GTagRuntimeTrieNode node)
            {
                if(!node.active) return;
                result = this.Contains(node.hash);
                if (!result)
                    stopTraverse = true;
            }
            // 注意 rootNode 本身不能参与
            foreach (var check in selector.rootNode.subNodes)
                GTagRuntimeTrieNode.TraverseTree(check, Visitor, (node) => stopTraverse);
            return result;
        }

        public bool ContainsAny(IEnumerable<GameplayTagHash> tags)
        {
            var result = false;
            foreach (var tagHash in tags)
            {
                result = Contains(tagHash);
                if(result)
                    break;
            }

            return result;
        }

        public bool ContainsAny(GameplayTagContainer selector)
        {
            var stopTraverse = false;
            var result = false;

            void Visitor(GTagRuntimeTrieNode node)
            {
                result = Contains(node.hash);
                if (result)
                    stopTraverse = true;
            }
            
            // 注意 rootNode 本身不能参与
            foreach (var check in selector.rootNode.subNodes)
                GTagRuntimeTrieNode.TraverseTree(check, Visitor, (node) => stopTraverse);
            return result;
        }
    }
}
