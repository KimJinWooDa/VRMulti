using System;
using UnityEngine;

namespace disguys.Infrastructure
{
    //이 ScriptableObject은 고유 식별을 위한 GUID를 저장하는데 사용됩니다.
    //이 필드의 값은 에디터 스크립트에서 구현됩니다.
    //즉, 에디터에서 해당 ScriptableObject의 값을 설정하고 저장할 수 있습니다.
    [Serializable]
    public abstract class GuidScriptableObject : ScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        byte[] m_Guid;

        public Guid Guid => new Guid(m_Guid);

        void OnValidate()
        {
            if (m_Guid.Length == 0)
            {
                m_Guid = Guid.NewGuid().ToByteArray();
            }
        }
    }
}