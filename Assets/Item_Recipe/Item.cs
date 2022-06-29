using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rair.Items.Attributes;
using Rair.Items.Properties;

namespace Rair.Items {
    public class Item
    {
        internal string coreName;
        public string name, description;
        public Sprite sprite;

        #region Attributes + Properties
        public ADurability durability = new();
        public ARecipeCount recipeCount = new();


        public ACalorie calorie = new();
        public ACombustibility combustibility = new();

        public List<Property> properties = new();
        public bool HasProperty(Property p) => properties.Contains(p);
        public Property GetProperty(Property p) => properties.Find(x => x == p);
        public void AddProperty(Property p) { if (!HasProperty(p)) properties.Add(p); }
        public bool RemoveProperty(Property p) => properties.Remove(p);
        public void RemoveProperty(params Property[] ps) { foreach(var p in ps) properties.Remove(p); }
        public IEnumerable<Property> ORMergeProperty(IEnumerable<Property> other) {
            var current = properties.ConvertAll(p => p);
            foreach (var prop in other) if (!current.Contains(prop)) current.Add(prop);
            return current;
        }
        public IEnumerable<Property> ANDMergeProperty(IEnumerable<Property> other) {
            var current = new List<Property>();
            foreach(var prop in other) if (properties.Contains(prop)) current.Add(prop);
            return current;
        }
        #endregion

        /// <summary>새 아이템을 생성할 때만 사용합니다. 입력한 속성에 맞춰 특성이 부여됩니다.</summary>
        public Item(string name, string description, float durability, int recipeCount, string coreName = "", float calorie = 0, float combustibility = 0, params Property[] properties) {
            this.name = name;
            this.coreName = (coreName.Length == 0) ? name : coreName;
            this.description = description;
            this.durability.MaxValue = durability;
            this.durability.Value = durability;
            this.recipeCount.Value = recipeCount;

            //* Sub Attributes + Related Property Tagger
            if (calorie != 0) {
                this.calorie.Value = calorie;
                if (calorie > 1f) AddProperty(Property.Cooking.RawFood.Clone());
            }
            if (combustibility != 0) {
                this.combustibility.Value = combustibility;
                if (combustibility >= 10) AddProperty(Property.Cooking.Fuel.Clone());
            }

            //* Independent Properties
            foreach (var p in properties) AddProperty(p);
        }
        /// <summary>아이템을 복제할 때만 사용합니다.</summary>
        private Item(Item original) {
            name = original.name;
            coreName = original.coreName;
            description = original.description;
            durability     = (ADurability)original.durability.Clone();
            recipeCount    = (ARecipeCount)original.recipeCount.Clone();
            calorie        = (ACalorie)original.calorie.Clone();
            combustibility = (ACombustibility)original.combustibility.Clone();
            //TODO 속성 추가 시 반영해야 합니다.

            properties = new(original.properties);
        }

        public Item Clone() => new(this);

        //! 아이템의 레퍼런스 일치가 아닌 상태 비교는 특성/속성 조건 일치로 수행해야 합니다.
    }


    namespace Attributes {
        /// <summary>정수 값을 가지는 속성 클래스입니다. 복합적인 연산이 요구되지 않는 속성에 사용됩니다.</summary>
        public abstract class AttributeInt {
            /// <summary>이 속성 데이터가 정보 UI에 표시되는지를 나타냅니다. 속성 값이나 특성에 따라 변화할 수 있습니다.</summary>
            public virtual bool Exposed => true;
            protected int _value;
            /// <summary>정수형 속성의 값을 나타냅니다.</summary>
            public virtual int Value {
                get => _value;
                set { _value = value; OnValueChanged(); }
            }
            /// <summary>속성 값이 변경될 때 호출되는 콜백 함수입니다.</summary>
            public virtual Action OnValueChanged { get; set; }

            public AttributeInt() { OnValueChanged = () => { }; }

