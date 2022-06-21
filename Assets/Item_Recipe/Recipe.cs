using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Rair.Items.Properties;
using Rair.Skills;

namespace Rair.Items.Recipe.Abstract {
    #region Interfaces
    public abstract class ARecipe {
        /// <summary>플레이어에게 표시되는 레시피의 이름입니다.</summary>
        public abstract string label { get; }
        public virtual RequirementItem[] reqItems => null;
        public virtual RequirementTool[] reqTools => null;
        public virtual RequirementSkill[] reqSkill => null;
        public virtual RequirementInst[] reqInsts => null;
        protected abstract void _RunRecipe();

        /// <summary>최종 사용 가능 여부를 확인 후 레시피를 실제로 사용합니다.</summary>
        /// <returns>레시피 실행 여부를 반환합니다.</returns>
        public bool RunRecipe() {
            if (canRunRecipe) { _RunRecipe(); return true; }
            return false;
        }

        /// <summary>모든 필요 조건이 만족되어 레시피를 사용할 수 있음을 나타냅니다.</summary>
        public bool canRunRecipe {
            get {
                return (reqItems == null || reqItems.All(r => r.initialized && r.satisfied))
                    && (reqTools == null || reqTools.All(r => r.initialized && r.satisfied))
                    && (reqSkill == null || reqSkill.All(r => r.initialized && r.satisfied))
                    && (reqInsts == null || reqInsts.All(r => r.initialized && r.satisfied))
                ;
            }
        }
    }

    /// <summary><see cref="ARecipe"/>를 사용하기 위한 개별 필요 조건을 기술하는 인터페이스입니다.</summary>
    public abstract class Requirement {
        /// <summary>호출 시점에 본 필요 조건이 충족되었는지를 나타냅니다.</summary>
        public bool satisfied => constraints.All(_ => _);
        /// <summary>제약 조건을 간략히 설명하는 텍스트를 기술합니다.</summary>
        public abstract string[] constraintsInfo { get; }
        /// <summary>제약조건 검사에 필요한 객체가 모두 지정되었는지 검사합니다.</summary>
        public abstract bool initialized { get; }
        /// <summary>해당 필요 물품의 제약조건을 검사합니다.</summary>
        public abstract bool[] constraints { get; }
        /// <summary>인터페이스에 표시할 재료/조건의 이름을 나타냅니다.</summary>
        /// <example>요리 Recipe의 주 재료는 "식재료"로 표현될 수 있습니다.</example>
        public abstract string label { get; }
        public string fullText {
            get {
                string str = $"{label} :\n";
                foreach (string st in constraintsInfo) str += $"<color=gray>{st} (-)</color>\n";
                return str;
            }
        }
        public string fullTextWithCheck {
            get {
                string str = $"{label} :\n";
                for (int i=0; i < constraintsInfo.Length; i++) {
                    if (constraints[i]) str += $"<color=cyan>{constraintsInfo[i]} (O)</color>\n";
                    else                str += $"<color=red>{constraintsInfo[i]} (X)</color>\n";
                }
                return str;
            }
        }
    }
    public abstract class RequirementItem : Requirement {
        protected RequirementItem() { _items = new Item[maxCount]; }

        public override bool initialized => item != null;
        public virtual Item item { get; set; }

        /// <summary>해당 아이템이 조건을 만족하는지 확인합니다.</summary>
        public bool Satisfying(Item item) => constItems.All(f => f(item));
        //? Item을 선택하여 제약조건을 검사할 수 있게 오버라이드합니다.
        public override bool[] constraints => constItems.Select(f => f(item)).ToArray();
        /// <summary>아이템을 받아 조건 충족 여부를 검사하는 배열입니다.</summary>
        public abstract Func<Item, bool>[] constItems { get; }


        //? 아이템을 여러 개 요구하는 조건일 경우 아래 멤버들을 사용합니다.
        public bool singleItem => maxCount == 1;


        protected Item[] _items;
        public virtual Item[] items => (_items ??= new Item[maxCount]);

        public virtual int count { get; set; }
        public virtual int maxCount => 1;
    }

    public abstract class RequirementSkill : Requirement {
        public override bool initialized => skill != null;
        public virtual Skill skill { get; set; }
    }
    public abstract class RequirementTool : Requirement {
        public override bool initialized => tool != null;
        public virtual Item tool { get; set; }

