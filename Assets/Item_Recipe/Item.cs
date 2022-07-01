using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rair.Items {
    public partial class Item
    {
        public string name, description, coreName;
        public Sprite sprite;
        public List<Tag> tags = new();

        /// <summary>아이템이 주어진 태그(및 태그 레벨)을 가지고 있는지 검사합니다.</summary>
        public bool Has(Tag tag) {
            var t = tags.FirstOrDefault(i => i == tag);
            return (t is not null) && (t.Level >= tag.Level);
        }
        /// <summary>아이템에 해당 태그를 합산합니다.</summary>
        public void AddTag(Tag tag) {
            var t = tags.FirstOrDefault(i => i == tag);
            if (t is not null) t += tag;
            else tags.Add(tag);
        }
        /// <summary>아이템에 해당 태그를 합산합니다.</summary>
        public void AddTag(params Tag[] tag) {
            foreach (var t in tag) AddTag(t);
        }

        public Item(string coreName, string description, Type type, string prefix = "", string suffix = "", params Tag[] tags) {
            this.name = prefix + coreName + suffix;
            this.description = description;
            this.coreName = coreName;
            this.type = type;
            //* Properties
            this.tags = new List<Tag>(tags);
        }

        public Item Clone() {
            var clone = (Item)MemberwiseClone();
            clone.tags = tags.Select(t => t.Clone()).ToList();
            return clone;
        }

        #region Misc
        public override bool Equals(object obj) => obj is Item item && type == item.type; // 타입 검사만 수행
        public override int GetHashCode() => HashCode.Combine(name, type);
        public static bool operator ==(Item left, Item right) => left.Equals(right);
        public static bool operator !=(Item left, Item right) => !(left == right);
        #endregion
    }
    
    public partial class Tag {
        public string name, description;
        public int Level { get; private set; }
        public readonly int maxLevel;
        private Tag(string name, int level, int maxLevel = 1, string description = "") {
            this.name = name;
            this.maxLevel = maxLevel;
            if (level == 0) Level = UnityEngine.Random.Range(1, maxLevel + 1);
            else Level = Mathf.Clamp(level, 1, maxLevel);
            this.description = description;
        }

        public static Tag[] Merge(params IEnumerable<Tag>[] tags) {
            List<Tag> result = new();
            foreach (var ts in tags) foreach (var t in ts) {
                if (result.Contains(t)) result.First(i => i == t).Level += t.Level;
                else result.Add(t);
            }

            return result.ToArray();
        }

        #region Misc
        public override string ToString() =>  $"{name} Lv.{Level}";
        public override bool Equals(object obj) => obj is Tag p && p.name == name; // 이름만 비교합니다.
        public static bool operator ==(Tag A, Tag B) => A.Equals(B);
        public static bool operator !=(Tag A, Tag B) => !A.Equals(B);
        public static Tag operator +(Tag t1, Tag t2) {
            if (t1 != t2) throw new Exception("서로 다른 tag를 더할 수 없습니다.");
            t1.Level = Mathf.Clamp(t1.Level + t2.Level, 0, t1.maxLevel);
            return t1;
        }
        public override int GetHashCode() => name.GetHashCode();
        public Tag Clone() => (Tag)MemberwiseClone();
        #endregion
    }
}

public static class EnumerableExtension {
    public static T Random<T>(this IEnumerable<T> source) => source.ElementAt(UnityEngine.Random.Range(0, source.Count()));
}