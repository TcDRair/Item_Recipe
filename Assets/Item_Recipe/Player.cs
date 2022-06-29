using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using Rair.Samples;
using Rair.Items;
using Rair.Skills;
using Rair.Items.Properties;
namespace Rair {
    public sealed class Player : MonoBehaviour
    {
        public static Player Instance;

        public Skill cooking, dexerity;

        public List<Item> items = new();
        public Text itemLog;

        void Awake() {
            Instance = this;
        }

        void Start() {
            itemLog.text = "";
            cooking = new Cooking();
            cooking.AddExp(100);
            cooking.Acquire(Cooking.Grill);
            dexerity = new Dexterity();
            SampleSkillUI.Refresh(cooking, dexerity);
        }

        void Update() {
            if (cooking.Changed || dexerity.Changed) SampleSkillUI.Refresh(cooking, dexerity);
        }

        #region Item
        public static void AddItem(Item item) => Instance.Add(item);
        void Add(Item item) { items.Add(item); ItemLog(); SampleLogger.GotItem(item.name); }
        public static void RemoveItem(Item item) => Instance.Remove(item);
        void Remove(Item item) { if (items.Remove(item)) ItemLog(); }
        public static void UpdateLog() => Instance.ItemLog();
        /// <summary>주어진 조건을 만족하는 Item 한 개를 찾아 인벤토리에서 제거합니다.</summary>
        /// <returns>인벤토리에서 제거된 해당 Item을 반환합니다. 제거된 Item이 없으면 <see langword="null"/>을 반환합니다.</returns>
        public static Item FindAndRemoveItem(Func<Item, bool> predicate) => Instance.FRItem(predicate);
        Item FRItem(Func<Item, bool> predicate) {
            if (!items.Any(predicate)) return null;
            
            Item item = items.First(predicate);
            items.Remove(item);
            ItemLog();
            return item.Clone();
        }
        
        /// <summary>주어진 조건을 만족하는 Item 한 개를 찾습니다.</summary>
        /// <returns>해당 Item을 반환합니다. 조건을 만족하는 Item이 없으면 <see langword="null"/>을 반환합니다.</returns>
        public static Item FindItem(Func<Item, bool> predicate) => Instance.FItem(predicate);
        Item FItem(Func<Item, bool> predicate) {
            if (!items.Any(predicate)) return null;
            
            Item item = items.First(predicate);
            ItemLog();
            return item;
        }

        void ItemLog() {
            if (items.Count == 0) { itemLog.text = ""; return; }

            StringBuilder str = new();
            foreach (var item in items) {
                str.Append($"{item.name} ({item.durability.Value:F0}/{item.durability.MaxValue:F0})\n");
            }
            str.Remove(str.Length-1, 1); // remove '\n' in last item

            itemLog.text = str.ToString();
        }
        #endregion
    
        public void AddTool() {
            Item tool = new("간이도구", "다양한 작업에 사용할 수 있는 다목적 간이 도구입니다.", 20, 0, properties: Property.TierProps.Tool.Clone());

            AddItem(tool);
        }
    }
}