using System.Collections.Generic;
using disguys.Gameplay.Configuration;
using disguys.Gameplay.GameplayObjects.Character;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects
{
    public class GameDataSource : MonoBehaviour
    {
        /// <summary>
        /// 이것은 모든 GameData에 대한 정적인 접근자입니다.
        /// 즉, 게임 내에서 데이터에 대한 접근이 필요할 때 이 접근자를 사용하여
        /// GameData 클래스의 인스턴스를 가져올 수 있습니다.
        /// 이것은 일반적으로 싱글톤 패턴과 유사한 패턴을 사용하여 구현됩니다.
        /// </summary>
        public static GameDataSource Instance { get; private set; }

        [Header("Character classes")]
        [Tooltip("All CharacterClass data should be slotted in here")]
        [SerializeField]
        private CharacterClass[] m_CharacterData;

        Dictionary<CharacterTypeEnum, CharacterClass> m_CharacterDataMap;
        
        public Dictionary<CharacterTypeEnum, CharacterClass> CharacterDataByType
        {
            get
            {
                if (m_CharacterDataMap == null)
                {
                    m_CharacterDataMap = new Dictionary<CharacterTypeEnum, CharacterClass>();
                    foreach (CharacterClass data in m_CharacterData)
                    {
                        if (m_CharacterDataMap.ContainsKey(data.CharacterType))
                        {
                            throw new System.Exception($"Duplicate character definition detected: {data.CharacterType}");
                        }
                        m_CharacterDataMap[data.CharacterType] = data;
                    }
                }
                return m_CharacterDataMap;
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                throw new System.Exception("Multiple GameDataSources defined!");
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }

}