        /// <summary>해당 아이템이 조건을 만족하는지 확인합니다.</summary>
        public bool Satisfying(Item item) => constItems.All(f => f(item));
        //? Item을 선택하여 제약조건을 검사할 수 있게 오버라이드합니다.
        public override bool[] constraints => constItems.Select(f => f(tool)).ToArray();
        /// <summary>아이템을 받아 조건 충족 여부를 검사하는 배열입니다.</summary>
        public abstract Func<Item, bool>[] constItems { get; }
    }
    public abstract class RequirementInst : Requirement {
        protected const float refreshTimeout = 5f;
        //TODO : 건물 내부 기능은 Rair.Building.Installation이라는 클래스로 구현한다... 언젠가
    }
    #endregion

    public sealed class Grill : ARecipe {
        private Grill() {}

        static Grill _inst;
        public static Grill Instance => (_inst ??= new Grill());

        public override string label => "굽기";
        public override RequirementItem[] reqItems => new RequirementItem[2] { main, fuel };
        public override RequirementTool[] reqTools => new RequirementTool[1] { tool };
        public override RequirementSkill[] reqSkill => new ReqSkill[1] { skill };
        class MainItem : RequirementItem {
            public override Func<Item, bool>[] constItems => new Func<Item, bool>[3] {
                item => item.calorie.value >= 5f && item.calorie.value <= 50f,
                item => item.durability.value >= 30f,
                item => item.recipeCount.value > 0
            };
            public override string[] constraintsInfo => new string[3] {
                "열량 5 ~ 50",
                "내구도 30 이상",
                "추가 작업 가능"
            };
            public override string label => "식재료";
        } MainItem main = new MainItem();
        class FuelItem : RequirementItem {
            public override Func<Item, bool>[] constItems => new Func<Item, bool>[2] {
                item => item.durability.value > 0,
                item => item.combustibility.value >= 100
            };
            public override string[] constraintsInfo => new string[2] {
                "내구도 0 초과",
                "연료 100 이상"
            };
            public override string label => "연료";
        } FuelItem fuel = new FuelItem();
        class ReqSkill : RequirementSkill {
            public override bool[] constraints => new bool[2] {
                skill.GetType().IsAssignableFrom(typeof(Cooking)),
                skill.lv > 0
            };
            public override string[] constraintsInfo => new string[2] {
                "요리 스킬",
                "1레벨 이상"
            };
            public override string label => "필요 스킬";
        } ReqSkill skill = new ReqSkill();
        class ReqTool : RequirementTool {
            public override Item tool { get; set; }
            public override string label => "조리 도구";
            public override Func<Item, bool>[] constItems => new Func<Item, bool>[2] {
                tool => tool.durability.value > 0,
                tool => tool.HasProperty(Property.Tool)
            };

            public override string[] constraintsInfo => new string[2] {
                "내구도 0 초과",
                "도구"
            };
        } ReqTool tool = new ReqTool();


        public void SetInstance(Item mainItem, Item fuelItem, Item tool) {
            main.item = mainItem;
            fuel.item = fuelItem;
            this.tool.tool = tool;
        }

        protected override void _RunRecipe() {
            Item resItem = main.item.Clone();

            resItem.name = resItem.coreName + " 구이";
            float ratio = resItem.durability.ratio;
            resItem.durability.maxValue -= 10;
            resItem.durability.ratio = ratio;
            resItem.calorie.value *= 2.5f;
            resItem.recipeCount.value -= 1;
            resItem.properties.Remove(Property.RawFood);
            Player.AddItem(resItem);

            fuel.item.combustibility.value -= 100;

            skill.skill.AddExp(15f);

            tool.tool.durability.value -= 2f; //TODO 2f - (reqSkill.skill.lv/reqSkill.skill.maxLv);

            //* Reset
            main.item = null;
            fuel.item = null;
            tool.tool = null;
            skill.skill = null;
        }
    }

    public sealed class Sewing : ARecipe {
        private Sewing() {}
        static Sewing _inst;
        public static Sewing Instance => (_inst ??= new Sewing());

        public override string label => "바느질";

        public override RequirementItem[] reqItems => new RequirementItem[1] { fiber };
        public override RequirementTool[] reqTools => new RequirementTool[1] { needle };
        public override RequirementSkill[] reqSkill => new RequirementSkill[1] { skill };

