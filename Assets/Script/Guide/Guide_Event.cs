using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

[System.Serializable]
public class GuideData
{
    public string guide_text;
}

public class Guide_Event : MonoBehaviour
{

    public GameObject Player;
    public Text robot_text;

    // frame1~3 이벤트용 오디오 소스
    public AudioSource frame1Audio;
    public AudioSource frame2Audio;
    public AudioSource frame3Audio;

    // event_frame1~3 이벤트용 오디오 소스
    public AudioSource eventFrame1Audio;
    public AudioSource eventFrame2Audio;
    public AudioSource eventFrame3Audio;

    public GameObject frame1;
    public GameObject frame2;
    public GameObject frame3;

    public GameObject event_frame1;
    public GameObject event_frame2;
    public GameObject event_frame3;

    // Player와 frame들 사이의 거리를 저장하는 변수들
    [SerializeField] private float frame1_pos;
    [SerializeField] private float frame2_pos;
    [SerializeField] private float frame3_pos;

    // Player와 event_frame들 사이의 거리를 저장하는 변수들
    [SerializeField] private float event_frame1_pos;
    [SerializeField] private float event_frame2_pos;
    [SerializeField] private float event_frame3_pos;

    // 이벤트 발생을 위한 거리 임계값
    public float frameEventDistance = 3.0f;
    public float eventFrameEventDistance = 2.0f;

    // 이벤트 지속 시간 설정
    public float frameEventDuration = 3.0f;
    public float eventFrameEventDuration = 2.0f;

    // Player 이동 목적지 좌표들
    public Vector3 eventFrame1Destination = new Vector3(0, 0, 0);
    public Vector3 eventFrame2Destination = new Vector3(5, 0, 5);
    public Vector3 eventFrame3Destination = new Vector3(-5, 0, -5);

    // 이벤트 상태 추적 변수들
    [SerializeField] private bool frame1EventActive = false;
    [SerializeField] private bool frame2EventActive = false;
    [SerializeField] private bool frame3EventActive = false;
    [SerializeField] private bool eventFrame1EventActive = false;
    [SerializeField] private bool eventFrame2EventActive = false;
    [SerializeField] private bool eventFrame3EventActive = false;

    // 이벤트 타이머 변수들
    [SerializeField] private float frame1EventTimer = 5f;
    [SerializeField] private float frame2EventTimer = 5f;
    [SerializeField] private float frame3EventTimer = 5f;
    [SerializeField] private float eventFrame1EventTimer = 5f;
    [SerializeField] private float eventFrame2EventTimer = 5f;
    [SerializeField] private float eventFrame3EventTimer = 5f;

    // event_frame 이벤트 후 텍스트 표시 및 이동 지연 시간
    public float eventFrameTextDuration = 3.0f;

    // event_frame 텍스트 표시 타이머 변수들
    [SerializeField] private float eventFrame1TextTimer = 0f;
    [SerializeField] private float eventFrame2TextTimer = 0f;
    [SerializeField] private float eventFrame3TextTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 이벤트 타이머 관리
        UpdateEventTimers();

