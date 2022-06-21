using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Rair.Items;
using Rair.Items.Recipe.Abstract;
namespace Rair.Samples {
    public class SampleRecipe : MonoBehaviour
    {
        public Text rcpLabel, rcpItem1, rcpItem2, rcpTool, rcpSkill;
        public Button runButton;
        public Image recipeProgress;
        RequirementItem item1, item2;
        RequirementTool tool;
        RequirementSkill skill;

        ARecipe recipe;


        // Start is called before the first frame update
        void Start() {
            recipe = Grill.Instance;
            recipeProgress.fillAmount = 0;

            SetRcp();
        }

        void Update() {
            runButton.interactable = recipe.canRunRecipe;
            if (_changed) {
                UpdateRcp();
                _changed = false;
            }
        }

        void SetRcp() {
            rcpLabel.text = recipe.label;
            item1 = recipe.reqItems[0]; rcpItem1.text = item1.fullText;
            item2 = recipe.reqItems[1]; rcpItem2.text = item2.fullText;
            tool  = recipe.reqTools[0]; rcpTool.text  = tool.fullText;
            skill = recipe.reqSkill[0]; rcpSkill.text = skill.fullText;
        }

        void UpdateRcp() {
            rcpItem1.text = (item1.item is null) ? item1.fullText : item1.fullTextWithCheck;
            rcpItem2.text = (item2.item is null) ? item2.fullText : item2.fullTextWithCheck;
            rcpTool.text = (tool.tool is null) ? tool.fullText : tool.fullTextWithCheck;
            rcpSkill.text = (skill.skill is null) ? skill.fullText : skill.fullTextWithCheck;
        }

        bool _changed = false;
        public void SupplyItem1() {
            if (item1.item != null) return;
            Item item = Player.FindAndRemoveItem(item => {
                item1.item = item;
                return item1.satisfied;
            });
            if (item is null) {
                item1.item = null;
                SampleLogger.AddLog("조건을 만족하는 아이템 없음");
            }
            else {
                item1.item = item;
                _changed = true;
            }
        }
        public void SupplyItem2() {
            if (item2.item != null) return;
            item2.item = SampleInstallation.AsItem;
            if (!item2.satisfied) {
                item2.item = null;
                SampleLogger.AddLog("연료량 조건 미달");
            }
            else {
                _changed = true;
            }
        }
        public void SupplyTool() {
            if (tool.tool != null) return;
            Item item = Player.FindItem(item => {
                tool.tool = item;
                return tool.satisfied;
            });
            if (item is null) {
                tool.tool = null;
                SampleLogger.AddLog("조건을 만족하는 아이템 없음");
            }
            else {
                tool.tool = item;
                _changed = true;
            }
        }
        public void SetSkill() {
            skill.skill = Player.Instance.cooking;
            _changed = true;
        }

        public void RunRcp() {
            if (recipe.canRunRecipe) StartCoroutine(_RunRcp());
        }
        float _prg = 0f; //TODO rcpMask 인스펙터에서 지정할 것
        IEnumerator _RunRcp() {
            while ((_prg += Time.deltaTime) < 5f) {
                recipeProgress.fillAmount = _prg/5f;
                yield return null;
            }
            recipe.RunRecipe();
            SampleInstallation.Instance.ApplyChange();
            UpdateRcp();
            Player.UpdateLog();
            _prg = 0;
            recipeProgress.fillAmount = 0f;
        }
    }
}