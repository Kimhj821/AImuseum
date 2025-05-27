using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public Text dialogueText;   // Text(Legacy) 연결
    public Transform playerHead;  // Main Camera 연결
    public Vector3 offset = new Vector3(0, 0, 2f); // 카메라 앞 2m 위치

    void Update()
    {
        // 캔버스를 플레이어 앞에 위치
        transform.position = playerHead.position + playerHead.forward * offset.z;
        transform.rotation = Quaternion.LookRotation(transform.position - playerHead.position);
    }

    public void ShowDialogue(string text)
    {
        dialogueText.text = text;
    }
}