using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MuseumEventManagement : MonoBehaviour
{
    public static MuseumEventManagement Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [System.Serializable]
    public class ExhibitInfo
    {
        public int eventId;
        public string roomFolder;    // OneRoom, TwoRoom, ThreeRoom
        public string jsonFile;
        public string mp3File;
    }

    // 전시품 리스트 초기화
    public List<ExhibitInfo> exhibitList = new List<ExhibitInfo>()
    {
        // 1번방(OneRoom)
        new ExhibitInfo { eventId = 1001, roomFolder = "OneRoom", jsonFile = "Eojin.json",      mp3File = "Eojin_v.wav" },
        new ExhibitInfo { eventId = 1002, roomFolder = "OneRoom", jsonFile = "Silok.json",      mp3File = "Silok_v.wav" },
        new ExhibitInfo { eventId = 1003, roomFolder = "OneRoom", jsonFile = "Weapon.json",     mp3File = "Weapon_v.wav" },
        new ExhibitInfo { eventId = 1004, roomFolder = "OneRoom", jsonFile = "Mongju.json",     mp3File = "Mongju_v.wav" },
        new ExhibitInfo { eventId = 1005, roomFolder = "OneRoom", jsonFile = "Goryeo.json",     mp3File = "Goryeo_v.wav" },
        new ExhibitInfo { eventId = 1006, roomFolder = "OneRoom", jsonFile = "GukSae.json",     mp3File = "GukSae_v.wav" },

        // 2번방(TwoRoom)
        new ExhibitInfo { eventId = 2001, roomFolder = "TwoRoom", jsonFile = "Potato.json",     mp3File = "Potato_v.wav" },     // 감자 먹는 사람들
        new ExhibitInfo { eventId = 2002, roomFolder = "TwoRoom", jsonFile = "Bandage.json",    mp3File = "Bandage_v.wav" },    // 귀에 붕대를 감은 자화상
        new ExhibitInfo { eventId = 2003, roomFolder = "TwoRoom", jsonFile = "Letter.json",     mp3File = "Letter_v.wav" },     // 반고흐 마지막 편지
        new ExhibitInfo { eventId = 2004, roomFolder = "TwoRoom", jsonFile = "Sketch.json",     mp3File = "Sketch_v.wav" },     // 가족사진 및 스케치 (유년기 스케치)
        new ExhibitInfo { eventId = 2005, roomFolder = "TwoRoom", jsonFile = "Star.json",       mp3File = "Star_v.wav" },       // 별이 빛나는 밤
        new ExhibitInfo { eventId = 2006, roomFolder = "TwoRoom", jsonFile = "Bible.json",      mp3File = "Bible_v.wav" },      // 성경이 있는 정물
        new ExhibitInfo { eventId = 2007, roomFolder = "TwoRoom", jsonFile = "Arle.json",       mp3File = "Arle_v.wav" },       // 아를의 침실
        new ExhibitInfo { eventId = 2008, roomFolder = "TwoRoom", jsonFile = "Sunflower.json",  mp3File = "Sunflower_v.wav" },  // 해바라기

        // 3번방(ThreeRoom)
        new ExhibitInfo { eventId = 3001, roomFolder = "ThreeRoom", jsonFile = "Checklist.json", mp3File = "Checklist_v.wav" },  // 아폴로11호 우주복
        new ExhibitInfo { eventId = 3002, roomFolder = "ThreeRoom", jsonFile = "Persnality.json",    mp3File = "Persnality_v.wav" },     // 달 체크리스트 및 장비
        new ExhibitInfo { eventId = 3003, roomFolder = "ThreeRoom", jsonFile = "SpaceClothes.json",   mp3File = "SpaceClothes_v.wav" },    // 암스트롱 개인소장품
        new ExhibitInfo { eventId = 3004, roomFolder = "ThreeRoom", jsonFile = "Moonrock.json",     mp3File = "Moonrock_v.wav" },      // 문락 달 암석 샘플
        new ExhibitInfo { eventId = 3005, roomFolder = "ThreeRoom", jsonFile = "Footstep.json",     mp3File = "Footstep_v.wav" },      // 달 표면 발자국 석고본
        new ExhibitInfo { eventId = 3006, roomFolder = "ThreeRoom", jsonFile = "X15.json",          mp3File = "X15_v.wav" },           // X-15 로켓플레인
        new ExhibitInfo { eventId = 3007, roomFolder = "ThreeRoom", jsonFile = "Commander.json",    mp3File = "Commander_v.wav" }     //사령선 
    };

    // 전시품 정보 찾기
    public ExhibitInfo GetExhibitInfo(int eventId)
    {
        return exhibitList.Find(e => e.eventId == eventId);
    }

    // 이벤트 트리거 진입 (외부에서 호출)
    public void OnEventTriggered(int eventId)
    {
        var exhibit = GetExhibitInfo(eventId);
        if (exhibit == null)
        {
            Debug.LogWarning($"[MuseumEvent] 등록되지 않은 eventId: {eventId}");
            return;
        }

        // 서브폴더까지 경로 포함
        string jsonPath = Path.Combine(Application.streamingAssetsPath, exhibit.roomFolder, exhibit.jsonFile);
        string mp3Path = Path.Combine(Application.streamingAssetsPath, exhibit.roomFolder, exhibit.mp3File);

        // JSON 읽기
        string jsonText = "";
        if (File.Exists(jsonPath))
        {
            jsonText = File.ReadAllText(jsonPath);
            Debug.Log($"[MuseumEvent] JSON 내용: {jsonText}");
        }
        else
        {
            Debug.LogWarning($"[MuseumEvent] JSON 파일 없음: {jsonPath}");
        }

        // MP3 경로 확인
        if (File.Exists(mp3Path))
        {
            Debug.Log($"[MuseumEvent] MP3 파일 경로: {mp3Path}");
            // 오디오 플레이어 연동 등 사용 가능
        }
        else
        {
            Debug.LogWarning($"[MuseumEvent] MP3 파일 없음: {mp3Path}");
        }
        
        // --- 설명 UI 출력 ---
        if (descriptionUI != null)
        {
            // 로봇 Transform 설정 (UI가 로봇을 따라가도록)
            if (guideRobot != null)
            {
                descriptionUI.robotTransform = guideRobot.transform;
            }
            
            descriptionUI.ShowExhibitDescription(jsonPath);
            descriptionUI.PlayExhibitAudio(mp3Path);
        }

        // 가이드 로봇에게 설명 명령
        if (guideRobot != null)
            guideRobot.StartExplainMode(eventId);
    }

    // 가이드 로봇 연결(에디터에서 할당)
    public Robot_Control guideRobot;

    // --- 설명 UI 연결(에디터에서 할당) ---
    public ExhibitDescriptionUI descriptionUI;
}
