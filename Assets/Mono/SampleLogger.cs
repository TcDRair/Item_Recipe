using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Rair.Samples {
    public class SampleLogger : MonoBehaviour
    {
        public static SampleLogger Instance { get; private set; }

        public Text logUI;
        Queue logs;

        const int maxLog = 8;

        void Awake() {
            Instance = this;
        }

        void Start() {
            logs = new Queue(maxLog);
            logUI.text = "";
        }

        public static void AddLog(string newLog) => Instance._UpdateLog(newLog);
        public static void GotItem(string item) => Instance._UpdateLog($"{item} 이(가) 추가됨");
        void _UpdateLog(string newLog) {
            if (!newLog.EndsWith('\n')) newLog += '\n';

            if (logs.Count == maxLog) logs.Dequeue();
            logs.Enqueue(newLog);

            string str = "";
            foreach (var log in logs) str += log;
            
            logUI.text = str;
        }
    }
}