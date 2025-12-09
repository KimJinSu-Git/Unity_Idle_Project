using UnityEngine;
using Bird.Idle.Data;
using Bird.Idle.Core;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// 보스 몬스터 전용 컨트롤러
    /// </summary>
    public class BossMonsterController : MonsterController
    {
        private readonly int attack2Hash = Animator.StringToHash("Attack2");
        // TODO ::: 추후 Attack3, Skill 등이 추가될 수 있음

        protected override void TryAttackPlayer()
        {
            float monsterDamage = MonsterData.baseDamage;
            
            if (CharacterManager.Instance != null && monsterDamage > 0)
            {
                CharacterManager.Instance.ApplyDamage(monsterDamage);
                
                PlayRandomAttackAnimation();
            }
        }

        private void PlayRandomAttackAnimation()
        {
            if (Animator == null) return;

            int randomAction = UnityEngine.Random.Range(0, 2); 

            if (randomAction == 0)
            {
                Animator.Play(Attack1Hash);
            }
            else
            {
                Animator.Play(attack2Hash);
            }
        }
    }
}