        class Fiber : RequirementItem {
            public override string label => "섬유";
            public override string[] constraintsInfo => new string[3] {
                "내구도 80% 이상",
                "섬유 조각",
                "5개"
            };
            public override Func<Item, bool>[] constItems => new Func<Item, bool>[3] {
                item => item.durability.ratio >= 0.8f,
                item => item.HasProperty(Property.Fiber),
                item => count == maxCount
            };

            public override Item item {
                get => _items[count-1];
                set { if (count != maxCount) _items[count++] = value; }
            }
            public override int maxCount => 5;
        } Fiber fiber = new Fiber();

        class Needle : RequirementTool {
            public override string label => "바늘";
            public override string[] constraintsInfo => new string[2] {
                "망가지지 않음",
                "바늘"
            };
            public override Func<Item, bool>[] constItems => new Func<Item, bool>[2] {
                tool => tool.durability.value > 0,
                tool => tool.HasProperty(Property.Needle)
            };
        } Needle needle = new Needle();

        class ReqDex : RequirementSkill {
            public new Dexterity skill { get; set; }
            public override string label => "재봉 스킬";
            public override string[] constraintsInfo => new string[1] { "직조 기술 보유" };
            public override bool[] constraints => new bool[1] { skill.Acquired(Dexterity.Weave) };
        } ReqDex skill = new ReqDex();


        protected override void _RunRecipe() {
            Item resItem = fiber.items[0].Clone(), refItem = fiber.items[1];

            resItem.name = resItem.coreName + " 직물";
            resItem.description = $"{resItem.coreName}? 가공하여 직물로 짠 것"; //TODO <- string.AddParticle() 구현 후 수정
            resItem.durability.maxValue = fiber.items.Average(i => i.durability.maxValue);
            resItem.durability.ratio = fiber.items.Average(i => i.durability.ratio);
            resItem.recipeCount.value = Mathf.RoundToInt((float)fiber.items.Average(i => i.recipeCount.value)) + 1;
            resItem.combustibility.value = fiber.items.Average(i => i.combustibility.value);

            resItem.RemoveProperty(Property.Fiber);
            resItem.AddProperty(Property.Fabric);

            Player.AddItem(resItem);
            needle.tool.durability.value -= 2;
            skill.skill.AddExp(10);
            fiber.count = 0;
        }
    }
}

namespace Rair.Items.Recipe.Construct {
    public sealed class CRecipe {
        public string label, description;
        public RequirementItem[] reqItems;
        public RequirementTool[] reqTools;
        public RequirementSkill[] reqSkill;
        public RequirementInst installation;
        public Action<CRecipe> _runRecipe = rcp => Debug.LogWarning($"{rcp.label} 레시피의 동작이 정의되지 않았습니다.");

