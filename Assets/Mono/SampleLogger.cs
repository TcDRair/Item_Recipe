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

        void Update() {
            if (Input.GetKeyDown(KeyCode.Alpha1) && Player.Instance.items.Count >= 1) Debug.Log(string.Join(", ", Player.Instance.items[0].tags));
            if (Input.GetKeyDown(KeyCode.Alpha2) && Player.Instance.items.Count >= 2) Debug.Log(string.Join(", ", Player.Instance.items[1].tags));
            if (Input.GetKeyDown(KeyCode.Alpha3) && Player.Instance.items.Count >= 3) Debug.Log(string.Join(", ", Player.Instance.items[2].tags));
            if (Input.GetKeyDown(KeyCode.Alpha4) && Player.Instance.items.Count >= 4) Debug.Log(string.Join(", ", Player.Instance.items[3].tags));
            if (Input.GetKeyDown(KeyCode.Alpha5) && Player.Instance.items.Count >= 5) Debug.Log(string.Join(", ", Player.Instance.items[4].tags));
            if (Input.GetKeyDown(KeyCode.Alpha6) && Player.Instance.items.Count >= 6) Debug.Log(string.Join(", ", Player.Instance.items[5].tags));
            if (Input.GetKeyDown(KeyCode.Alpha7) && Player.Instance.items.Count >= 7) Debug.Log(string.Join(", ", Player.Instance.items[6].tags));
            if (Input.GetKeyDown(KeyCode.Alpha8) && Player.Instance.items.Count >= 8) Debug.Log(string.Join(", ", Player.Instance.items[7].tags));
        }

        public static void AddLog(string newLog) => Instance.UpdateLog(newLog);
        public static void GotItem(string item) => Instance.UpdateLog($"{item} 이(가) 추가됨");
        void UpdateLog(string newLog) {
            if (!newLog.EndsWith('\n')) newLog += '\n';

            if (logs.Count == maxLog) logs.Dequeue();
            logs.Enqueue(newLog);

            string str = "";
            foreach (var log in logs) str += log;
            
            logUI.text = str;
        }
    }
}