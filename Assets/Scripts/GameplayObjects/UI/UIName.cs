using disguys.Utilities;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.UI
{
    /// <summary>
    /// 이 클래스는 개체 이름을 시각적으로 나타내는 UI 개체를 나타냅니다.
    /// 이 클래스는 NetworkVariable이 수정될 때마다 시각적 요소가 업데이트됩니다.
    /// 즉, 개체 이름이 변경될 때마다 UI가 업데이트됩니다.
    /// </summary>
    public class UIName : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI m_UINameText;

        NetworkVariable<FixedPlayerName> m_NetworkedNameTag;

        public void Initialize(NetworkVariable<FixedPlayerName> networkedName)
        {
            m_NetworkedNameTag = networkedName;

            m_UINameText.text = networkedName.Value.ToString();
            networkedName.OnValueChanged += NameUpdated;
        }

        void NameUpdated(FixedPlayerName previousValue, FixedPlayerName newValue)
        {
            m_UINameText.text = newValue.ToString();
        }

        void OnDestroy()
        {
            m_NetworkedNameTag.OnValueChanged -= NameUpdated;
        }
    }
}