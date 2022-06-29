using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rair.Items;
using Rair.Items.Properties;
using Rair.Skills;

//? [abstract -> each class] type of Recipe is no longer developed
namespace Rair.OldRecipes {
    #region Interfaces
    public abstract class ARecipe {
        /// <summary>플레이어에게 표시되는 레시피의 이름입니다.</summary>
        public abstract string Label { get; }
        public virtual RequirementItem[] ReqItems => null;
        public virtual RequirementTool[] ReqTools => null;
        public virtual RequirementSkill[] ReqSkill => null;
        public virtual RequirementInst[] ReqInsts => null;
        protected abstract void RunRecipe();

        /// <summary>최종 사용 가능 여부를 확인 후 레시피를 실제로 사용합니다.</summary>
        /// <returns>레시피 실행 여부를 반환합니다.</returns>
        public bool Run() {
            if (CanRunRecipe) { RunRecipe(); return true; }
            return false;
        }

        /// <summary>모든 필요 조건이 만족되어 레시피를 사용할 수 있음을 나타냅니다.</summary>
        public bool CanRunRecipe {
            get {
                return (ReqItems == null || ReqItems.All(r => r.Initialized && r.Satisfied))
                    && (ReqTools == null || ReqTools.All(r => r.Initialized && r.Satisfied))
                    && (ReqSkill == null || ReqSkill.All(r => r.Initialized && r.Satisfied))
                    && (ReqInsts == null || ReqInsts.All(r => r.Initialized && r.Satisfied))
                ;
            }
        }
    }

    /// <summary><see cref="ARecipe"/>를 사용하기 위한 개별 필요 조건을 기술하는 인터페이스입니다.</summary>
    public abstract class Requirement {
        /// <summary>호출 시점에 본 필요 조건이 충족되었는지를 나타냅니다.</summary>
        public bool Satisfied => Constraints.All(_ => _);
        /// <summary>제약 조건을 간략히 설명하는 텍스트를 기술합니다.</summary>
        public abstract string[] ConstraintsInfo { get; }
        /// <summary>제약조건 검사에 필요한 객체가 모두 지정되었는지 검사합니다.</summary>
        public abstract bool Initialized { get; }
        /// <summary>해당 필요 물품의 제약조건을 검사합니다.</summary>
        public abstract bool[] Constraints { get; }
        /// <summary>인터페이스에 표시할 재료/조건의 이름을 나타냅니다.</summary>
        /// <example>요리 Recipe의 주 재료는 "식재료"로 표현될 수 있습니다.</example>
        public abstract string Label { get; }
        public string Text {
            get {
                string str = $"{Label} :\n";
                foreach (string st in ConstraintsInfo) str += $"<color=gray>{st} (-)</color>\n";
                return str;
            }
        }
        public string TextWithCheck {
            get {
                string str = $"{Label} :\n";
                for (int i=0; i < ConstraintsInfo.Length; i++) {
                    if (Constraints[i]) str += $"<color=cyan>{ConstraintsInfo[i]} (O)</color>\n";
                    else                str += $"<color=red>{ConstraintsInfo[i]} (X)</color>\n";
                }
                return str;
            }
        }
    }
    public abstract class RequirementItem : Requirement {
        protected RequirementItem() { _items = new Item[MaxCount]; }

        public override bool Initialized => Item != null;
        public virtual Item Item { get; set; }

        /// <summary>해당 아이템이 조건을 만족하는지 확인합니다.</summary>
        public bool Satisfying(Item item) => CheckItems.All(f => f(item));
        //? Item을 선택하여 제약조건을 검사할 수 있게 오버라이드합니다.
        public override bool[] Constraints => CheckItems.Select(f => f(Item)).ToArray();
        /// <summary>아이템을 받아 조건 충족 여부를 검사하는 배열입니다.</summary>
        public abstract Func<Item, bool>[] CheckItems { get; }

