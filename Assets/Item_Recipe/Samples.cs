using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rair.Items;

namespace Rair.Items {
    public partial class Item {
        public enum Type {
            세검, 광물
        } public Type type;

        static readonly string[] mats = new string[] { "청동", "납", "철", "은", "백금", "흑요석" };
        public static Item 무작위_광석 => new(
            mats.Random(),
            "_tbd_description",
            Type.광물,
            "",
            " 광석",
            Tag.단단함(
                UnityEngine.Random.value switch {
                    > 0.98f => 4, // 2%
                    > 0.73f => 3, // 25%
                    > 0.28f => 2, // 45%
                    _       => 1, // 28%
                }
            )
        );
        public static Item 광석 => new(
            "광석",
            "",
            Type.광물,
            ""
        );
    }

    public partial class Tag {
        public static Tag HP증가(int level) => new("HP 증가", level, 10, "레벨당 HP 500 증가");
        public static Tag HP증폭(int level) => new("HP 증폭", level, 10, "레벨당 HP 5% 증가");
        public static Tag 단단함(int level) => new("단단함", level, 10, "레벨당 방어력 2.5% 증가");
        public static Tag 예리함(int level) => new("예리함", level, 10, "레벨당 공격력 2.5% 증가");

        public static Tag 랜덤 => tags.Random()(0);
        static readonly Func<int, Tag>[] tags = new Func<int, Tag>[] {
            HP증가, HP증폭, 단단함, 예리함
        };
    }
}

namespace Rair.Rcp {
    public sealed partial class Recipe {
        public static Recipe Sample1 => new() {
            label = "조건 체크용 샘플", description = "_tbd_description",
            requirements = new RequirementItem[] {
                new() {
                    Label = "단단한 광석",
                    ConditionInfo = new string[] { "광물", "단단함 Lv.1 이상" },
                    Conditions = new Func<Item, bool>[] { i => i.type == Item.Type.광물, i => i.Has(Tag.단단함(1)) }
                }
            },
            Behavior = (rcp, player) => {
                var item = rcp.requirements[0].Items[0];
                (string prefix, Tag[] tags) = UnityEngine.Random.value switch {
                    > 0.75f => ("매우 단단한 ", new Tag[] { Tag.단단함(2) }),
                    > 0.50f => ("단단한 ", new Tag[] { Tag.단단함(1) }),
                    > 0.25f => ("예리한 ", new Tag[] { Tag.예리함(1) }),
                    _ => ("", null),
                };

                Item newItem = new(item.coreName, "_tbd_description", Item.Type.세검, prefix, " 세검", rcp.AllTags);
                if (tags is not null) newItem.AddTag(tags);

                player.AddItem(newItem);
            }
        };
    
        public static Recipe 또뭐가있지 => new() {
            //TODO
        };
    }
}