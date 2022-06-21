using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using Rair.Items;
using Rair.Items.Properties;
namespace Rair.Samples {
    public class SampleInteractor : MonoBehaviour
    {
        public Image progressMask;
        public Text buttonLabel;
        
        [Range(0, 10)]
        public float time = 1f, cooldown = 0f;
        public string label;
        
        public enum Type {
            tree, bladderwort
        } public Type type;
        Item stick, minnow;

        void Awake() {
            stick = new Item("나뭇가지", "도구 제작에 쓰거나 연료로 사용할 수 있는 작은 나뭇가지.", 10f, 2, combustibility: 30f);
            minnow = new Item("송사리", "민물에서 구할 수 있는 신선한 식자재", 50f, 3, calorie: 25f);
        }
        void Start() {
            //* 내부적으로 Fuel 태그가 붙습니다.
            buttonLabel.text = label;

            progressMask.fillAmount = 0;
            _prg1 = 0; _prg2 = cooldown;
        }

        public void Interaction() {
            if (!active) {
                StartCoroutine(Timer());
            }
        }

        bool active = false;
        float _prg1, _prg2;
        IEnumerator Timer() {
            active = true;
            while ((_prg1 += Time.deltaTime) < time) {
                progressMask.fillAmount = _prg1/time;
                yield return null;
            }

            switch (type) {
                case Type.tree : Player.AddItem(stick.Clone()); break;
                case Type.bladderwort : Player.AddItem(minnow.Clone()); break;

                default : Debug.Log($"{type.ToString()} 개체의 동작을 지정해주세요."); break;
            }
            _prg1 = 0f;

            yield return StartCoroutine(CooldownTimer());
        }

        IEnumerator CooldownTimer() {
            while ((_prg2 -= Time.deltaTime) > 0) {
                progressMask.fillAmount = _prg2/cooldown;
                yield return null;
            }

            progressMask.fillAmount = 0;
            _prg2 = cooldown;
            active = false;
        }
    }
}
