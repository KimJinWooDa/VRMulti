using disguys.Gameplay.GameplayObjects.Character;
using disguys.Infrastructure;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects
{
    /// <summary>
    /// 이것은 서버와 클라이언트 모두에서 채워지는 <see cref="PersistentPlayer"/> 객체의 런타임 리스트입니다.
    /// </summary>
    
    [CreateAssetMenu]
    public class ClientPlayerAvatarRuntimeCollection : RuntimeCollection<ClientPlayerAvatar>
    {
    }

}
