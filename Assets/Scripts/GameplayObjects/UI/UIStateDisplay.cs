using disguys.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.UI
{
    /// <summary>
    /// 해당 클래스는 UI 프리팹의 참조를 포함하는 클래스이며, 활성화되면 UI 자식 요소를 표시할 수 있습니다.
    /// 이 클래스에는 하나의 참조가 있으며, 프리팹에 대해 기본적으로 비활성화되어 있습니다.
    /// 이 클래스는 일반적으로 UI와 관련된 게임 객체에 대한 참조를 포함하는 스크립트에서 사용됩니다.
    /// </summary>
    public class UIStateDisplay : MonoBehaviour
    {
        [SerializeField]
        UIName m_UIName;

        public void DisplayName(NetworkVariable<FixedPlayerName> networkedName)
        {
            m_UIName.gameObject.SetActive(true);
            m_UIName.Initialize(networkedName);
        }
    }
}