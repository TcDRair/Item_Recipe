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
            text.text = skill.name +"\nLv : " + skill.lv + " / " + skill.maxLv + "\nexp : " + skill.exp + " / " + skill.maxExp;
        }
        void _Refresh(Skill skill1, Skill skill2) {
            text.text = skill1.name +"\nLv : " + skill1.lv + " / " + skill1.maxLv + "\nexp : " + skill1.exp + " / " + skill1.maxExp + "\n" + skill2.name + "\nLv : " + skill2.lv + " / " + skill2.maxLv + "\nexp : " + skill2.exp + " / " + skill2.maxExp;
        }
    }
}