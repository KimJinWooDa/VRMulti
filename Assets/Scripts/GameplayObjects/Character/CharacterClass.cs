using disguys.Gameplay.GameplayObjects.Character;
using UnityEngine;

namespace disguys.Gameplay.Configuration
{
    /// <summary>
    /// 캐릭터의 데이터 표현
    /// </summary>
    
    [CreateAssetMenu(menuName = "GameData/CharacterClass", order = 1)]
    public class CharacterClass : ScriptableObject
    {
        [Tooltip("which character this data represents")]
        public CharacterTypeEnum CharacterType;
    }
}
