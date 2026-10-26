using UnityEngine;

namespace Systems
{
    public class EffectManager
    {

        [Header("Hit Effects")]
        [SerializeField] private GameObject playerHitPrefab;

        public void SpawnPlayerAttack(Vector3 position, GameObject attackPrefab)
        {
            if (attackPrefab != null) {
                Managers.Resource.Instantiate(attackPrefab, position);
            }
        }

        public void SpawnPlayerHit(Vector3 position)
        {
            if (playerHitPrefab != null)
            {
                Managers.Resource.Instantiate(playerHitPrefab, position, Quaternion.identity);
            }
        }

        public void SpawnEnemyHit(Vector3 position, GameObject hitPrefab)
        {
            if (hitPrefab != null)
            {
                Managers.Resource.Instantiate(hitPrefab, position);
            }
        }

        //public void SpawnStun(Transform parent, float duration)
        //{
        //    if (stunPrefab != null)
        //    {
        //        StunEffect existingStun = parent.GetComponentInChildren<StunEffect>();
        //        if (existingStun != null)
        //        {
        //            // 이미 스턴 이펙트가 있다면 시간만 갱신합니다.
        //            existingStun.Refresh(duration);
        //        }
        //        else
        //        {
        //            // 없다면 새로 생성하고 StunEffect 컴포넌트를 붙여줍니다.
        //            GameObject stunObj = Instantiate(stunPrefab, parent.position, Quaternion.identity, parent);
        //            StunEffect newStun = stunObj.AddComponent<StunEffect>();
        //            newStun.Refresh(duration);
        //        }
        //    }
        //}
    }
}