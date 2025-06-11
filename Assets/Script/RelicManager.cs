using UnityEngine;
using System.Collections;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance;

    public GameObject[] relics; // RelicsPedestal_1, _2, _3...
     public GameObject[] pointLights;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void EnableRelicByRoomNum(int roomNumber)
    {
        int index = roomNumber - 1;
        if (index >= 0 && index < relics.Length)
        {
            if (relics[index] != null && !relics[index].activeSelf)
            {
                relics[index].SetActive(true);  // 해당 유물만 true로, 나머지는 건드리지 않음
            }
        }
    }
    // 🌟 추가: 포인트 라이트 활성화 (6초 후 실행됨)
    
    public void EnableLightByRoomNum(int roomNumber)
    {

        int index = roomNumber;  // 예: roomNumber 1이면 index 1 → PointLight_2
        if (index >= 0 && index < pointLights.Length)
        {
            if (pointLights[index] != null && !pointLights[index].activeSelf)
            {
                pointLights[index].SetActive(true);
            }
        }
    }

    
}
