using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

using Rair.Items;

namespace Rair.Rcp {
    public sealed partial class Recipe {
        public string label, description;
        public RequirementItem[] requirements;
        public Action<Recipe, Player> Behavior = (rcp, _) => Debug.LogWarning($"{rcp.label} 레시피의 동작이 정의되지 않았습니다.");

        /// <summary>레시피에 등록된 모든 아이템의 태그를 합산한 컬렉션을 가져옵니다.</summary>
        public Tag[] AllTags => Tag.Merge(requirements.SelectMany(r => r.Items, (_, i) => i.tags).ToArray());

        /// <summary>최종 사용 가능 여부를 확인 후 레시피를 실제로 사용합니다.</summary>
        /// <returns>레시피 실행 여부를 반환합니다.</returns>
        public bool Run(Player player) {
            if (Verify() && CanRunRecipe) {
                Behavior(this, player);
                Reset();
                return true;
            }
            return false;
        }
        void Reset() { foreach (var r in requirements) r.Clear(); }
        bool Verify() {
            if (requirements is null) Debug.LogWarning("레시피에 요구 아이템이 등록되지 않았습니다. 최소 한 개의 항목이 있어야 합니다.");
            else if (requirements.All(i => i.Conditions.Length != i.ConditionInfo.Length)) Debug.LogWarning("Item 항목의 조건 개수가 일치하지 않습니다.");
            else if (requirements.All(i => i.minCount > i.maxCount)) Debug.LogWarning("Item 항목의 최소 요구량이 최대 요구량을 초과합니다.");
            else return true;
            
            Debug.LogWarning("해당 Recipe 스크립트를 다시 확인해주세요.");
            return false;
        }

        /// <summary>모든 필요 조건이 만족되어 레시피를 사용할 수 있음을 나타냅니다.</summary>
        public bool CanRunRecipe => requirements.All(i => i.Satisfied);
    }

    public sealed class RequirementItem {
        public bool Satisfied => (_items.Count() >= minCount) && Items.All(item => item is null || Conditions.All(F => F(item)));
        public Func<Item, bool>[] Conditions { get; init; }
        public string[] ConditionInfo { get; init; }
        public string Label { get; init; }

        public string TextLog {
            get {
                string str = (maxCount == 1) ? $"{Label} :\n" : $"{Label} ({_items.Count}/{maxCount}) :\n";
                if (_items.Count != maxCount) foreach (var st in ConditionInfo) str += $"<color=gray>{st} (-)</color>\n";
                else {
                    for (int i=0; i < ConditionInfo.Length; i++) {
                        if (_items.All(Conditions[i])) str += $"<color=cyan>{ConditionInfo[i]} (O)</color>\n";
                        else str += $"<color=red>{ConditionInfo[i]} (X)</color>\n";
                    }
                }
                return str;
            }
        }

        readonly List<Item> _items = new();
        public ReadOnlyCollection<Item> Items => _items.AsReadOnly();
        /// <summary>아이템의 최대 수용량을 나타냅니다. 기본값은 1입니다.</summary>
        public int maxCount = 1;
        /// <summary>아이템의 최소 요구량을 나타냅니다. 기본값은 1입니다.</summary>
        public int minCount = 1;
        public bool Full => _items.Count == maxCount;

        /// <summary>해당 아이템이 요구조건을 만족하는지 검사합니다.</summary>
        public bool Check(Item item) => Conditions.All(F => F(item));
        /// <summary>해당 아이템이 요구조건을 만족하는지 검사하여 결과를 출력합니다.</summary>
        public void CheckLog(Item item) {
            Mono.SampleLogger.AddLog($"{item.name} 검사: {Check(item)}");
            for (int i=0; i < Conditions.Length; i++) {
                if (Conditions[i](item)) Mono.SampleLogger.AddLog($"<color=cyan>{ConditionInfo[i]} (O)</color>");
                else Mono.SampleLogger.AddLog($"<color=red>{ConditionInfo[i]} (X)</color>");
            }
        }
        /// <summary>지정 아이템 추가를 시도합니다.</summary>
        public void Add(Item item) {
            if (Check(item)) _items.Add(item);
            else Mono.SampleLogger.AddLog($"{item.name} 아이템이 요구조건을 만족하지 않았습니다.");
        }
        /// <summary>레시피에 추가된 아이템을 모두 제거합니다.</summary>
        public void Clear() => _items.Clear();
    }
}

namespace System.Runtime.CompilerServices { internal static class IsExternalInit {} } //? init 이니셜라이저 사용을 위해