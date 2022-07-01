using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Rair.Mono {
    public class SampleLogger : MonoBehaviour
    {
        public static SampleLogger Instance { get; private set; }

        public Text logUI;

        public void Awake() { Instance = this; }

        public void Start() { logUI.text = ""; }

        public static void AddLog(string newLog) => Instance.UpdateLog(newLog);
        public static void GotItem(string item) => Instance.UpdateLog($"{item} 이(가) 추가됨");
        public static void LostItem(string item) => Instance.UpdateLog($"{item} 이(가) 제거됨");

        readonly StringBuilder str = new();
        void UpdateLog(string newLog) {
            if (!newLog.EndsWith('\n')) newLog += '\n';

            str.Insert(0, newLog);
            if (str.Length > 500) str.Remove(400, str.Length - 400);
            
            logUI.text = str.ToString();
        }
    }
}