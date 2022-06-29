using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using Rair.Skills;
namespace Rair.Samples {
    public class SampleSkillUI : MonoBehaviour
    {
        static SampleSkillUI Instance;
        void Awake() { Instance = this; }

        public Text text;

        public static void Refresh(Skill skill) => Instance._Refresh(skill);
        public static void Refresh(Skill skill1, Skill skill2) => Instance._Refresh(skill1, skill2);
        void _Refresh(Skill skill) {
            text.text = skill.Name +"\nLv : " + skill.Lv + " / " + skill.MaxLv + "\nexp : " + skill.Exp + " / " + skill.MaxExp;
        }
        void _Refresh(Skill skill1, Skill skill2) {
            text.text = skill1.Name +"\nLv : " + skill1.Lv + " / " + skill1.MaxLv + "\nexp : " + skill1.Exp + " / " + skill1.MaxExp + "\n" + skill2.Name + "\nLv : " + skill2.Lv + " / " + skill2.MaxLv + "\nexp : " + skill2.Exp + " / " + skill2.MaxExp;
        }
    }
}