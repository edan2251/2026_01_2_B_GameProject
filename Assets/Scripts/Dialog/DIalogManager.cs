using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DIalogManager : MonoBehaviour
{
    public static DIalogManager Instance { get; private set; }

    [Header("Dialog Reterences")]
    [SerializeField] private DialogDatabaseSO dialogDatabase;

    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;

    [SerializeField] private Image portraitImage;           //캐릭터 초상화 이미지

    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button NextButton;

    private DialogSO currentDialog;

    [Header("Dialog Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffect = true;

    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if ( Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if(dialogDatabase != null)
        {
            dialogDatabase.Initailize();
        }
        else
        {
            Debug.LogError("Dialog Database is not assinged to DialogManager");
        }

        if(NextButton != null)
        {
            NextButton.onClick.AddListener(NextDialog);
        }
        else
        {
            Debug.LogError("NextButton is Not assigned!");
        }

    }

    void Start()
    {
        //UI초기화 후 대화 시작 (ID 1)
        CloseDialog();  //자동으로 첫번째 대화 시작하기 전에 UI가 보이지 않도록 초기화
        StartDialog(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //ID로 대화 시작
    public void StartDialog(int dialogID)
    {
        DialogSO dialog = dialogDatabase.GetDialogById(dialogID);
        if(dialog != null)
        {
            StartDialog(dialog);
        }
        else
        {
            Debug.LogError($"Dialog with ID {dialogID} not found!");
        }
    }

    //DialogSO로 대화 시작
    public void StartDialog(DialogSO dialog)
    {
        if (dialog == null) return;

        currentDialog = dialog;
        ShowDialog();
        dialogPanel.SetActive(true);
    }

    public void ShowDialog()
    {
        if (currentDialog == null) return;
        characterNameText.text = currentDialog.characterName;       //캐릭터 이름 설정

        if (useTypewriterEffect)
        {
            StartTypingEffect(currentDialog.text);       //타이핑 효과 시작
        }
        else
        {
            dialogText.text = currentDialog.text;       //대화 텍스트 설정
        }

        //초상화 설정
        if (currentDialog.portrait != null)
        {
            portraitImage.sprite = currentDialog.portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else if(!string.IsNullOrEmpty(currentDialog.portraitPath))
        {
            //Resources 폴더에서 초상화 로드 시도
            //(Assets/Resources/Characters/Narrator.png) 예시 이미지 경로
            Sprite portrait = Resources.Load<Sprite>(currentDialog.portraitPath);
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Portrait not found at path: {currentDialog.portraitPath}");
                portraitImage.gameObject.SetActive(false);
            }
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }

    }

    public void CloseDialog()        //대화 종료
    {
        dialogPanel.SetActive(false);
        currentDialog = null;
        StopTypingEffect();
    }

    public void NextDialog()
    {
        if(isTyping)
        {
            StopTypingEffect();
            dialogText.text = currentDialog.text;
            isTyping = false;
            return;
        }

        if(currentDialog != null && currentDialog.nextId > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialogById(currentDialog.nextId);
            if(nextDialog != null)
            {
                currentDialog = nextDialog;
                ShowDialog();
            }
            else
            {
                CloseDialog();
            }
        }
        else
        {
            CloseDialog();
        }
    }

    //텍스트 타이핑 효과 코루틴
    private IEnumerator TypeText(string text)
    {
        dialogText.text = "";
        foreach(char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void StopTypingEffect()
    {
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    //타이핑 효과 함수 시작
    private void StartTypingEffect(string text)
    {
        isTyping = true;
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }
}
