using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


using Rair.Items;
using Rair.Items.Attributes;
using Rair.Items.Properties;
using Rair.Items.Recipe.Construct;
using Rair.Skills;

#if UNITY_EDITOR
namespace Rair.Editor { // namespace Rair.Editor;
public class RecipeEditor : EditorWindow
{
    static EditorWindow window;
    [MenuItem("Window/레시피 에디터")]
    public static void ShowWindow() {
        window = GetWindow<RecipeEditor>("레시피 에디터");
        window.maxSize = new Vector2(1280,720);
        window.minSize = new Vector2(1280,720);
    }

    static Rect[] _mA, _sA;
    static Rect[] mainArea => (_mA ??= new Rect[] { SelectArea, ConditionArea, ResultArea, SaveArea });
    static Rect[] subArea  = (_sA ??= new Rect[] { ItemSelectArea, SkillSelectArea });


    static readonly Rect SelectArea = new Rect(10,10,300,700),
        ConditionArea = new Rect(320,10,630,700),
        ResultArea = new Rect(960,260,310,450),
        SaveArea = new Rect(960,10,310,240),
        ItemSelectArea = new Rect(14,14,292,484),
        SkillSelectArea = new Rect(14,502,292,204)
    ;
    static readonly Color mainColor = new Color(0.3f, 0.3f, 0.3f, 1f),
        subColor = new Color(0.25f, 0.25f, 0.25f, 1f),
        slotColor = new Color(0.6f, 0.5f, 0.07f, 1f),
        groupColor = new Color(0.2f, 0.2f, 0.2f, 1f)
    ;

    void OnEnable() {
        itemSelectRect  = new Rect(0,0,279,0);
        skillSelectRect = new Rect(0,0,279,0);

        itemSlots = new List<ItemSlot>() { new ItemSlot() };
        skillSlots = new List<SkillSlot>() { new SkillSlot() };
    }

    //? 실제 리스트가 들어있는 Rect입니다.
    Rect itemSelectRect, skillSelectRect;
    //? ScrollView에 사용되는 Vector입니다.
    Vector2 scrollISA, scrollSSA;

    //* Selection Variables
    List<ItemSlot> itemSlots;
    List<ItemSlot> toolSlots;
    List<SkillSlot> skillSlots;
    Slots current;

    public void OnGUI() {
        foreach(var rect in mainArea) EditorGUI.DrawRect(rect, mainColor);
        foreach(var rect in  subArea) EditorGUI.DrawRect(rect,  subColor);

        scrollISA = GUI.BeginScrollView(ItemSelectArea, scrollISA, itemSelectRect, false, false);
            int idx1 = 0;
            foreach(var slot in itemSlots.ToArray()) DrawItemSelector(idx1++, slot);
            if (itemSlots.Count < 10 && GUI.Button(new Rect(5, idx1*65 + 15, 240, 20), "+")) itemSlots.Add(new ItemSlot());

            itemSelectRect.height = idx1*65 + 60;
        GUI.EndScrollView();

        scrollSSA = GUI.BeginScrollView(SkillSelectArea, scrollSSA, skillSelectRect, false, false);
            int idx2 = 0;
            foreach(var slot in skillSlots.ToArray()) DrawSkillSelector(idx2++, slot);
            if (GUI.Button(new Rect(5, idx2*65 + 15, 240, 20), "+")) skillSlots.Add(new SkillSlot());

            skillSelectRect.height = idx2*65 + 60;
        GUI.EndScrollView();

        GUI.BeginGroup(ConditionArea);
            DrawConstraintsEditor(current);
        GUI.EndGroup();
    }

