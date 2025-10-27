using System;

namespace Bird.Idle.Core
{
    /// <summary>
    /// 파일에 직렬화되어 저장될 모든 게임 데이터의 컨테이너
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public long LastExitTimeTicks;
        
        // 재화 데이터
        public long GoldAmount; 
        
    }
}