using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using Rair.Mono;
using Rair.Items;
using Rair.Rcp;
namespace Rair {
    public sealed class Player : MonoBehaviour
    {
        public static Player Instance;

        public List<Item> items = new();
        public Text itemLog;

        public void Awake() {
            Instance = this;
        }

        Recipe rcp;    
        public void Start() {
            itemLog.text = "";

            rcp = Recipe.Sample1;
        }

        public void Update() {
            if (Input.GetKeyDown(KeyCode.F1)) {
                AddItem(Item.무작위_광석);
            }
            if (Input.GetKeyDown(KeyCode.F2)) {
                if (items.Count != 0) RemoveItem(items[0]);
            }
            if (Input.GetKeyDown(KeyCode.F3)) {
                var item = FindAndRemoveItem(i => rcp.requirements[0].Check(i));
                if (item is null) SampleLogger.AddLog("조건을 만족하는 아이템이 없습니다.");
                else {
                    rcp.requirements[0].Add(item);
                    rcp.Run(this);
                }
            }
            if (Input.GetKeyDown(KeyCode.F12)) {
                foreach (var item in items) rcp.requirements[0].CheckLog(item);
            }
        }


        #region Item
        public void AddItem(Item item) { items.Add(item); ItemLog(); SampleLogger.GotItem(item.name); }
        public void RemoveItem(Item item) { if (items.Remove(item)) ItemLog(); SampleLogger.LostItem(item.name); }
        /// <summary>주어진 조건을 만족하는 Item 한 개를 찾아 인벤토리에서 제거합니다.</summary>
        /// <returns>인벤토리에서 제거된 해당 Item을 반환합니다. 제거된 Item이 없으면 <see langword="null"/>을 반환합니다.</returns>
        public Item FindAndRemoveItem(Func<Item, bool> predicate) {
            if (!items.Any(predicate)) return null;
            
            Item item = items.First(predicate);
            items.Remove(item);
            ItemLog();
            return item;
        }
        
        /// <summary>주어진 조건을 만족하는 Item 한 개를 찾습니다.</summary>
        /// <returns>해당 Item을 반환합니다. 조건을 만족하는 Item이 없으면 <see langword="null"/>을 반환합니다.</returns>
        public Item FindItem(Func<Item, bool> predicate) {
            if (!items.Any(predicate)) return null;
            
            Item item = items.First(predicate);
            ItemLog();
            return item;
        }

        void ItemLog() {
            if (items.Count == 0) { itemLog.text = ""; return; }

            string str = "";
            foreach (var item in items) {
                if (item.tags.Count == 0) str += item.name + "\n";
                else str += item.name + "(" + string.Join(", ", item.tags) + ")\n";
            }

            itemLog.text = str;
        }
        #endregion
    }
}