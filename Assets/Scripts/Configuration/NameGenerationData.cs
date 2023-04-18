using UnityEngine;

namespace disguys.Gameplay.Configuration
{
    /// <summary>
    /// 플레이어 이름을 생성할 때 사용되는 모든 유효한 문자열 데이터 저장소입니다. 현재 이름은 형용사-명사 조합으로 이루어진 두 단어 조합입니다. (예: Happy Apple)
    [CreateAssetMenu(menuName = "GameData/NameGeneration", order = 2)]
    public class NameGenerationData : ScriptableObject
    {
        [Tooltip("The list of all possible strings the game can use as the first word of a player name")]
        public string[] FirstWordList;

        [Tooltip("The list of all possible strings the game can use as the second word in a player name")]
        public string[] SecondWordList;

        public string GenerateName()
        {
            var firstWord = FirstWordList[Random.Range(0, FirstWordList.Length - 1)];
            var secondWord = SecondWordList[Random.Range(0, SecondWordList.Length - 1)];

            return firstWord + " " + secondWord;
        }
    }
}