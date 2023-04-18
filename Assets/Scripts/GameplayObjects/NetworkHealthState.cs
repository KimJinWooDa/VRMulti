using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace disguys.Gameplay.GameplayObjects
{
    /// <summary>
    /// 이 MonoBehaviour는 이 객체의 피격판정을 나타내는 NetworkVariableBool 하나만을 포함합니다.
    /// </summary>
    public class NetworkHealthState : NetworkBehaviour
    {
        [FormerlySerializedAs("IsAlive")] [FormerlySerializedAs("IsSurvive")] [FormerlySerializedAs("isSurvive")] [FormerlySerializedAs("HitPoints")] [HideInInspector] 
        public NetworkVariable<bool> Alive = new NetworkVariable<bool>(true);

        public event Action HitPointDepleted; //hp -> 0 즉 down

        void OnEnable()
        {
            Alive.OnValueChanged += HitPointsChanged;
        }

        void OnDisable()
        {
            Alive.OnValueChanged -= HitPointsChanged;
        }


        void HitPointsChanged(bool previousValue, bool newValue)
        {
            if (previousValue) //누군가 attack을 해서 true가 된다면
            {
                HitPointDepleted?.Invoke();
            }
        }
    }
}

