
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZZZ;

public class SwitchTimeUI : MonoBehaviour, IUI
{
    [SerializeField] private Sprite corinImage;
    [SerializeField] private Sprite anBiImage;
    [SerializeField] private Sprite BiLiImage;

    [SerializeField] private Image R_Image;
    [SerializeField] private Image L_Image;

    [SerializeField] private TextMeshProUGUI TextMeshPro;
    [field: SerializeField] public float timeLeft { get; set; }

    private int second = 0;
    private float millisecond = 0;
    private int millisecondInt = 0;
    private bool stopTime = false;
    private RectTransform uIPos;
    private Vector3 initPos;
    Vector3 targetPos = new Vector3(0, -640, 0);
    Coroutine coroutine;
    private void Awake()
    {
        uIPos = GetComponent<RectTransform>();

    }
    private void Start()
    {
        initPos = uIPos.anchoredPosition;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateCountDown();
    }


    public void ActiveImage(CharacterNameList LCharacterName, CharacterNameList RCharacterName, float time)
    {
        //先激活
        gameObject.SetActive(true);
        //移动UI
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(MoveUI(targetPos, initPos, 10, false));
        
        //开始计时
        StartCountDown(time);

        R_Image.sprite = MatchImage(RCharacterName);

        L_Image.sprite = MatchImage(LCharacterName);
    }
    /// <summary>
    /// 移除显示
    /// </summary>
    public void UnActive()
    {
        if (this.gameObject.activeSelf == false) { return; }
        Debug.Log("移除UI显示");
     
        //移动UI
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(MoveUI(initPos, targetPos, 8, true));
    }
    IEnumerator MoveUI(Vector3 initPos ,Vector3 targetPos,float Speed,bool canUnActive)
    {
        uIPos.anchoredPosition = initPos;
        Debug.Log("进入协程" + Vector3.Distance(targetPos, uIPos.anchoredPosition));
        while (Vector3.Distance(uIPos.anchoredPosition, targetPos) >1f)
        {
            uIPos.anchoredPosition = Vector3.Lerp(uIPos.anchoredPosition, targetPos, Time.unscaledDeltaTime * Speed);
            yield return null;
        }
        uIPos.anchoredPosition = targetPos;

        if (canUnActive)
        {
            gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 切换头像
    /// </summary>
    /// <param name="characterName"></param>
    /// <returns></returns>
    private Sprite MatchImage(CharacterNameList characterName)
    {
        switch (characterName)
        {
            case CharacterNameList.KeLin:
                return corinImage;
                
            case CharacterNameList.AnBi:
                return anBiImage;

            case CharacterNameList.BiLi:
                return BiLiImage;
        }
        return null;
    }
    /// <summary>
    /// 初始化倒计时
    /// </summary>
    /// <param name="time"></param>
    private void StartCountDown(float time)
    {
        timeLeft = time;
        stopTime = false;
    }
    /// <summary>
    /// 计算计时器
    /// </summary>
    private void UpdateCountDown()
    {
        if (stopTime) { return; }
        //计算秒
        timeLeft -= Time.unscaledDeltaTime;
        second = Mathf.FloorToInt(timeLeft);
        //计算毫秒
        millisecond = (timeLeft - second) * 100;
        millisecondInt = Mathf.FloorToInt(millisecond);

        TextMeshPro.text = "00:" + Mathf.FloorToInt(second).ToString("00") + ":" + millisecondInt.ToString("00");

        if (second == 0 && millisecondInt == 0)
        {
            TextMeshPro.text = "00:" + Mathf.FloorToInt(timeLeft).ToString("00") + ":00";
            stopTime = true;
        }
    }
}
