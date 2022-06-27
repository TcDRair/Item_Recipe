using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Rair.Items;
using Rair.Rcp;

namespace Rair.Samples {
    public class SampleRecipe : MonoBehaviour
    {
        public Text rcpLabel, rcpItem1, rcpItem2, rcpTools, rcpSkill;
        public Button runButton;
        public Image recipeProgress;
        RequirementItem item1, item2;
        RequirementTool tool;
        RequirementSkill skill;

        Recipe recipe;


        // Start is called before the first frame update
        void Start() {
            recipe = Recipes.Grill;
            recipeProgress.fillAmount = 0;

            UpdateRcp();
        }

        void Update() {
            runButton.interactable = recipe.CanRunRecipe;
            if (_changed) {
                UpdateRcp();
                _changed = false;
            }
        }

        void UpdateRcp() {
            rcpLabel.text = recipe.label;
            item1 = recipe.reqItems[0]; rcpItem1.text = item1.TextLog;
            item2 = recipe.reqItems[1]; rcpItem2.text = item2.TextLog;
            tool  = recipe.reqTools[0]; rcpTools.text =  tool.TextLog;
            skill = recipe.reqSkill[0]; rcpSkill.text = skill.TextLog;
        }

        bool _changed = false;
        public void SupplyItem1() {
            if (item1.Full) return;
            Item item = Player.FindAndRemoveItem(item1.Check);
            if (item is null) SampleLogger.AddLog("조건을 만족하는 아이템 없음");
            else {
                item1.items.Add(item);
                _changed = true;
            }
        }
        public void SupplyItem2() {
            if (item2.Full) return;
            var item = SampleInstallation.AsItem;
            if (item2.Check(item)) {
                _changed = true;
                item2.items.Add(item);
            }
            else SampleLogger.AddLog("연료량 조건 미달");
        }
        public void SupplyTool() {
            if (tool.item is not null) return;
            Item item = Player.FindItem(tool.Check);
            if (item is null) SampleLogger.AddLog("조건을 만족하는 아이템 없음");
            else {
                tool.item = item;
                _changed = true;
            }
        }
        public void SetSkill() {
            skill.skill = Player.Instance.cooking;
            _changed = true;
        }

        public void Run() {
            if (recipe.CanRunRecipe) StartCoroutine(RcpEnumerator());
        }
        float _prg = 0f; //TODO rcpMask 인스펙터에서 지정할 것
        IEnumerator RcpEnumerator() {
            while ((_prg += Time.deltaTime) < 5f) {
                recipeProgress.fillAmount = _prg/5f;
                yield return null;
            }
            recipe.Run();
            SampleInstallation.Instance.ApplyChange();
            UpdateRcp();
            Player.UpdateLog();
            _prg = 0;
            recipeProgress.fillAmount = 0f;
        }
    }
}