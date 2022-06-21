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
        public ADurability durability = new ADurability();
        public ARecipeCount recipeCount = new ARecipeCount();


        public ACalorie calorie = new ACalorie();
        public ACombustibility combustibility = new ACombustibility();

        public List<Property> properties = new List<Property>();
        public bool HasProperty(Property p) => properties.Any(q => q == p);
        public void AddProperty(Property p) { if (!HasProperty(p)) properties.Add(p); }
        public bool RemoveProperty(Property p) => properties.Remove(p);
        public void RemoveProperty(params Property[] ps) { foreach(var p in ps) properties.Remove(p); }
        public IEnumerable<Property> ORMergeProperty(IEnumerable<Property> other) {
            var current = properties.ConvertAll(p => p);
            foreach (var prop in other) if (!current.Any(p => p == prop)) current.Add(prop);
            return current;
        }
        public IEnumerable<Property> ANDMergeProperty(IEnumerable<Property> other) {
            var current = new List<Property>();
            foreach(var prop in other) if (properties.Any(p => p == prop)) current.Add(prop);
            return current;
        }
        #endregion

        /// <summary>새 아이템을 생성할 때만 사용합니다. 입력한 속성에 맞춰 특성이 부여됩니다.</summary>
        public Item(string name, string description, float durability, int recipeCount, string coreName = "", float calorie = 0, float combustibility = 0, params Property[] properties) {
            this.name = name;
            this.coreName = (coreName.Length == 0) ? name : coreName;
            this.description = description;
            this.durability.maxValue = durability;
            this.durability.value = durability;
            this.recipeCount.value = recipeCount;

            //* Sub Attributes + Related Property Tagger
            if (calorie != 0) {
                this.calorie.value = calorie;
                if (calorie > 1f) AddProperty(Property.RawFood);
            }
            if (combustibility != 0) {
                this.combustibility.value = combustibility;
                if (combustibility >= 10) AddProperty(Property.Fuel);
            }

            //* Independent Properties
            foreach (var p in properties) AddProperty(p);
        }


        public Item Clone() => (Item)this.MemberwiseClone();

        //? 아이템은 항상 객체 상태에서만 동일 체크를 수행하도록 합니다. 이 경우 레퍼런스가 같은 경우만 허용합니다.
        /* default implementation
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
        public static bool operator ==(Item a, Item b) => a.Equals(b);
        public static bool operator !=(Item a, Item b) => !a.Equals(b);
        */
        //! 아이템의 정적 상태 비교는 Equals, == 연산자가 아닌 특성/속성 조건 일치로만 수행해야 합니다.
    }


    namespace Attributes {
        /// <summary>정수 값을 가지는 속성 인터페이스입니다. 복합적인 연산이 요구되지 않는 속성에 사용됩니다.</summary>
        public interface IAttributeInt {
            bool revealed { get; }
            int value { get; }
        }
        /// <summary>실수 값을 가지는 속성 인터페이스입니다. </summary>
        public interface IAttributeFloat {
            bool revealed { get; }
            float value { get; }
        }


        public class ADurability : IAttributeFloat {
            public bool revealed => true; //? 이 속성은 항상 표시됩니다.
            
            float _v;
            public float value { get => _v; set => _v = Mathf.Clamp(value, 0, maxValue); }
            public float maxValue { get; set; }

            public float ratio { get => value/maxValue; set => this.value = maxValue * value; }
        }
        public class ARecipeCount : IAttributeInt {
            public bool revealed => true; //? 이 속성도 항상 표시됩니다.
            public int value { get; set; }
            public float fValue { set => this.value = (int)value; }
        }
        public class ACalorie : IAttributeFloat {
            public bool revealed => value > 0.01f; // 해당 수치 이상을 유효한 칼로리 데이터로 간주합니다.
            public float value { get; set; }
        }
        // 가연성
        public class ACombustibility : IAttributeFloat {
            public bool revealed => false;
            public float value { get; set; }
        }
    }

    namespace Properties {
        public enum Property {

            Liquid, // 액체

            RawFood, // 날것
            Edible, // 식용
            
            Powder, // 식용 구분 없음
            Dust, // 식용 불가능
            Flour, // 식용 가능
            
            
            Thread, Fiber, Fabric, // 실, 섬유, 직물
            Fuel,
            

            Tool, // <- 일반 도구
            SewingTool, // <- 재봉 도구


            Needle, //? "바늘 제작" 레시피 결과물, 또는 특정 희귀 채집물만 선천적으로 보유하는 특성
            Mortar, //? 절구. 절구와 절굿공이 특성은 동일한 아이템에 동시에 부여될 수 있음.
            Pestle, //? 절굿공이. 절구와 절굿공이 특성은 동일한 아이템에 동시에 부여될 수 있음.
        }


        /*
        public sealed class Property {
            static int _idx;
            int idx;
            Property() { idx = _idx++; }
            public static Property RawFood = new Property(),
                Fuel = new Property()
            ;


            public override int GetHashCode() => idx.GetHashCode();
            public override bool Equals(object obj) => obj.GetType() == typeof(Property) && (Property)obj == this;
            public static bool operator ==(Property a, Property b) => a.idx == b.idx;
            public static bool operator !=(Property a, Property b) => a.idx != b.idx;
        }*/
    }
}