using System.Collections;
using UnityEngine;
using Bird.Idle.Core;
using Bird.Idle.Gameplay;

namespace Bird.Idle.Visual
{
    /// <summary>
    /// 플레이어 캐릭터의 애니메이션 및 시각적 상태를 제어
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public static Transform PlayerTransform { get; private set; }
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string runAnim = "Run";
        [SerializeField] private string attackAnim = "Attack";
        [SerializeField] private string idleAnim = "Idle";
        [SerializeField] private string deathAnim = "Death";

        private CharacterManager characterManager;
        private BattleManager battleManager;

        private void Awake()
        {
            characterManager = CharacterManager.Instance;
            battleManager = BattleManager.Instance;
            
            PlayerTransform = transform;
            
            if (animator == null) animator = GetComponentInChildren<Animator>();

            if (battleManager != null)
            {
                battleManager.OnBattleStateChanged += UpdateVisualState;
            }
            if (characterManager != null)
            {
                characterManager.OnPlayerDied += PlayDeathAnimation;
            }
        }
        
        private void OnEnable()
        {
            UpdateVisualState(false); 
        }

        private void OnDestroy()
        {
            if (battleManager != null)
            {
                battleManager.OnBattleStateChanged -= UpdateVisualState;
            }
            if (characterManager != null)
            {
                characterManager.OnPlayerDied -= PlayDeathAnimation;
            }
        }

        /// <summary>
        /// BattleManager의 상태 변경에 따라 애니메이션을 업데이트
        /// </summary>
        public void UpdateVisualState(bool isFighting)
        {
            if (characterManager != null && !characterManager.IsAlive) return;

            if (isFighting)
            {
                PlayAttackLoop();
            }
            else
            {
                PlayRunAnimation();
            }
        }

        private void PlayRunAnimation()
        {
            StopCoroutine(AttackLoopCoroutine());
            animator.Play(runAnim);
        }
        
        private void PlayAttackLoop()
        {
            StartCoroutine(AttackLoopCoroutine());
        }

        private IEnumerator AttackLoopCoroutine()
        {
            float attackInterval = battleManager.GetAttackInterval;
            
            while (characterManager.IsAlive)
            {
                animator.Play(attackAnim);
                yield return new WaitForSeconds(1f); 
                
                animator.Play(idleAnim);
                yield return new WaitForSeconds(attackInterval - 0.1f);
            }
        }
        
        private void PlayDeathAnimation()
        {
            animator.Play(deathAnim);
            
            StopAllCoroutines(); 
            // TODO: GameManager에 게임 오버 상태를 알림
        }
    }
}