        // Player와 frame들 사이의 거리 계산
        if (Player != null)
        {
            // frame1~3과의 거리 계산
            if (frame1 != null)
            {
                frame1_pos = Vector3.Distance(Player.transform.position, frame1.transform.position);
                CheckFrameDistance(frame1_pos, "frame1", ref frame1EventActive);
            }
            
            if (frame2 != null)
            {
                frame2_pos = Vector3.Distance(Player.transform.position, frame2.transform.position);
                CheckFrameDistance(frame2_pos, "frame2", ref frame2EventActive);
            }
            
            if (frame3 != null)
            {
                frame3_pos = Vector3.Distance(Player.transform.position, frame3.transform.position);
                CheckFrameDistance(frame3_pos, "frame3", ref frame3EventActive);
            }

            // event_frame1~3과의 거리 계산
            if (event_frame1 != null)
            {
                event_frame1_pos = Vector3.Distance(Player.transform.position, event_frame1.transform.position);
                CheckEventFrameDistance(event_frame1_pos, "event_frame1", ref eventFrame1EventActive);
            }
            
            if (event_frame2 != null)
            {
                event_frame2_pos = Vector3.Distance(Player.transform.position, event_frame2.transform.position);
                CheckEventFrameDistance(event_frame2_pos, "event_frame2", ref eventFrame2EventActive);
            }
            
            if (event_frame3 != null)
            {
                event_frame3_pos = Vector3.Distance(Player.transform.position, event_frame3.transform.position);
                CheckEventFrameDistance(event_frame3_pos, "event_frame3", ref eventFrame3EventActive);
            }
        }
    }

    // 이벤트 타이머 업데이트
    private void UpdateEventTimers()
    {
        // frame 이벤트 타이머
        if (frame1EventActive)
        {
            frame1EventTimer += Time.deltaTime;
            if (frame1EventTimer >= frameEventDuration)
            {
                frame1EventActive = false;
                frame1EventTimer = 0f;
            }
        }

        if (frame2EventActive)
        {
            frame2EventTimer += Time.deltaTime;
            if (frame2EventTimer >= frameEventDuration)
            {
                frame2EventActive = false;
                frame2EventTimer = 0f;
            }
        }

        if (frame3EventActive)
        {
            frame3EventTimer += Time.deltaTime;
            if (frame3EventTimer >= frameEventDuration)
            {
                frame3EventActive = false;
                frame3EventTimer = 0f;
            }
        }

        // event_frame 이벤트 타이머
        if (eventFrame1EventActive)
        {
            eventFrame1EventTimer += Time.deltaTime;
            if (eventFrame1EventTimer >= eventFrameEventDuration)
            {
                eventFrame1EventActive = false;
                eventFrame1EventTimer = 0f;
            }
        }

        if (eventFrame2EventActive)
        {
            eventFrame2EventTimer += Time.deltaTime;
            if (eventFrame2EventTimer >= eventFrameEventDuration)
            {
                eventFrame2EventActive = false;
                eventFrame2EventTimer = 0f;
            }
        }

        if (eventFrame3EventActive)
        {
            eventFrame3EventTimer += Time.deltaTime;
            if (eventFrame3EventTimer >= eventFrameEventDuration)
            {
                eventFrame3EventActive = false;
                eventFrame3EventTimer = 0f;
            }
        }

        // event_frame 텍스트 타이머 및 페이드/이동 처리
        UpdateEventFrameTextTimers();
    }

    // event_frame 텍스트 타이머 업데이트 및 페이드/이동 처리
    private void UpdateEventFrameTextTimers()
    {
        // event_frame1 텍스트 타이머
        if (eventFrame1TextTimer > 0f)
        {
            eventFrame1TextTimer += Time.deltaTime;
            if (eventFrame1TextTimer >= eventFrameTextDuration)
            {
                eventFrame1TextTimer = 0f;
                // 3초 후 페이드 및 이동 실행
                ExecuteFadeAndMove("event_frame1");
            }
        }

        // event_frame2 텍스트 타이머
        if (eventFrame2TextTimer > 0f)
        {
            eventFrame2TextTimer += Time.deltaTime;
            if (eventFrame2TextTimer >= eventFrameTextDuration)
            {
                eventFrame2TextTimer = 0f;
                // 3초 후 페이드 및 이동 실행
                ExecuteFadeAndMove("event_frame2");
            }
        }

        // event_frame3 텍스트 타이머
        if (eventFrame3TextTimer > 0f)
        {
            eventFrame3TextTimer += Time.deltaTime;
            if (eventFrame3TextTimer >= eventFrameTextDuration)
            {
                eventFrame3TextTimer = 0f;
                // 3초 후 페이드 및 이동 실행
                ExecuteFadeAndMove("event_frame3");
            }
        }
    }

    // frame과의 거리 체크 및 이벤트 발생
    private void CheckFrameDistance(float distance, string frameName, ref bool eventActive)
    {
        if (distance <= frameEventDistance && !eventActive)
        {
            // frame 이벤트 발생
            eventActive = true;
            TriggerFrameEvent(frameName, distance);
        }
    }

    // event_frame과의 거리 체크 및 이벤트 발생
    private void CheckEventFrameDistance(float distance, string eventFrameName, ref bool eventActive)
    {
        if (distance <= eventFrameEventDistance && !eventActive)
        {
            // event_frame 이벤트 발생
            eventActive = true;
            TriggerEventFrameEvent(eventFrameName, distance);
        }
    }

    // frame 이벤트 처리 메서드
    private void TriggerFrameEvent(string frameName, float distance)
    {
        Debug.Log($"{frameName} 이벤트 발생! 거리: {distance:F2}");
        
        // JSON 파일에서 가이드 텍스트 읽어오기
        string guideText = LoadGuideTextFromJson(frameName);
        
        // robot_text에 텍스트 표시
        if (robot_text != null && !string.IsNullOrEmpty(guideText))
        {
            robot_text.text = guideText;
        }

        // 해당하는 오디오 재생
        PlayFrameAudio(frameName);
        
        // 여기에 원하는 이벤트 로직을 추가하세요
        // 예: UI 표시, 사운드 재생, 애니메이션 실행 등
    }

    // frame 오디오 재생 메서드
    private void PlayFrameAudio(string frameName)
    {
        AudioSource targetAudio = null;
        
        switch (frameName)
        {
            case "frame1":
                targetAudio = frame1Audio;
                break;
            case "frame2":
                targetAudio = frame2Audio;
                break;
            case "frame3":
                targetAudio = frame3Audio;
                break;
        }
        
        if (targetAudio != null && targetAudio.clip != null)
        {
            targetAudio.Play();
            Debug.Log($"{frameName} 오디오 재생 시작");
        }
        else
        {
            Debug.LogWarning($"{frameName} 오디오 소스가 설정되지 않았습니다.");
        }
    }

    // JSON 파일에서 가이드 텍스트를 읽어오는 메서드
    private string LoadGuideTextFromJson(string frameName)
    {
        try
        {
            // frame 이름에서 번호 추출 (frame1 -> 1)
            string frameNumber = frameName.Replace("frame", "");
            string jsonFileName = $"guide{frameNumber}.json";
            
            // StreamingAssets 경로 구성
            string jsonPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
            
            // 파일이 존재하는지 확인
            if (File.Exists(jsonPath))
            {
                // JSON 파일 읽기
                string jsonContent = File.ReadAllText(jsonPath);
                
                // JSON 파싱
                GuideData guideData = JsonUtility.FromJson<GuideData>(jsonContent);
                
                if (guideData != null)
                {
                    return guideData.guide_text;
                }
                else
                {
                    Debug.LogError($"JSON 파싱 실패: {jsonFileName}");
                    return "가이드 텍스트를 불러올 수 없습니다.";
                }
            }
            else
            {
                Debug.LogError($"JSON 파일을 찾을 수 없습니다: {jsonPath}");
                return "가이드 파일을 찾을 수 없습니다.";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON 파일 읽기 오류: {e.Message}");
            return "가이드 텍스트 로드 중 오류가 발생했습니다.";
        }
    }

    // event_frame 이벤트 처리 메서드
    private void TriggerEventFrameEvent(string eventFrameName, float distance)
    {
        Debug.Log($"{eventFrameName} 이벤트 발생! 거리: {distance:F2}");
        
        // JSON 파일에서 가이드 텍스트 읽어오기
        string guideText = LoadEventGuideTextFromJson(eventFrameName);
        
        // robot_text에 텍스트 표시
        if (robot_text != null && !string.IsNullOrEmpty(guideText))
        {
            robot_text.text = guideText;
        }
        
        // 해당하는 오디오 재생
        PlayEventFrameAudio(eventFrameName);

        // 텍스트 표시 타이머 시작 (3초 후 페이드 및 이동)
        StartEventFrameTextTimer(eventFrameName);
        
        // 여기에 원하는 이벤트 로직을 추가하세요
        // 예: 특별한 이벤트 UI 표시, 효과음 재생 등
    }

    // event_frame 텍스트 타이머 시작
    private void StartEventFrameTextTimer(string eventFrameName)
    {
        switch (eventFrameName)
        {
            case "event_frame1":
                eventFrame1TextTimer = 0f;
                break;
            case "event_frame2":
                eventFrame2TextTimer = 0f;
                break;
            case "event_frame3":
                eventFrame3TextTimer = 0f;
                break;
        }
    }

    // 페이드 및 이동 실행 메서드
    private void ExecuteFadeAndMove(string eventFrameName)
    {
        Vector3 destination = Vector3.zero;
        
        switch (eventFrameName)
        {
            case "event_frame1":
                destination = eventFrame1Destination;
                break;
            case "event_frame2":
                destination = eventFrame2Destination;
                break;
            case "event_frame3":
                destination = eventFrame3Destination;
                break;
        }
        
        // 커스텀 페이드 시퀀스 실행
        StartCoroutine(CustomFadeSequence(destination, eventFrameName));
    }

    // 커스텀 페이드 시퀀스
    private IEnumerator CustomFadeSequence(Vector3 destination, string eventFrameName)
    {
        Debug.Log($"{eventFrameName} 페이드 시퀀스 시작");
        
        // 1. 페이드 아웃 (화면을 어둡게)
        if (FadeManager.Instance != null && FadeManager.Instance.fadeAnimator != null)
        {
            FadeManager.Instance.fadeAnimator.SetTrigger("fadeOut");
            Debug.Log("페이드 아웃 시작");
        }
        
        // 2. 페이드 아웃 애니메이션 완료까지 대기 (약 2.5초)
        yield return new WaitForSeconds(2.5f);
        
        // 3. Player 위치 이동 (화면이 완전히 어두워진 상태에서)
        if (Player != null)
        {
            Player.transform.position = destination;
            Debug.Log($"Player를 {destination}로 이동 완료");
        }
        
        // 4. 잠시 대기 (위치 이동이 완료된 후)
        yield return new WaitForSeconds(0.5f);
        
        // 5. 페이드 인 (화면을 밝게)
        if (FadeManager.Instance != null && FadeManager.Instance.fadeAnimator != null)
        {
            FadeManager.Instance.fadeAnimator.SetTrigger("fadeIn");
            Debug.Log("페이드 인 시작");
        }
        
        Debug.Log($"{eventFrameName} 페이드 시퀀스 완료");
    }

    // event_frame 오디오 재생 메서드
    private void PlayEventFrameAudio(string eventFrameName)
    {
        AudioSource targetAudio = null;
        
        switch (eventFrameName)
        {
            case "event_frame1":
                targetAudio = eventFrame1Audio;
                break;
            case "event_frame2":
                targetAudio = eventFrame2Audio;
                break;
            case "event_frame3":
                targetAudio = eventFrame3Audio;
                break;
        }
        
        if (targetAudio != null && targetAudio.clip != null)
        {
            targetAudio.Play();
            Debug.Log($"{eventFrameName} 오디오 재생 시작");
        }
        else
        {
            Debug.LogWarning($"{eventFrameName} 오디오 소스가 설정되지 않았습니다.");
        }
    }

    // JSON 파일에서 event_frame 가이드 텍스트를 읽어오는 메서드
    private string LoadEventGuideTextFromJson(string eventFrameName)
    {
        try
        {
            // event_frame 이름에서 번호 추출 (event_frame1 -> 1)
            string eventFrameNumber = eventFrameName.Replace("event_frame", "");
            string jsonFileName = $"event_guide{eventFrameNumber}.json";
            
            // StreamingAssets 경로 구성
            string jsonPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
            
            // 파일이 존재하는지 확인
            if (File.Exists(jsonPath))
            {
                // JSON 파일 읽기
                string jsonContent = File.ReadAllText(jsonPath);
                
                // JSON 파싱
                GuideData guideData = JsonUtility.FromJson<GuideData>(jsonContent);
                
                if (guideData != null)
                {
                    return guideData.guide_text;
                }
                else
                {
                    Debug.LogError($"JSON 파싱 실패: {jsonFileName}");
                    return "가이드 텍스트를 불러올 수 없습니다.";
                }
            }
            else
            {
                Debug.LogError($"JSON 파일을 찾을 수 없습니다: {jsonPath}");
                return "가이드 파일을 찾을 수 없습니다.";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON 파일 읽기 오류: {e.Message}");
            return "가이드 텍스트 로드 중 오류가 발생했습니다.";
        }
    }
}
