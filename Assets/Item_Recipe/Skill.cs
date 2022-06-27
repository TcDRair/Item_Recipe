using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rair.Skills {
    public abstract class Skill {
        public abstract string name { get; }
        public float exp { get; private set; }
        public abstract float maxExp { get; }
        public int lv { get; private set; }
        public abstract int maxLv { get; }

        protected float totalExp = 0f, _prevTExp;
        public bool changed {
            get {
                if (_prevTExp == totalExp) return false;
                _prevTExp = totalExp;
                return true;
            }
        }

        bool max = false;
        public void AddExp(float amt) {
            if (max) return;
            if ((exp += amt) >= maxExp) {
                if (++lv >= maxLv) { lv = maxLv; exp = maxExp; max = true; }
                else exp = 0;
            }

            totalExp += amt;
        }


        public class Branch {
            public enum Tier { O, I, II, III, IV, V, VI, VII, VIII, IX, X, XI, XII, XIII, XIV, XV, XVI, XVII, XVIII, XIX, XX }
            public readonly Tier tier;

            public List<Branch> prev, post;

            public Branch(string name, string description, Tier tier, params Branch[] previousBranches) {
                this.tier = tier;
                prev = new List<Branch>();
                post = new List<Branch>();


                foreach(var br in previousBranches) {
                    if (this.tier < br.tier) Debug.LogError($"Tree progress between {br.tier} -> {this.tier} is not allowed.");
                    br.post.Add(this);
                    this.prev.Add(br);
                }
            }
        }
        
        protected abstract Branch[] Branches { get; }
        Dictionary<Branch, bool> _acq;
        Dictionary<Branch, bool> acquired {
            get {
                if (_acq is null) {
                    _acq = new Dictionary<Branch, bool>();
                    foreach (var b in Branches) _acq.Add(b, false);
                }
                return _acq;
            }
        }
        /// <summary>브랜치가 해당 스킬트리에 존재하며, 습득한 상태인지 확인합니다.</summary>
        public bool Acquired(Branch branch) => acquired.ContainsKey(branch) && acquired[branch];
        /// <summary>해당 스킬트리에 존재하는 브랜치를 습득합니다.</summary>
        public void Acquire(Branch branch) => acquired[branch] = true;
    }


    public sealed class Cooking : Skill {
        public override string name => "요리";
        public override int maxLv => 60;
        public override float maxExp => 100f;

        protected override Branch[] Branches => new Branch[] { Grill, Grind, Knead, Smoke, Bake, };


        public readonly static Branch Grill = new("굽기", "식재료를 불에 짧게 구워내는 요리법.", Branch.Tier.I), // 보존기간 증가, 날것 특성 제거
            Grind = new("제분", "건조 상태의 식재료를 잘게 갈아내는 요리법", Branch.Tier.II), // 보존기간 증가, 식재료 특성 유지
            Knead = new("반죽", "가루 등을 액체에 섞어 덩어리로 만드는 요리법", Branch.Tier.III, Grind), // 보존기간 일부 감소, 식재료 특성 유지
            Smoke = new("훈연", "연기를 쐬어 풍미를 더하고 보존을 용이하게 만드는 요리법", Branch.Tier.III, Grill), // 보존기간 증폭, 날것 특성 제거
            Bake  = new("제빵", "", Branch.Tier.IV, Knead) //TODO 보존기간 미정, 많은 특성 변화, + 별도의 숙련도 시스템 필요
        ;
    }

    public sealed class Dexterity : Skill {
        public override string name => "손재주";
        public override int maxLv => 20;
        public override float maxExp => 200f;

        protected override Branch[] Branches => new Branch[] {
            DexEfficiencyI, DexEfficiencyII, DexEfficiencyIII, DexEfficiencyIV, DexEfficiencyMaster,
            Weave,
        };

        public readonly static Branch DexEfficiencyI = new("", "", Branch.Tier.I),
            DexEfficiencyII  = new("", "", Branch.Tier.II, DexEfficiencyI),
            DexEfficiencyIII = new("", "", Branch.Tier.III, DexEfficiencyII),
            DexEfficiencyIV  = new("", "", Branch.Tier.IV, DexEfficiencyIII),
            DexEfficiencyV   = new("", "", Branch.Tier.V, DexEfficiencyIV),
            DexEfficiencyMaster = new("", "", Branch.Tier.X, DexEfficiencyV),
            
            Weave = new("", "", Branch.Tier.III)

        ;
    }


}