        /// <summary>최종 사용 가능 여부를 확인 후 레시피를 실제로 사용합니다.</summary>
        /// <returns>레시피 실행 여부를 반환합니다.</returns>
        public bool RunRecipe() {
            if (_Verify() && canRunRecipe) {
                _runRecipe(this);
                _ResetRecipe();
                return true;
            }
            return false;
        }
        void _ResetRecipe() {
            foreach (var r in reqItems) r.items = null;
        }
        bool _Verify() {
            if (reqItems is not null) {
                if (reqItems.All(i => i.conditions.Length == i.conditionInfo.Length)) {
                    if (reqItems.All(i => i.minCount <= i.maxCount)) {
                        if (reqTools is not null && reqTools.All(t => t.conditions.Length != t.conditionInfo.Length)) Debug.LogWarning("Tool 항목의 조건 개수가 일치하지 않습니다.");
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
        public bool canRunRecipe {
            get {
                return reqItems.All(r => r.satisfied)
                && (reqTools is null || reqTools.All(r => r.satisfied))
                && (reqSkill is null || reqSkill.All(r => r.satisfied))
                && (installation is null/* || installation.satisfied*/)
                ;
            }
        }
    }

    public sealed class RequirementItem {
        public bool satisfied => (realItems.Count() >= minCount) && (items.All(item => item is null || conditions.All(F => F(item))));
        public Func<Item, bool>[] conditions { get; init; }
        public string[] conditionInfo { get; init; }
        public string label { get; init; }

        public Item[] items;
        public Item[] realItems => items.Where(item => item is not null).ToArray();
        /// <summary>아이템의 최대 수용량을 나타냅니다. 기본값은 1입니다.</summary>
        public int maxCount = 1;
        /// <summary>아이템의 최소 요구량을 나타냅니다. 기본값은 1입니다.</summary>
        public int minCount = 1;
        /// <summary>해당 아이템이 요구조건을 만족하는지 검사합니다.</summary>
        public bool Check(Item item) => conditions.All(F => F(item));
    }
    public sealed class RequirementTool {
        public bool satisfied => item is not null && conditions.All(F => F(item));
        public Func<Item, bool>[] conditions { get; init; }
        public string[] conditionInfo { get; init; }
        public string label { get; init; }

        public Item item;
        public bool Check(Item item) => conditions.All(F => F(item));
    }
    public sealed class RequirementSkill {
        public bool satisfied => skill is not null && skill.GetType().Name == skillType && conditions.All(F => F(skill));
        public Func<Skill, bool>[] conditions { get; init; }
        public string[] conditionInfo { get; init; }
        public string skillType { get; init; }

        public Skill skill;
    }
    public sealed class RequirementInst {
        public bool satisfied => conditions.All(F => F());
        public Func<bool>[] conditions { get; init; }
    }

    public static class Recipes {
        //* Sample
        public static CRecipe Grind = new CRecipe() {
            label = "빻기",
            description = "재료를 잘게 빻아 가루를 냅니다.",
            reqItems = new RequirementItem[] { //? 1슬롯 / 1~10개 / 식재료
                new RequirementItem() {
                    maxCount = 10,
                    conditions = new Func<Item, bool>[] {
                        item => item.durability.value > 0,
                        item => item.recipeCount.value > 0,
                        item => item.HasProperty(Property.RawFood)
                    },
                    conditionInfo = new string[] {
                        "파괴되지 않음",
                        "추가 작업 가능",
                        "식재료"
                    },
                    label = "식재료"
                }
            },
            reqTools = new RequirementTool[] { //? 2슬롯 / 절구+절굿공이 / 내구도 > 0
                new RequirementTool() {
                    conditions = new Func<Item, bool>[] {
                        item => item.durability.value > 0,
                        item => item.HasProperty(Property.Mortar)
                    },
                    conditionInfo = new string[] {
                        "파괴되지 않음",
                        "절구(막자사발)"
                    },
                    label = "절구"
                },
                new RequirementTool() {
                    conditions = new Func<Item, bool>[] {
                        item => item.durability.value > 0,
                        item => item.HasProperty(Property.Pestle)
                    },
                    conditionInfo = new string[] {
                        "파괴되지 않음",
                        "절굿공이(막자)"
                    },
                    label = "절굿공이"
                }
            },
            reqSkill = new RequirementSkill[] { //? 요리 / 제분 기술 보유
                new RequirementSkill() {
                    conditions = new Func<Skill, bool>[] { skill => skill.Acquired(Cooking.Grind) },
                    conditionInfo = new string[] { "제분 기술 보유" },
                    skillType = nameof(Cooking)
                }
            },
            _runRecipe = rcp => {
                var items = rcp.reqItems.Single().realItems;
                Item item = items.Random();

                item.durability.maxValue *= 1.2f;
                item.durability.ratio = items.Average(i => i.durability.ratio);
                item.RemoveProperty(Property.RawFood);
                item.AddProperty(Property.Flour);

                foreach (var tool in rcp.reqTools) tool.item.durability.value -= 1;
                for (var _=0; _<items.Length; _++) Player.AddItem(item); //TODO 검증 과정 구현(인벤토리 크기 or bool 반환)
                rcp.reqSkill.Single().skill.AddExp(1f); 
            }
        };
    
        public static CRecipe Knead = new CRecipe() {
            label = "반죽",
            description = "재료를 액체와 섞어 반죽 형태로 만듭니다.",
            reqItems = new RequirementItem[] { //? 2슬롯 / 가루(5개) + 액체(??)
                new RequirementItem() {
                    minCount = 5, maxCount = 5,
                    conditions = new Func<Item, bool>[] {
                        item => item.durability.value > 0,
                        item => item.HasProperty(Property.Powder),
                    },
                    conditionInfo = new string[] {
                        "파괴되지 않음",
                        "가루 형태"
                    },
                    label = "가루"
                },
                new RequirementItem() {
                    maxCount = 1,
                    conditions = new Func<Item, bool>[] {
                        item => item.durability.value > 0,
                        item => item.HasProperty(Property.Liquid)
                    },
                    conditionInfo = new string[] {
                        "파괴되지 않음",
                        "액체"
                    },
                    label = "액체"
                }
            },
            reqSkill = new RequirementSkill[] { //? 요리 / 반죽 기술 보유
                new RequirementSkill() {
                    conditions = new Func<Skill, bool>[] { skill => skill.Acquired(Cooking.Knead) },
                    conditionInfo = new string[] { "반죽 기술 보유" },
                    skillType = nameof(Cooking)
                }
            },
            _runRecipe = rcp => {
                Item[] item1 = rcp.reqItems[0].items, item2 = rcp.reqItems[1].items;
                Item item = item1.Random();

                item.durability.maxValue = item1.Average(i => i.durability.value) * 0.7f;
                item.durability.ratio = 1;
                item.RemoveProperty(Property.Powder, Property.Dust, Property.Flour);
                if (item1.All(i => i.HasProperty(Property.Edible)) && item2.All(i => i.HasProperty(Property.Edible))) item.AddProperty(Property.Edible);
                else item.RemoveProperty(Property.Edible);

                Player.AddItem(item);
                rcp.reqSkill.Single().skill.AddExp(2f);
            }
        };

        public static CRecipe Weave = new CRecipe() {
            label = "바느질",
            description = "섬유 조각들을 천으로 재단합니다.",
            reqItems = new RequirementItem[] { //? 2슬롯 / 섬유 조각(5개) / 실(1개)
                new RequirementItem() {
                    label = "섬유",
                    minCount = 5, maxCount = 5,
                    conditions = new Func<Item, bool>[] {
                        item => item.durability.ratio >= 0.8f,
                        item => item.HasProperty(Property.Fiber)
                    },
                    conditionInfo = new string[] {
                        "내구도 80% 이상",
                        "섬유 조각"
                    }
                },
                new RequirementItem() {
                    label = "실",
                    conditions = new Func<Item, bool>[] {
                        item => item.HasProperty(Property.Thread),
                        item => item.durability.value >= 1f
                    },
                    conditionInfo = new string[] {
                        "실",
                        "내구도 1 이상"
                    }
                }
            },
            reqTools = new RequirementTool[] { //? 1슬롯 / 바늘
                new RequirementTool {
                    label = "바늘",
                    conditions = new Func<Item, bool>[] {
                        item => item.HasProperty(Property.Needle),
                        item => item.durability.value > 0
                    },
                    conditionInfo = new string[] {
                        "바늘",
                        "파괴되지 않음"
                    }
                }
            },
            reqSkill = new RequirementSkill[] { //? 손재주 / 바느질 보유
                new RequirementSkill {
                    skillType = nameof(Dexterity),
                    conditions = new Func<Skill, bool>[] { skill => skill.Acquired(Dexterity.Weave) },
                    conditionInfo = new string[] { "바느질 기술 보유" }
                }
            },
            _runRecipe = rcp => {
                Item result = rcp.reqItems[0].items.Random().Clone();
                Item[] refItems = rcp.reqItems[0].items;
    
                result.name = result.coreName + " 직물";
                result.description = $"{result.coreName}? 가공하여 직물로 짠 것"; //TODO <- string.AddParticle() 구현 후 수정
                result.durability.maxValue = refItems.Average(i => i.durability.maxValue);
                result.durability.ratio = refItems.Average(i => i.durability.ratio);
                result.recipeCount.value = Mathf.RoundToInt((float)refItems.Average(i => i.recipeCount.value)) + 1;
                result.combustibility.value = refItems.Average(i => i.combustibility.value);
    
                result.RemoveProperty(Property.Fiber);
                result.AddProperty(Property.Fabric);
    
                Player.AddItem(result);
                rcp.reqTools[0].item.durability.value -= 2;
                rcp.reqSkill[0].skill.AddExp(10);
            }
        };
    }


    public static class LinqMethods {
        public static T Random<T>(this IEnumerable<T> enumerable) where T : class =>
            (enumerable.Count() < 1)
            ? null
            : enumerable.ElementAt(UnityEngine.Random.Range(0, enumerable.Count()-1))
        ;
    }
}



namespace System.Runtime.CompilerServices { internal static class IsExternalInit {} } //? init 이니셜라이저 사용을 위해