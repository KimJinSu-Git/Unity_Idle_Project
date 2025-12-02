using System;
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
        
        [Header("Animation Settings")]
        [SerializeField] private float damageApplicationTime = 0.5f; 

        private CharacterManager characterManager;
        private BattleManager battleManager;
        
        private float currentAttackCooldown;
        private bool isAttacking = false;
        
        private int runAnimHash;
        private int attackAnimHash;
        private int idleAnimHash;
        private int deathAnimHash;
        
        public int GetRunAnimHash => runAnimHash;
        public int GetAttackAnimHash => attackAnimHash;
        public int GetIdleAnimHash => idleAnimHash;
        public int GetDeathAnimHash => deathAnimHash;
        public Animator GetAnimator => animator;
        
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
            
            runAnimHash = Animator.StringToHash(runAnim);
            attackAnimHash = Animator.StringToHash(attackAnim);
            idleAnimHash = Animator.StringToHash(idleAnim);
            deathAnimHash = Animator.StringToHash(deathAnim);
        }

        private void Start()
        {
            currentAttackCooldown = 3f;
        }

        private void Update()
        {
            if (!battleManager.PlayerBattleMode) return; 

            currentAttackCooldown -= Time.deltaTime;
            
            if (currentAttackCooldown <= 0f && characterManager.IsAlive)
            {
                if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == idleAnimHash)
                {
                    TryTriggerAttack();
                }
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
            if(isFighting)
            {
                animator.Play(idleAnimHash);
            }
            else
            {
                animator.Play(runAnimHash);
            }
        }
        
        private void TryTriggerAttack()
        {
            animator.Play(attackAnimHash);
            currentAttackCooldown = battleManager.GetAttackInterval();
            StartCoroutine(ApplyDamageAfterDelay());
        }
        
        private IEnumerator ApplyDamageAfterDelay()
        {
            yield return new WaitForSeconds(damageApplicationTime);
    
            battleManager.TryAutoAttack(); 
        }
        
        private void PlayDeathAnimation()
        {
            animator.Play(deathAnim);
            
            StopAllCoroutines(); 
            // TODO: GameManager에 게임 오버 상태를 알림
        }
    }
}