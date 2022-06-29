using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rair.Items {
    public class Item
    {
        public string name, description;
        public Sprite sprite;
        public List<Tag> tags = new();

        public Item(string name, string description, params Tag[] tags) {
            this.name = name;
            this.description = description;

            //* Properties
            this.tags = new List<Tag>(tags);
        }

        /// <summary>아이템을 복제할 때만 사용합니다.</summary>
        private Item(Item original) {
            name = original.name;
            description = original.description;

            tags = new(original.tags);
        }
        public Item Clone() => new(this);
    }
    
    public class Tag {
        public string name, description;
        public int Level { get; private set; }
        public readonly int maxLevel;
        private Tag(string name, int level, int maxLevel = 1, string description = "") {
            this.name = name;
            Level = level;
            this.maxLevel = maxLevel;
            this.description = description;
        }

        public static Tag operator +(Tag t1, Tag t2) {
            if (t1 != t2) throw new Exception("서로 다른 tag를 더할 수 없습니다.");
            t1.Level = Mathf.Clamp(t1.Level + t2.Level, 0, t1.maxLevel);
            return t1;
        }

        #region Misc
        public override string ToString() =>  $"{name} Lv.{Level}";
        public override bool Equals(object obj) => obj is Tag p && p.name == name; // 이름만 비교합니다.
        public static bool operator ==(Tag A, Tag B) => A.Equals(B);
        public static bool operator !=(Tag A, Tag B) => !A.Equals(B);
        public override int GetHashCode() => name.GetHashCode();
        #endregion

        #region Sample Tags
        public static Tag HPIncrease(int level) => new("HP Increase", level, 10, "레벨당 HP 500 증가");
        public static Tag HPMultiply(int level) => new("HP Boost", level, 10, "레벨당 HP 5% 증가");
        #endregion
    }
}