        //? 아이템을 여러 개 요구하는 조건일 경우 아래 멤버들을 사용합니다.
        public bool SingleItem => MaxCount == 1;

        protected Item[] _items;
        public virtual Item[] Items => (_items ??= new Item[MaxCount]);

        public virtual int Count { get; set; }
        public virtual int MaxCount => 1;
    }
    public abstract class RequirementSkill : Requirement {
        public override bool Initialized => Skill != null;
        public Skill Skill { get; init; }
    }
    public abstract class RequirementTool : Requirement {
        public override bool Initialized => Tool != null;
        public virtual Item Tool { get; set; }

        /// <summary>해당 아이템이 조건을 만족하는지 확인합니다.</summary>
        public bool Satisfying(Item item) => CheckItems.All(f => f(item));
        //? Item을 선택하여 제약조건을 검사할 수 있게 오버라이드합니다.
        public override bool[] Constraints => CheckItems.Select(f => f(Tool)).ToArray();
        /// <summary>아이템을 받아 조건 충족 여부를 검사하는 배열입니다.</summary>
        public abstract Func<Item, bool>[] CheckItems { get; }
    }
    public abstract class RequirementInst : Requirement {
        protected const float refreshTimeout = 5f;
    }
    #endregion

    public sealed class Grill : ARecipe {
        private Grill() {}

        static Grill _inst;
        public static Grill Instance => _inst ??= new Grill();

        public override string Label => "굽기";
        public override RequirementItem[] ReqItems => new RequirementItem[2] { main, fuel };
        public override RequirementTool[] ReqTools => new RequirementTool[1] { tool };
        public override RequirementSkill[] ReqSkill => new RqSkill[1] { skill };
        class MainItem : RequirementItem {
            public override Func<Item, bool>[] CheckItems => new Func<Item, bool>[3] {
                item => item.calorie.Value >= 5f && item.calorie.Value <= 50f,
                item => item.durability.Value >= 30f,
                item => item.recipeCount.Value > 0
            };
            public override string[] ConstraintsInfo => new string[3] {
                "열량 5 ~ 50",
                "내구도 30 이상",
                "추가 작업 가능"
            };
            public override string Label => "식재료";
        } readonly MainItem main = new();
        class FuelItem : RequirementItem {
            public override Func<Item, bool>[] CheckItems => new Func<Item, bool>[2] {
                item => item.durability.Value > 0,
                item => item.combustibility.Value >= 100
            };
            public override string[] ConstraintsInfo => new string[2] {
                "내구도 0 초과",
                "연료 100 이상"
            };
            public override string Label => "연료";
        } readonly FuelItem fuel = new();
        class RqSkill  : RequirementSkill {
            public override bool[] Constraints => new bool[2] {
                Skill.GetType().IsAssignableFrom(typeof(Cooking)),
                Skill.Lv > 0
            };
            public override string[] ConstraintsInfo => new string[2] {
                "요리 스킬",
                "1레벨 이상"
            };
            public override string Label => "필요 스킬";
        } readonly RqSkill skill = new();
        class ReqTool  : RequirementTool {
            public override Item Tool { get; set; }
            public override string Label => "조리 도구";
            public override Func<Item, bool>[] CheckItems => new Func<Item, bool>[2] {
                tool => tool.durability.Value > 0,
                tool => tool.HasProperty(Property.TierProps.Tool.Clone())
            };

            public override string[] ConstraintsInfo => new string[2] {
                "내구도 0 초과",
                "도구"
            };
        } readonly ReqTool tool = new();

        public void SetInstance(Item mainItem, Item fuelItem, Item tool) {
            main.Item = mainItem;
            fuel.Item = fuelItem;
            this.tool.Tool = tool;
        }

