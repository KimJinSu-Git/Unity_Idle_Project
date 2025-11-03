using UnityEngine;
using System.Collections.Generic;
using System;
using Bird.Idle.Data;
using Bird.Idle.Core;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// 장비 아이템의 획득, 저장, 장착 및 해제를 관리하는 싱글톤 클래스
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        // 장착된 장비 딕셔너리 (Type별 1개)
        private Dictionary<EquipmentType, EquipmentData> equippedItems = new Dictionary<EquipmentType, EquipmentData>();
        
        public Action OnEquipmentChanged;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 장비를 장착하고 스탯을 CharacterManager에 반영하도록 요청
        /// </summary>
        public bool EquipItem(EquipmentData item)
        {
            if (item == null) return false;

            if (equippedItems.ContainsKey(item.type))
            {
                UnequipItem(item.type);
            }

            // 장비 장착 및 인벤토리에서 제거
            equippedItems[item.type] = item;

            // 스탯 반영
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.ApplyEquipmentStats(); 
            }
            
            OnEquipmentChanged?.Invoke(); // 장착 UI 갱신
            return true;
        }
        
        /// <summary>
        /// 장비를 해제하고 인벤토리에 반환
        /// </summary>
        public void UnequipItem(EquipmentType type)
        {
            if (equippedItems.TryGetValue(type, out EquipmentData unequippedItem))
            {
                equippedItems.Remove(type);
                
                // 스탯 업데이트 요청
                if (CharacterManager.Instance != null)
                {
                    CharacterManager.Instance.ApplyEquipmentStats();
                }

                OnEquipmentChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// 특정 장비 타입에 해당 ID의 아이템이 현재 장착 중인지 확인
        /// </summary>
        public bool IsItemEquipped(EquipmentType type, int equipID)
        {
            if (equippedItems.TryGetValue(type, out EquipmentData equippedItem))
            {
                // 장착된 아이템의 ID와 현재 슬롯의 ID가 일치하는지 확인
                return equippedItem.equipID == equipID; 
            }
            return false;
        }
        
        public EquipmentData GetEquippedItem(EquipmentType type)
        {
            equippedItems.TryGetValue(type, out EquipmentData item);
            return item;
        }
        
        /// <summary>
        /// 현재 장착 중인 모든 장비의 스탯 합계를 계산
        /// public (float totalAttack, float totalHealth) => 튜플(Tuple) 기능의 메서드 반환 형식.
        /// 여러 개의 값을 하나의 묶음으로 반환할 때 유용하며, 코드를 간결하게 함.
        /// float 형식의 두 값을 반환하며, 각각 totalAttack, totalHealth 라는 이름으로 접근할 수 있도록 지정.
        /// (float totalAttack, float totalHealth)이 아닌 (float, float)로 해도 상관없음. 이름은 그냥 가독성을 높여주고자 적은 내용.
        /// </summary>
        public (float totalAttack, float totalHealth) GetTotalEquipmentBonus()
        {
            float totalAttack = 0;
            float totalHealth = 0;

            foreach (var item in equippedItems.Values)
            {
                totalAttack += item.attackBonus;
                totalHealth += item.healthBonus;
            }
            return (totalAttack, totalHealth); // 반환 형식에서 이름을 지정했기 때문에, 컴파일러는 이 값들이 그 이름에 해당한다고 유추.
        }
    }
}