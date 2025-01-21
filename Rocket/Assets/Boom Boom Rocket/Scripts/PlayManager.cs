using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using TMPro;
using UnityEngine.Analytics;


public class PlayManager : MonoBehaviour
{
    public GameObject GameoverPanel;

    public GameObject PlayerObj;

    int currentScore = 0;
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI best;

    public string clickid;
    private StarkAdManager starkAdManager;
    Color BaseColor;
    void Start()
    {
        BaseColor = currentScoreText.color;
        Time.timeScale = 1f;
        currentScoreText.text = currentScore.ToString();
        bestScoreText.text = PlayerPrefs.GetInt("BestScore", 0).ToString();
    }


    void Update()
    {
        currentScore = (int)PlayerObj.transform.position.y / 2;
        currentScoreText.text = currentScore.ToString();

        if (currentScore > PlayerPrefs.GetInt("BestScore", 0))
        {
            PlayerPrefs.SetInt("BestScore", currentScore);
            bestScoreText.text = PlayerPrefs.GetInt("BestScore", 0).ToString();
        }
    }


    public void GameOver()
    {
        StartCoroutine(GameOverCoroutine());
        ShowInterstitialAd("1lcaf5895d5l1293dc",
            () => {
                Debug.LogError("--插屏广告完成--");

            },
            (it, str) => {
                Debug.LogError("Error->" + str);
            });
    }

    public void GameContinue()
    {
        ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {

                    Player.PlayerRespawn();
                    StartCoroutine(GameContinueCoroutine());


                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
        
    }
    IEnumerator GameOverCoroutine()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        Time.timeScale = 0.005f;
        GameoverPanel.SetActive(true);
        ChangeColorToWhite();

        yield break;
    }
    IEnumerator GameContinueCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1;
        GameoverPanel.SetActive(false);
        BackColor();
        yield break;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackColor()
    {
        currentScoreText.color = BaseColor;
        bestScoreText.color = BaseColor;
        best.color = BaseColor;

    }
    public void ChangeColorToWhite()
    {
        currentScoreText.color = Color.white;
        bestScoreText.color = Color.white;
        best.color = Color.white;
    }
    public void getClickid()
    {
        var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
        if (launchOpt.Query != null)
        {
            foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                if (kv.Value != null)
                {
                    Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                    if (kv.Key.ToString() == "clickid")
                    {
                        clickid = kv.Value.ToString();
                    }
                }
                else
                {
                    Debug.Log(kv.Key + "<-参数-> " + "null ");
                }
        }
    }

    public void apiSend(string eventname, string clickid)
    {
        TTRequest.InnerOptions options = new TTRequest.InnerOptions();
        options.Header["content-type"] = "application/json";
        options.Method = "POST";

        JsonData data1 = new JsonData();

        data1["event_type"] = eventname;
        data1["context"] = new JsonData();
        data1["context"]["ad"] = new JsonData();
        data1["context"]["ad"]["callback"] = clickid;

        Debug.Log("<-data1-> " + data1.ToJson());

        options.Data = data1.ToJson();

        TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
           response => { Debug.Log(response); },
           response => { Debug.Log(response); });
    }


    /// <summary>
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="closeCallBack"></param>
    /// <param name="errorCallBack"></param>
    public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
        }
    }
    /// <summary>
    /// 播放插屏广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="errorCallBack"></param>
    /// <param name="closeCallBack"></param>
    public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
            mInterstitialAd.Load();
            mInterstitialAd.Show();
        }
    }

}
