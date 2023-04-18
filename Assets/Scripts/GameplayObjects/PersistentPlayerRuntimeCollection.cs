using disguys.Infrastructure;
using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects
{
    //클라이언트와 서버 모두에 채워지는 "PersistentPlayer" 객체의 런타임 List(목록)
    [CreateAssetMenu]
    public class PersistentPlayerRuntimeCollection : RuntimeCollection<PersistentPlayer>
    {
        public bool TryGetPlayer(ulong clientID, out PersistentPlayer persistentPlayer)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (clientID == Items[i].OwnerClientId) 
                 {
                     persistentPlayer = Items[i];
                     return true;
                 }
            }

            persistentPlayer = null;
            return false;
        }
    }

}
