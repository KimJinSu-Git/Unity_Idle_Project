using System;

namespace Bird.Idle.Data
{
    [Serializable]
    public class CollectionEntry
    {
        public int equipID;
        public int count; // 보유 수량
        public int collectionLevel; // 컬렉션 업그레이드 레벨

        public CollectionEntry(int id)
        {
            equipID = id;
            count = 0;
            collectionLevel = 0;
        }
    }
}