        protected override void RunRecipe() {
            Item resItem = main.Item.Clone();

            resItem.name = resItem.coreName + " 구이";
            float ratio = resItem.durability.Ratio;
            resItem.durability.MaxValue -= 10;
            resItem.durability.Ratio = ratio;
            resItem.calorie.Value *= 2.5f;
            resItem.recipeCount.Value -= 1;
            resItem.properties.Remove(Property.Cooking.RawFood.Clone());
            Player.AddItem(resItem);

            fuel.Item.combustibility.Value -= 100;

            skill.Skill.AddExp(15f);

            tool.Tool.durability.Value -= 2f; //TODO 2f - (reqSkill.skill.lv/reqSkill.skill.maxLv);

            //* Reset
            main.Item = null;
            fuel.Item = null;
            tool.Tool = null;
        }
    }

    public sealed class Sewing : ARecipe {
        private Sewing() {}
        static Sewing _inst;
        public static Sewing Instance => _inst ??= new Sewing();

        public override string Label => "바느질";

        public override RequirementItem[] ReqItems => new RequirementItem[1] { fiber };
        public override RequirementTool[] ReqTools => new RequirementTool[1] { needle };
        public override RequirementSkill[] ReqSkill => new RequirementSkill[1] { skill };

        class Fiber : RequirementItem {
            public override string Label => "섬유";
            public override string[] ConstraintsInfo => new string[3] {
                "내구도 80% 이상",
                "섬유 조각",
                "5개"
            };
            public override Func<Item, bool>[] CheckItems => new Func<Item, bool>[3] {
                item => item.durability.Ratio >= 0.8f,
                item => item.HasProperty(Property.Cooking.Fiber.Clone()),
                item => Count == MaxCount
            };

            public override Item Item {
                get => _items[Count-1];
                set { if (Count != MaxCount) _items[Count++] = value; }
            }
            public override int MaxCount => 5;
        } readonly Fiber fiber = new();

        class Needle : RequirementTool {
            public override string Label => "바늘";
            public override string[] ConstraintsInfo => new string[2] {
                "망가지지 않음",
                "바늘"
            };
            public override Func<Item, bool>[] CheckItems => new Func<Item, bool>[2] {
                tool => tool.durability.Value > 0,
                tool => tool.HasProperty(Property.Cooking.Needle.Clone())
            };
        } readonly Needle needle = new();

        class ReqDex : RequirementSkill {
            public ReqDex() { Skill = new Dexterity(); }
            public override string Label => "재봉 스킬";
            public override string[] ConstraintsInfo => new string[1] { "직조 기술 보유" };
            public override bool[] Constraints => new bool[1] { Skill.Acquired(Dexterity.Weave) };
        } readonly ReqDex skill = new();

        protected override void RunRecipe() {
            Item resItem = fiber.Items[0].Clone(), refItem = fiber.Items[1];

            resItem.name = resItem.coreName + " 직물";
            resItem.description = $"{resItem.coreName}? 가공하여 직물로 짠 것"; //TODO <- string.AddParticle() 구현 후 수정
            resItem.durability.MaxValue = fiber.Items.Average(i => i.durability.MaxValue);
            resItem.durability.Ratio = fiber.Items.Average(i => i.durability.Ratio);
            resItem.recipeCount.Value = Mathf.RoundToInt((float)fiber.Items.Average(i => i.recipeCount.Value)) + 1;
            resItem.combustibility.Value = fiber.Items.Average(i => i.combustibility.Value);

            resItem.RemoveProperty(Property.Cooking.Fiber.Clone());
            resItem.AddProperty(Property.Cooking.Fabric.Clone());

            Player.AddItem(resItem);
            needle.Tool.durability.Value -= 2;
            skill.Skill.AddExp(10);
            fiber.Count = 0;
        }
    }
}

