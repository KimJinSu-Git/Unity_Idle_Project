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
        
        // TODO: DefensivePower, MagicResistance, Evasion, Accuracy, GoldDrop, ItemDrop, GemDrop 추가 필요

        /// <summary>
        /// 모든 Core Stat과 외부 보너스를 합산하여 최종 전투 스탯을 계산
        /// </summary>
        public void CalculateFinalStats(
            (float attack, float health, float critChance, float critDamage, float attackSpeed, float defensivePower, float magicResistance, float healthRegen, float evasion, float accuracy, float luckBonus) equipBonus,
            float permanentAtk, float permanentHp)
        {
            // ATK: STR 기여 기본 공격력 7.5f에 초기스탯 1씩을 포함해서 2.5f 더하면 10f로 시작하도록 가정
            FinalAttackPower = (7.5f + (Strength * 2.5f)) + permanentAtk + equipBonus.attack;
            
            // MaxHealth: STR 기여
            FinalMaxHealth = (95f + (Strength * 5f)) + permanentHp + equipBonus.health;

            // Crit Damage (%): STR/LCK 기여 // 기본 100%(1.0f) 기준
            FinalCritDamage = 1.0f + (Strength * 0.05f) + (Luck * 0.05f) + equipBonus.critDamage; 
            
            // HealthRegen: INT 기여
            FinalHealthRegen = (Intelligence * 0.1f) + equipBonus.healthRegen;

            // TODO: 나머지 Stat 계산 로직 추가
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