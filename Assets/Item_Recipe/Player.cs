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

        List<Item> items = new List<Item>();
        public Text itemLog;

        void Awake() {
            Instance = this;
        }

        void Start() {
            itemLog.text = "";
            cooking = new Cooking();
            cooking.AddExp(100);
            dexerity = new Dexterity();
            SampleSkillUI.Refresh(cooking, dexerity);
        }

        void Update() {
            if (cooking.changed || dexerity.changed) SampleSkillUI.Refresh(cooking, dexerity);
        }

        #region Item
        public static void AddItem(Item item) => Instance._AddItem(item);
        void _AddItem(Item item) { items.Add(item); ItemLog(); SampleLogger.GotItem(item.name); }
        public static void RemoveItem(Item item) => Instance._RemoveItem(item);
        void _RemoveItem(Item item) { if (items.Remove(item)) ItemLog(); }
        public static void UpdateLog() => Instance.ItemLog();
        /// <summary>주어진 조건을 만족하는 Item 한 개를 찾아 인벤토리에서 제거합니다.</summary>
        /// <returns>인벤토리에서 제거된 해당 Item을 반환합니다. 제거된 Item이 없으면 <see langword="null"/>을 반환합니다.</returns>
        public static Item FindAndRemoveItem(Func<Item, bool> predicate) => Instance._FindAndRemoveItem(predicate);
        Item _FindAndRemoveItem(Func<Item, bool> predicate) {
            if (!items.Any(predicate)) return null;
            
            Item item = items.First(predicate);
            items.Remove(item);
            ItemLog();
            return item.Clone();
        }
        
        /// <summary>주어진 조건을 만족하는 Item 한 개를 찾습니다.</summary>
        /// <returns>해당 Item을 반환합니다. 조건을 만족하는 Item이 없으면 <see langword="null"/>을 반환합니다.</returns>
        public static Item FindItem(Func<Item, bool> predicate) => Instance._FindItem(predicate);
        Item _FindItem(Func<Item, bool> predicate) {
            if (!items.Any(predicate)) return null;
            
            Item item = items.First(predicate);
            ItemLog();
            return item;
        }

        void ItemLog() {
            if (items.Count == 0) { itemLog.text = ""; return; }

            StringBuilder str = new StringBuilder();
            foreach (var item in items) {
                str.Append($"{item.name} ({item.durability.value.ToString("F0")}/{item.durability.maxValue.ToString("F0")})\n");
            }
            str.Remove(str.Length-1, 1); // remove '\n' in last item

            itemLog.text = str.ToString();
        }
        #endregion
    
        public void AddTool() {
            Item tool = new Item("간이도구", "다양한 작업에 사용할 수 있는 다목적 간이 도구입니다.", 20, 0, properties: Property.Tool);

            AddItem(tool);
        }
    }
}