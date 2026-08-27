using UnityEngine;
using System;

[Serializable]
public struct EntityStat  // 개체 공통 스탯 (플레이어, 몬스터 공유)
{
    #region Fields
    [SerializeField] internal int level;
    [SerializeField] internal int maxHP;
    [SerializeField] internal int attackPower;  // 공격력
    [SerializeField] internal int defensePower;  // 물리 방어
    [SerializeField] internal int spDefensePower;  // 특수 방어
    [SerializeField] [Range(0, 100)]
    internal float critRate;  // 치명타 확률
    [SerializeField] internal float critDamage; // 치명타 피해량 (기존 공격력에 몇 배인지)
    [SerializeField] internal float speed;  // 속도
    [SerializeField] internal float attackSpeed;  // 공격 속도
    [SerializeField] internal int hpRegen;  // 초당 체력 회복량
    [SerializeField] internal float jumpPower;  // 점프력
    #endregion
}

[Serializable]
public struct PlayerStat  // 플레이어만 가지는 스탯
{
    #region Fields
    [SerializeField] internal EntityStat commonStat;
    [SerializeField] internal int exp;  // 경험치
    [SerializeField] internal int maxStamina;  // 최대 스태미너
    [SerializeField] internal int staminaRegen;  // 초당 스태미너 회복량
    #endregion
}

public abstract class EntityData : ScriptableObject
{
    #region Fields
    [SerializeField] internal string entityName;
    #endregion

    #region Properties
    public string EntityName => entityName;
    #endregion

    // 여러 메서드 (보류)
}
