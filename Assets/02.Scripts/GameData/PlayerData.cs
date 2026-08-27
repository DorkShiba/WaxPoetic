using System;
using UnityEngine;

namespace GameData
{
    [CreateAssetMenu(fileName = "New Player Data", menuName = "Data/Player")]
    public class PlayerData : EntityData
    {
        #region Fields
        [SerializeField] private PlayerStat playerStat;  // 기본 스탯(1레벨 기준)

        [Header("성장 밸런스")]
        [Tooltip("x축: 레벨, y축: 스탯 곱연산 계수")]
        [SerializeField] private AnimationCurve statGrowthCurve;  // 레벨에 따른 스탯 계수

        [SerializeField] private int species;  // 종(개, 고양이), 현재는 보류
        [SerializeField] private Vector2 currPosition;  // 현재 위치(현재 맵 상의 좌표)
        [SerializeField] private int currLocation;  // 현재 위치(맵 번호)
        #endregion

        [Header("스킬 데미지 배율")]
        [Tooltip("Skill1~4의 attackPower 배율. 예: 1.0 = 100% 공격력")]
        [SerializeField] private float[] skillDamageMultipliers = { 1.0f, 1.5f, 2.0f, 2.5f };

        #region Properties
        public PlayerStat PlayerStat => playerStat;
        public int Level => playerStat.commonStat.level;
        public int MaxHP => PlayerStat.commonStat.maxHP;
        public int MaxStamina => playerStat.maxStamina;
        public int AtkPower => PlayerStat.commonStat.attackPower;
        public int DfsPower => PlayerStat.commonStat.defensePower;
        public int SpdPower => playerStat.commonStat.spDefensePower;
        public float CritRate => playerStat.commonStat.critRate;
        public float CritDamage => playerStat.commonStat.critDamage;
        public float Spd => playerStat.commonStat.speed;
        public float AtkSpd => playerStat.commonStat.attackSpeed;
        public int HpRegen => playerStat.commonStat.hpRegen;
        public int StaminaRegen => playerStat.staminaRegen;

        public Vector2 CurrPosition
        {
            get => currPosition;
            set => currPosition = value;
        }
        #endregion

        public float GetStatMultiplier(int level)
        {
            return statGrowthCurve.Evaluate(level);
        }

        public float GetSkillDamage(int skillIndex)
        {
            float multiplier = (skillIndex < skillDamageMultipliers.Length) ? skillDamageMultipliers[skillIndex] : 1f;
            float levelScale = GetStatMultiplier(Level);
            return AtkPower * multiplier * levelScale;
        }
    }
}