namespace Rair.Rcp {
    public sealed class Recipe {
        public string label, description;
        public RequirementItem[] reqItems;
        public RequirementTool[] reqTools;
        public RequirementSkill[] reqSkill;
        public RequirementInst installation;
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
        void Reset() {
            foreach (var r in reqItems) r.items.Clear();
        }
        bool Verify() {
            if (reqItems is not null) {
                if (reqItems.All(i => i.Conditions.Length == i.ConditionInfo.Length)) {
                    if (reqItems.All(i => i.minCount <= i.maxCount)) {
                        if (reqTools is not null && reqTools.All(t => t.Conditions.Length != t.ConditionInfo.Length)) Debug.LogWarning("Tool 항목의 조건 개수가 일치하지 않습니다.");
                        else return true; //* Verified.
                    }
                    else Debug.LogWarning("Item 항목의 최소 요구량이 최대 요구량을 초과합니다.");
                }
                else Debug.LogWarning("Item 항목의 조건 개수가 일치하지 않습니다.");
            }
            else Debug.LogWarning("레시피에 요구 아이템이 등록되지 않았습니다. 최소 한 개의 항목이 있어야 합니다.");
            
            Debug.LogWarning("해당 Recipe 스크립트를 다시 확인해주세요.");
            return false;
        }

        /// <summary>모든 필요 조건이 만족되어 레시피를 사용할 수 있음을 나타냅니다.</summary>
        public bool CanRunRecipe {
            get {
                return reqItems.All(r => r.Satisfied)
                && (reqTools is null || reqTools.All(r => r.Satisfied))
                && (reqSkill is null || reqSkill.All(r => r.Satisfied))
                && (installation is null/* || installation.satisfied*/)
                ;
            }
        }
    }

    public sealed class RequirementItem {
        public bool Satisfied => (items.Count() >= minCount) && items.All(item => item is null || Conditions.All(F => F(item)));
        public Func<Item, bool>[] Conditions { get; init; }
        public string[] ConditionInfo { get; init; }
        public string Label { get; init; }