            public AttributeInt Clone() => (AttributeInt)MemberwiseClone();
        }
        /// <summary>실수 값을 가지는 속성 클래스입니다.</summary>
        public abstract class AttributeFloat {
            /// <summary>이 속성 데이터가 정보 UI에 표시되는지를 나타냅니다. 속성 값이나 특성에 따라 변화할 수 있습니다.</summary>
            public virtual bool Exposed => true;
            protected float _value;
            /// <summary>실수형 속성의 값을 나타냅니다.</summary>
            public virtual float Value {
                get => _value;
                set { _value = value; OnValueChanged(); }
            }
            /// <summary>속성 값이 변경될 때 호출되는 콜백 함수입니다.</summary>
            public virtual Action OnValueChanged { get; set; }

            public AttributeFloat() { OnValueChanged = () => { }; }

            public AttributeFloat Clone() => (AttributeFloat)MemberwiseClone();
        }


        public class ADurability : AttributeFloat {
            public override float Value {
                get => _value;
                set { _value = Mathf.Clamp(value, 0, MaxValue); OnValueChanged(); }
            }
            public float MaxValue { get; set; }
            public float Ratio { get => Value/MaxValue; set => this.Value = MaxValue * value; }
        }
        public class ARecipeCount : AttributeInt {}
        public class ACalorie : AttributeFloat {
            public override bool Exposed => Value > 0.01f; // 해당 수치 이상을 유효한 열량 데이터로 간주합니다.
        }
        public class ACombustibility : AttributeFloat { // 가연성
            public override bool Exposed => Value >= 1;
        }
    }

    namespace Properties {
        public class Property {
            public string name;
            public string[] alias;
            int _lv = 1;
            public int Level {
                get => _lv;
                set => _lv = Mathf.Clamp(value, 1, MaxLevel);
            }
            public readonly int MaxLevel;
            internal Property(string name, int maxLevel = 1, params string[] alias) {
                if (alias.Length != 0 && alias.Length != maxLevel) Debug.LogWarning($"{name} 특성의 별칭을 정확히 {maxLevel}개로 설정해야 합니다.");
                this.name = name;
                this.MaxLevel = maxLevel;
                this.alias = alias;
            }

            public override string ToString() {
                if (isStatic) Debug.LogWarning("정적 개체를 직접 사용하고 있습니다. 레퍼런스 분리를 위해 Clone()를 사용하세요.");
                return $"{name}{(MaxLevel > 1 ? $" Lv.{Level}" : "")}{(alias.Length > 0 ? $"({alias[Level-1]})" : "")}";
                //? MaxLv : 1 + alias X = MFG
                //? MaxLv : 2 + alias X = MFG Lv.2
                //? MaxLv : 2 + alias O = MFG Lv.2(Broken)
            }
            public override bool Equals(object obj) => obj is Property p && p.name == name; // 이름만 비교합니다.
            public static bool operator ==(Property A, Property B) => A.Equals(B);
            public static bool operator !=(Property A, Property B) => !A.Equals(B);
            public override int GetHashCode() => name.GetHashCode();
            //TODO : 특성을 추가할 때 레퍼런스를 분리하는 방법을 찾거나, Level을 개별 적용하는 좋은 방법을 고민해봅시다.
            public bool isStatic = true;
            public Property Clone() {
                var P = (Property)MemberwiseClone();
                P.Level = 1; P.isStatic = false;
                return P;
            }
            public Property Clone(int level) {
                var P = (Property)MemberwiseClone();
                P.Level = level; P.isStatic = false;
                return P;
            }
            public static class Cooking {
                public readonly static Property Liquid = new("액체"),
                    RawFood = new("날것"),
                    Edible = new("음식"),
                    //* 식재료 기본 특성
                    Ingredient = new("식재료"),
                    Powder = new("가루"), Dust = new("먼지"), Flour = new("식용 가루"),
                    Thread = new("실"), Fiber = new("섬유"), Fabric = new("직물"),
                    //* 식재료 관련 특성
                    Rotten = new("부패", 3), Cooked = new("익힘", 5, "설익음", "레어", "미디엄", "웰던", "타버림"),
                    Fuel = new("연료"),
                    SewingTool = new("제봉도구"),
                    Needle = new("바늘"),
                    Mortar = new("절구"), Pestle = new("절굿공이")
                ;
            }
            public static class TierProps {
                const int Tier = 20; // 제작 시 플레이어의 관련 스킬 티어 레벨을 따라감.
                public readonly static Property Tool = new("도구", Tier)
                ;

            }
        }
    }
}