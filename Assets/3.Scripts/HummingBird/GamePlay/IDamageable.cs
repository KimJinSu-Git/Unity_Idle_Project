using UnityEngine;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// 데미지를 받을 수 있는 모든 객체가 구현해야 할 인터페이스
    /// </summary>
    public interface IDamageable
    {
        void ApplyDamage(float damage);
        
        bool IsAlive { get; }
    }
}