        public string TextLog {
            get {
                string str = (maxCount == 1) ? $"{Label} :\n" : $"{Label} ({items.Count}/{maxCount}) :\n";
                if (items.Count != maxCount) {
                    foreach (var st in ConditionInfo) str += $"<color=gray>{st} (-)</color>\n";
                }
                else {
                    for (int i=0; i < ConditionInfo.Length; i++) {
                        if (items.All(Conditions[i])) str += $"<color=cyan>{ConditionInfo[i]} (O)</color>\n";
                        else                str += $"<color=red>{ConditionInfo[i]} (X)</color>\n";
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
    public sealed class RequirementTool {
        public bool Satisfied => item is not null && Conditions.All(F => F(item));
        public Func<Item, bool>[] Conditions { get; init; }
        public string[] ConditionInfo { get; init; }
        public string Label { get; init; }
        public string TextLog {
            get {
                string str = $"{Label} :\n";
                if (item is null) {
                    foreach (var st in ConditionInfo) str += $"<color=gray>{st} (-)</color>\n";
                }
                else {
                    for (int i=0; i < ConditionInfo.Length; i++) {
                        if (Check(item)) str += $"<color=cyan>{ConditionInfo[i]} (O)</color>\n";
                        else             str += $"<color=red>{ConditionInfo[i]} (X)</color>\n";
                    }
                }
                return str;
            }
        }

        public Item item;
        public bool Check(Item item) => Conditions.All(F => F(item));
    }
    public sealed class RequirementSkill {
        public bool Satisfied => skill is not null && skill.GetType().Name == SkillType && Conditions.All(F => F(skill));
        public Func<Skill, bool>[] Conditions { get; init; }
        public string[] ConditionInfo { get; init; }
        public string SkillType { get; init; }        
        public string TextLog {
            get {
                string str = $"{SkillType} :\n";
                if (skill is null) {
                    foreach (var st in ConditionInfo) str += $"<color=gray>{st} (-)</color>\n";
                }
                else {
                    for (int i=0; i < ConditionInfo.Length; i++) {
                        if (Conditions[i](skill)) str += $"<color=cyan>{ConditionInfo[i]} (O)</color>\n";
                        else             str += $"<color=red>{ConditionInfo[i]} (X)</color>\n";
                    }
                }
                return str;
            }
        }

        public Skill skill;
        public bool Check(Skill skill) => Conditions.All(F => F(skill));
    }
    public sealed class RequirementInst {
        public bool Satisfied => Conditions.All(F => F());
        public Func<bool>[] Conditions { get; init; }
    }

    public static class Recipes {
        //* Samples
        public static Recipe Grill = new() {
            label = "굽기",
            description = "음식을 간단히 구워냅니다.",
            reqItems = new RequirementItem[] { //? 2슬롯 / 식재료 + 연료
                new() {
                    ConditionInfo = new string[3] {
                        "액체가 아닌 ",
                        "내구도 50% 이상",
                        "추가 작업 가능"
                    },
                    Conditions = new Func<Item, bool>[] {
                        item => item.HasProperty(Property.Cooking.Edible.Clone()) && !item.HasProperty(Property.Cooking.Liquid.Clone()),
                        item => item.durability.Ratio >= 0.5f,
                        item => item.recipeCount.Value > 0
                    },
                    Label = "식재료"
                },
                new() {
                    Conditions = new Func<Item, bool>[] {
                        item => item.durability.Value > 0,
                        item => item.combustibility.Value >= 100
                    },
                    ConditionInfo = new string[] {
                        "내구도 0 초과",
                        "연료 100 이상"
                    },
                    Label = "연료"
                }
            },
            reqTools = new RequirementTool[] { //? 1슬롯 / 조리도구
                new() {
                    Conditions = new Func<Item, bool>[] {
                        item => item.durability.Value > 0,
                        item => item.HasProperty(Property.TierProps.Tool.Clone())
                    },
                    ConditionInfo = new string[] {
                        "내구도 0 초과",
                        "도구"
                    },
                    Label = "조리도구"
                }
            },
            reqSkill = new RequirementSkill[] { //? 요리 / 굽기 기술 보유
                new() {
                    SkillType = nameof(Cooking),
                    Conditions = new Func<Skill, bool>[] { skill => skill.Acquired(Cooking.Grill) },
                    ConditionInfo = new string[] { "굽기 기술 보유" }
                }
            },
            Behavior = rcp => {
                Item resItem = rcp.reqItems[0].items[0].Clone();
                resItem.name = resItem.coreName + " 구이";
                float ratio = resItem.durability.Ratio;
                resItem.durability.MaxValue -= 10;
                resItem.durability.Ratio = ratio;
                resItem.calorie.Value *= 2.5f;
                resItem.recipeCount.Value -= 1;
                resItem.properties.Remove(Property.Cooking.RawFood);
                Property p;
                if (resItem.HasProperty(Property.Cooking.Cooked)) {
                    int progress = Player.Instance.cooking.LvRate switch {
                        > 0.8f => 1,
                        > 0.5f => UnityEngine.Random.value > 0.5f ? 2 : 1,
                        > 0.3f => UnityEngine.Random.value > 0.5f ? 3 : 2,
                        _ => 3
                    };
                    p = resItem.GetProperty(Property.Cooking.Cooked);
                    p.Level += progress;
                }
                else {
                    p = Property.Cooking.Cooked.Clone(Mathf.RoundToInt(3.5f + UnityEngine.Random.Range(-0.5f, 0.5f) * 0.4f / Player.Instance.cooking.LvRate));
                    resItem.properties.Add(p);
                }
                resItem.name = p.Level switch {
                    1 => "설익은 " + resItem.name,
                    2 => "레어 " + resItem.name,
                    3 => "미디움 " + resItem.name,
                    4 => "웰던 " + resItem.name,
                    5 => "타버린 " + resItem.name,
                    _ => resItem.name
                };
                Player.AddItem(resItem);

                rcp.reqItems[1].items[0].combustibility.Value -= 100;

                rcp.reqSkill[0].skill.AddExp(15f);

                rcp.reqTools[0].item.durability.Value -= 2f; //TODO 2f - (reqSkill.skill.lv/reqSkill.skill.maxLv);
            }
        };

        public static Recipe Grind = new() {
            label = "빻기",
            description = "재료를 잘게 빻아 가루를 냅니다.",
            reqItems = new RequirementItem[] { //? 1슬롯 / 1~10개 / 식재료
                new() {
                    maxCount = 10,
                    Conditions = new Func<Item, bool>[] {
                        item => item.durability.Value > 0,
                        item => item.recipeCount.Value > 0,
                        item => item.HasProperty(Property.Cooking.RawFood.Clone())
                    },
                    ConditionInfo = new string[] {
                        "내구도 0 초과",
                        "추가 작업 가능",
                        "식재료"
                    },
                    Label = "식재료"
                }
            },
            reqTools = new RequirementTool[] { //? 2슬롯 / 절구+절굿공이 / 내구도 > 0
                new() {
                    Conditions = new Func<Item, bool>[] {
                        item => item.durability.Value > 0,
                        item => item.HasProperty(Property.Cooking.Mortar.Clone())
                    },
                    ConditionInfo = new string[] {
                        "내구도 0 초과",
                        "막자사발"
                    },
                    Label = "절구"
                },
                new() {
                    Conditions = new Func<Item, bool>[] {
                        item => item.durability.Value > 0,
                        item => item.HasProperty(Property.Cooking.Pestle.Clone())
                    },
                    ConditionInfo = new string[] {
                        "내구도 0 초과",
                        "막자"
                    },
                    Label = "절굿공이"
                }
            },
            reqSkill = new RequirementSkill[] { //? 요리 / 제분 기술 보유
                new() {
                    Conditions = new Func<Skill, bool>[] { skill => skill.Acquired(Cooking.Grind) },
                    ConditionInfo = new string[] { "제분 기술 보유" },
                    SkillType = nameof(Cooking)
                }
            },
            Behavior = rcp => {
                var items = rcp.reqItems.Single().items;
                Item item = items.Random();

                item.durability.MaxValue *= 1.2f;
                item.durability.Ratio = items.Average(i => i.durability.Ratio);
                item.RemoveProperty(Property.Cooking.RawFood.Clone());
                item.AddProperty(Property.Cooking.Flour.Clone());

                foreach (var tool in rcp.reqTools) tool.item.durability.Value -= 1;
                for (var _=0; _<items.Count; _++) Player.AddItem(item); //TODO 검증 과정 구현(인벤토리 크기 or bool 반환)
                rcp.reqSkill.Single().skill.AddExp(1f); 
            }
        };
    
        public static Recipe Knead = new() {
            label = "반죽",
            description = "재료를 액체와 섞어 반죽 형태로 만듭니다.",
            reqItems = new RequirementItem[] { //? 2슬롯 / 가루(5개) + 액체(??)
                new() {
                    minCount = 5, maxCount = 5,
                    Conditions = new Func<Item, bool>[] {
                        item => item.durability.Value > 0,
                        item => item.HasProperty(Property.Cooking.Powder.Clone()),
                    },
                    ConditionInfo = new string[] {
                        "내구도 0 초과",
                        "가루 형태"
                    },
                    Label = "가루"
                },
                new() {
                    maxCount = 1,
                    Conditions = new Func<Item, bool>[] {
                        item => item.durability.Value > 0,
                        item => item.HasProperty(Property.Cooking.Liquid.Clone())
                    },
                    ConditionInfo = new string[] {
                        "내구도 0 초과",
                        "액체"
                    },
                    Label = "액체"
                }
            },
            reqSkill = new RequirementSkill[] { //? 요리 / 반죽 기술 보유
                new() {
                    Conditions = new Func<Skill, bool>[] { skill => skill.Acquired(Cooking.Knead) },
                    ConditionInfo = new string[] { "반죽 기술 보유" },
                    SkillType = nameof(Cooking)
                }
            },
            Behavior = rcp => {
                List<Item> flour = rcp.reqItems[0].items, liquid = rcp.reqItems[1].items;
                Item item = flour.Random();

                item.durability.MaxValue = flour.Average(i => i.durability.Value) * 0.7f;
                item.durability.Ratio = 1;
                item.RemoveProperty(Property.Cooking.Powder.Clone(), Property.Cooking.Dust.Clone(), Property.Cooking.Flour.Clone());
                if (flour.All(i => i.HasProperty(Property.Cooking.Edible.Clone())) && liquid.All(i => i.HasProperty(Property.Cooking.Edible.Clone()))) item.AddProperty(Property.Cooking.Edible.Clone());
                else item.RemoveProperty(Property.Cooking.Edible.Clone());

                Player.AddItem(item);
                rcp.reqSkill.Single().skill.AddExp(2f);
            }
        };

        public static Recipe Weave = new() {
            label = "바느질",
            description = "섬유 조각들을 천으로 재단합니다.",
            reqItems = new RequirementItem[] { //? 2슬롯 / 섬유 조각(5개) / 실(1개)
                new() {
                    Label = "섬유",
                    minCount = 5, maxCount = 5,
                    Conditions = new Func<Item, bool>[] {
                        item => item.durability.Ratio >= 0.8f,
                        item => item.HasProperty(Property.Cooking.Fiber.Clone())
                    },
                    ConditionInfo = new string[] {
                        "내구도 80% 이상",
                        "섬유 조각"
                    }
                },
                new() {
                    Label = "실",
                    Conditions = new Func<Item, bool>[] {
                        item => item.HasProperty(Property.Cooking.Thread.Clone()),
                        item => item.durability.Value >= 1f
                    },
                    ConditionInfo = new string[] {
                        "실",
                        "내구도 1 이상"
                    }
                }
            },
            reqTools = new RequirementTool[] { //? 1슬롯 / 바늘
                new() {
                    Label = "바늘",
                    Conditions = new Func<Item, bool>[] {
                        item => item.HasProperty(Property.Cooking.Needle.Clone()),
                        item => item.durability.Value > 0
                    },
                    ConditionInfo = new string[] {
                        "바늘",
                        "내구도 0 초과"
                    }
                }
            },
            reqSkill = new RequirementSkill[] { //? 손재주 / 바느질 보유
                new() {
                    SkillType = nameof(Dexterity),
                    Conditions = new Func<Skill, bool>[] { skill => skill.Acquired(Dexterity.Weave) },
                    ConditionInfo = new string[] { "바느질 기술 보유" }
                }
            },
            Behavior = rcp => {
                Item result = rcp.reqItems[0].items.Random().Clone();
                List<Item> refItems = rcp.reqItems[0].items;
    
                result.name = result.coreName + " 직물";
                result.description = $"{result.coreName}? 가공하여 직물로 짠 것"; //TODO <- string.AddParticle() 구현 후 수정
                result.durability.MaxValue = refItems.Average(i => i.durability.MaxValue);
                result.durability.Ratio = refItems.Average(i => i.durability.Ratio);
                result.recipeCount.Value = Mathf.RoundToInt((float)refItems.Average(i => i.recipeCount.Value)) + 1;
                result.combustibility.Value = refItems.Average(i => i.combustibility.Value);
    
                result.RemoveProperty(Property.Cooking.Fiber.Clone());
                result.AddProperty(Property.Cooking.Fabric.Clone());
    
                Player.AddItem(result);
                rcp.reqTools[0].item.durability.Value -= 2;
                rcp.reqSkill[0].skill.AddExp(10);
            }
        };
    }


    public static class LinqMethods {
        public static T Random<T>(this IEnumerable<T> enumerable) where T : class =>
            (enumerable.Count() < 1)
            ? null
            : enumerable.ElementAt(UnityEngine.Random.Range(0, enumerable.Count()))
        ;
    }
}



namespace System.Runtime.CompilerServices { internal static class IsExternalInit {} } //? init 이니셜라이저 사용을 위해