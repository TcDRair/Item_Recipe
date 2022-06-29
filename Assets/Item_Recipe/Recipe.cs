using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rair.Items;
using Rair.Items.Properties;

namespace Rair.Rcp {
    public sealed class Recipe {
        public string label, description;
        public RequirementItem[] requirements;
        public Action<Recipe> Behavior = rcp => Debug.LogWarning($"{rcp.label} 레시피의 동작이 정의되지 않았습니다.");

        /// <summary>최종 사용 가능 여부를 확인 후 레시피를 실제로 사용합니다.</summary>
        /// <returns>레시피 실행 여부를 반환합니다.</returns>
        public bool Run() {
            if (Verify() && CanRunRecipe) {
                Behavior(this);
                Reset();
                return true;
            }
            return false;
        }
        void Reset() { foreach (var r in requirements) r.items.Clear(); }
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
        public bool Satisfied => (items.Count() >= minCount) && items.All(item => item is null || Conditions.All(F => F(item)));
        public Func<Item, bool>[] Conditions { get; init; }
        public string[] ConditionInfo { get; init; }
        public string Label { get; init; }

        public string TextLog {
            get {
                string str = (maxCount == 1) ? $"{Label} :\n" : $"{Label} ({items.Count}/{maxCount}) :\n";
                if (items.Count != maxCount) foreach (var st in ConditionInfo) str += $"<color=gray>{st} (-)</color>\n";
                else {
                    for (int i=0; i < ConditionInfo.Length; i++) {
                        if (items.All(Conditions[i])) str += $"<color=cyan>{ConditionInfo[i]} (O)</color>\n";
                        else str += $"<color=red>{ConditionInfo[i]} (X)</color>\n";
                    }
                }
                return str;
            }
        }

        public List<Item> items = new();
        /// <summary>아이템의 최대 수용량을 나타냅니다. 기본값은 1입니다.</summary>
        public int maxCount = 1;
        /// <summary>아이템의 최소 요구량을 나타냅니다. 기본값은 1입니다.</summary>
        public int minCount = 1;
        public bool Full => items.Count == maxCount;
        /// <summary>해당 아이템이 요구조건을 만족하는지 검사합니다.</summary>
        public bool Check(Item item) => Conditions.All(F => F(item));
    }

    public static class Recipes {
        //* Samples
        public static Recipe Grill = new();
    }
}

namespace System.Runtime.CompilerServices { internal static class IsExternalInit {} } //? init 이니셜라이저 사용을 위해