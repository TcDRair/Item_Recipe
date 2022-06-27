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
        Item _asItem;
        public static Item AsItem {
            get {
                Instance._asItem ??= new Item("_Installation_연료", "아이템 형식으로 전달되는 설치물의 정보입니다.", float.PositiveInfinity, 0);
                Instance._asItem.combustibility.Value = Instance._fuel;
                return Instance._asItem;
            }
        }

        public Text fuel;
        float _fuel = 0;
        const string prev = "잔여 연료 : ";

        void Start() {
            fuel.text = prev + _fuel.ToString("F0");
        }

        public void MaterialSupply() {
            Item item = Player.FindAndRemoveItem(item => item.HasProperty(Property.Fuel));
            if (item != null) {
                _fuel += item.combustibility.Value;
                fuel.text = prev + _fuel.ToString("F0");
                SampleLogger.AddLog($"연료 {item.combustibility.Value:F0)} 추가");
                Player.Instance.dexerity.AddExp(0.5f);
            }
            else SampleLogger.AddLog("인벤토리에 연료 아이템 없음");
        }

        public void ApplyChange() {
            _fuel = _asItem.combustibility.Value;
            UpdateFuel();
        }

        void UpdateFuel() { fuel.text = prev + _fuel.ToString("F0"); }
    }
}