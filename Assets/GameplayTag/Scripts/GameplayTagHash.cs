using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EGF
{
    // 用于前缀树查询用的多层 hash，最多 4层，再多没必要
    [Serializable]
    public struct GameplayTagHash
    {
        public int hash0;
        public int hash1;
        public int hash2;
        public int hash3;
        
        public const int InValid = 0;
        public bool IsValid => hash0 == 0;
        
        public int Length
        {
            get
            {
                for (int i = 0; i < 4; i++)
                {
                    if (this[i] == InValid)
                        return i;
                }
                return 4;
            }
        }
        
        // 索引器
        public int this[int index]
        {
            get
            {
                return index switch
                {
                    0 => hash0,
                    1 => hash1,
                    2 => hash2,
                    3 => hash3,
                    _ => 0
                };
            }
            set
            {
                switch (index)
                {
                    case 0: hash0 = value; break;
                    case 1: hash1 = value; break;
                    case 2: hash2 = value; break;
                    case 3: hash3 = value; break;
                    // default: throw new ArgumentOutOfRangeException("index", "Index out of range");
                    default: break;
                }
            }
        }
    }
}