    #region Frame + Selector
    readonly Rect slotRect = new Rect(0, 10, 220, 60),
        editRect = new Rect(80, 40, 130, 20), editRect2 = new Rect(20, 40, 190, 20),
        removeRect = new Rect(220, 9, 20, 62),
        indexRect = new Rect(5, 30, 10, 20), labelRect = new Rect(80, 18, 40, 20), labelRect2 = new Rect(25, 18, 40, 20),
        spriteRect = new Rect(20, 15, 50, 50),
        textRect = new Rect(125, 18, 85, 20), enumRect = new Rect(70, 20, 140, 20)
    ;
    void DrawItemSelector(int index, ItemSlot slot) {
        RecT area = new RecT(5, index*65 + 5, 0, 0);
        EditorGUI.DrawRect(area + slotRect, slotColor);
        EditorGUI.LabelField(area + labelRect, "Item :");
        if (GUI.Button(area + removeRect, "-")) {
            if (itemSlots.Count > 1) { if (current == slot) current = null; itemSlots.Remove(slot); }
            else Debug.Log("레시피에는 최소 1개의 재료 아이템이 들어가야 합니다.");
        }
        EditorGUI.LabelField(area + indexRect, $"{index}");
        slot.sprite = (Sprite)EditorGUI.ObjectField(area + spriteRect, slot.sprite, typeof(Sprite), false);
        slot.label = EditorGUI.TextField(area + textRect, slot.label);
        if (GUI.Button(area + editRect, "조건 수정")) current = slot;
    }
    void DrawSkillSelector(int index, SkillSlot slot) {
        RecT area = new RecT(5, index*65 + 5, 0, 0);
        EditorGUI.DrawRect(area + slotRect, slotColor);
        EditorGUI.LabelField(area + labelRect2, "스킬 :");
        if (GUI.Button(area + removeRect, "-")) { if (current == slot) current = null; skillSlots.Remove(slot); }
        EditorGUI.LabelField(area + indexRect, $"{index}");
        slot.skill = (SkillEnum)EditorGUI.EnumPopup(area + enumRect, slot.skill);
        if (GUI.Button(area + editRect2, "조건 수정")) current = slot;
    }
    enum SkillEnum {
        손재주, 요리, //TODO 레시피 다 긁어와서 별도 처리?
    }
    #endregion


    #region Constraints Editor
    readonly Rect editLabelRect = new Rect(10, 10, 200, 20),
        editAreaRect = new Rect(10, 40, 610, 500)
    ;
    readonly Color editAreaColor = new Color(0.2f, 0.2f, 0.2f, 1)
    ;
    GUIStyle richTextStyle;
    void DrawConstraintsEditor(Slots slot) {
        if (richTextStyle is null) { richTextStyle = GUI.skin.label; richTextStyle.richText = true; richTextStyle.normal.textColor = Color.white; }

        if (slot is ItemSlot item) {
            EditorGUI.DrawRect(editLabelRect, groupColor);
            EditorGUI.LabelField(editLabelRect, $"조건 아이템 : <b>{item.label}</b>", richTextStyle);
            EditorGUI.DrawRect(editAreaRect, editAreaColor);
        }
        else if (slot is SkillSlot skill) {

        }
        // if null, nothing to draw.
    }
    #endregion







    /// <summary>Custom struct for implenenting + operator in <see cref="UnityEngine.Rect"/></summary>
    struct RecT {
        public RecT(float x, float y, float z, float w) { a=x; b=y; c=z; d=w; }
        public float a,b,c,d;
        public static RecT operator +(RecT l, RecT r) => new RecT(l.a+r.a, l.b+r.b, l.c+r.c, l.d+r.d);
        public static Rect operator +(RecT l, Rect r) => new Rect(l.a+r.x, l.b+r.y, l.c+r.width, l.d+r.height);
        public static Rect operator +(Rect l, RecT r) => new Rect(l.x+r.a, l.y+r.b, l.width+r.c, l.height+r.d);
        public static implicit operator Rect(RecT a) => new Rect(a.a, a.b, a.c, a.d);
    }

    interface Slots {}
    [Serializable]
    class ItemSlot : Slots {
        //* Selection
        public string label;
        public Sprite sprite;

        //* Constraints
        public int minCount, maxCount;
        public (IAttributeFloat type, float min, float max)[] floatConstraints;
        public (IAttributeInt   type, int   min, int   max)[]   intConstraints;
        public Property[] propConstraints;
    }
    [Serializable]
    class SkillSlot : Slots {
        public SkillEnum skill;
        public string label => skill.ToString();
        public Skill.Branch[] requireBranches;
    }
    [Serializable]
    class ConstraintsSlot : Slots {

    }
    [Serializable]
    class ResultSlot : Slots {
        [Serializable]
        class ItemReward {
            enum ValueType { Single, Average, Min, Max } // 기준 값을 어떤 값으로 선택할지
            (IAttributeFloat type, float add1, float prod, float add2, ValueType method, int itemsIndex)[] floatAttribs; // 속성 변경을 위한 프로퍼티.
            (IAttributeInt type, float add1, float prod, float add2, ValueType method, int itemsIndex)[] intAttribs; //결과물의 속성값은 (value + add1) * prod + add2가 된다.
            Property[] AddProperties, RemoveProperties;
        }

        ItemReward[] itemRewards;
        float[] toolDurabilityReduces, expRewards;
    }


    public class FileToRecipe {
        public static CRecipe CreateRecipe() {
            return new CRecipe();
        }
    }
}

}
#endif