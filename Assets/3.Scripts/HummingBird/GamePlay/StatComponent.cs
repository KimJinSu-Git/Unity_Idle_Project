using UnityEngine;
using System.Collections.Generic;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// Core Stat 및 계산된 전투 Stat을 관리하는 컴포넌트
    /// </summary>
    public class StatComponent
    {
        // 스탯 4종류
        public int Strength { get; private set; } = 1; // Attack / Health / CritDamage => 근본 공격 + 체력 탱킹 증가
        public int Dexterity { get; private set; } = 1; // AttackSpeed / CritChance / Accuracy / Evasion => 빠른 공격 + 회피 + 명중
        public int Intelligence { get; private set; } = 1; // MagicResistance / HealthRegen / SkillDamage / SkillCooldown => 스킬 기반 성장
        public int Luck { get; private set; } = 1; // GoldDrop / ItemDrop / CritDamage / GemDrop => 보상 및 크리데미지 증가

        // BASE MODIFIERS (장비, 영구 강화 등)
        // 장비/영구 보너스는 StatComponent 외부에서 제공
        
        // COMBAT STATS (최종 스탯)
        public float FinalAttackPower { get; private set; }
        public float FinalMaxHealth { get; private set; }
        public float FinalCritChance { get; private set; } // 민첩 기반
        public float FinalCritDamage { get; private set; } // 힘 + 행운 기반
        public float FinalAttackSpeed { get; private set; } // 민첩 기반
        public float FinalHealthRegen { get; private set; } // 지능 기반
        
        public float FinalDefensivePower { get; private set; }
        public float FinalMagicResistance { get; private set; }
        public float FinalEvasion { get; private set; } // % 단위 (0 ~ 1)
        public float FinalAccuracy { get; private set; } // % 단위 (0 ~ 1)
        public float FinalGoldDrop { get; private set; } // % 단위 (1.0 = 100% 드롭률 기준)
        public float FinalItemDrop { get; private set; } 
        public float FinalGemDrop { get; private set; }

        /// <summary>
        /// 모든 Core Stat과 외부 보너스를 합산하여 최종 전투 스탯을 계산
        /// </summary>
        public void CalculateFinalStats(
            (float attack, float health, float critChance, float critDamage, float attackSpeed, float defensivePower, float magicResistance, float healthRegen, float evasion, float accuracy, float luckBonus) equipBonus,
            float permanentAtk, float permanentHp)
        {
            FinalAttackPower = (7.5f + (Strength * 2.5f)) + permanentAtk + equipBonus.attack;
            FinalMaxHealth = (95f + (Strength * 5f)) + permanentHp + equipBonus.health;
            FinalHealthRegen = (Intelligence * 0.1f) + equipBonus.healthRegen;

            FinalCritDamage = 1.0f + (Strength * 0.05f) + (Luck * 0.05f) + equipBonus.critDamage; 
            FinalCritChance = (Dexterity * 0.005f) + equipBonus.critChance; // DEX당 0.5% 증가 (0.0 ~ 1.0)
            
            FinalAttackSpeed = 1.0f + (Dexterity * 0.01f) + equipBonus.attackSpeed; // 기본 100% (1.0)
            FinalDefensivePower = 0f + equipBonus.defensivePower; // 장비 위주
            FinalMagicResistance = (Intelligence * 0.1f) + equipBonus.magicResistance; // INT가 저항에 기여
            
            FinalEvasion = (Dexterity * 0.001f) + equipBonus.evasion; // DEX당 0.1% 증가
            FinalAccuracy = 1.0f + (Dexterity * 0.005f) + equipBonus.accuracy; // 기본 100% (1.0) + DEX 기여
            
            FinalGoldDrop = 1.0f + (Luck * 0.01f) + equipBonus.luckBonus; // 기본 100% (1.0) + LCK 기여
            // FinalItemDrop = 0.01f + (Luck * 0.0005f) + equipBonus.itemDrop;
            // FinalGemDrop = 0.0001f + (Luck * 0.00005f) + equipBonus.gemDrop; 
            
        }

        /// <summary>
        /// Stat Point 투자 시 호출
        /// </summary>
        public bool TryAllocateStatPoint(string statName)
        {
            switch (statName.ToLower())
            {
                case "strength": Strength++; break;
                case "dexterity": Dexterity++; break;
                case "intelligence": Intelligence++; break;
                case "luck": Luck++; break;
                default: return false;
            }
            return true;
        }
        
        /// <summary>
        /// 로드 시 Core Stat을 복원
        /// </summary>
        public void RestoreCoreStats(int str, int dex, int intel, int luck)
        {
            Strength = str;
            Dexterity = dex;
            Intelligence = intel;
            Luck = luck;
        }
    }
}