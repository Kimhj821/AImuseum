using UnityEngine;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance;

    public GameObject[] relics; // RelicsPedestal_1, _2, _3...

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
}
