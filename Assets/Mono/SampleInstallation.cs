using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Rair.Items;
using Rair.Items.Properties;
namespace Rair.Samples {
    public class SampleInstallation : MonoBehaviour
    {
        public static SampleInstallation Instance;
        void Awake() { Instance = this; }

        /// <summary>이 설치물을 아이템화하여 전달합니다. 연료 데이터 수송신만을 목적으로 합니다.</summary>
        public Item AsItem = new("_Installation_Fuel", "연료 데이터 전송을 위한 아이템 인스턴스입니다.", float.MaxValue, 0) {
            combustibility = new() { OnValueChanged = () => { Instance._fuel = Instance.AsItem.combustibility.Value; Instance.UpdateFuel(); } }
        };

        public Text fuel;
        float _fuel = 0;
        const string prev = "잔여 연료 : ";

        void Start() { fuel.text = $"{prev}{_fuel:F0}"; }

        public void MaterialSupply() {
            Item item = Player.FindAndRemoveItem(item => item.HasProperty(Property.Cooking.Fuel.Clone()));
            if (item != null) {
                _fuel += item.combustibility.Value;
                AsItem.combustibility.Value = _fuel;
                fuel.text = $"{prev}{_fuel:F0}";
                SampleLogger.AddLog($"연료 {item.combustibility.Value:F0} 추가");
                Player.Instance.dexerity.AddExp(0.5f);
            }
            else SampleLogger.AddLog("인벤토리에 연료 아이템 없음");
        }

        void UpdateFuel() { fuel.text = prev + _fuel.ToString("F0"); }
    }
}