using DevelopEngine;
using System.Collections;
using UnityEngine;

public class Timing : MonoSingleton<Timing> {

    //public Button plamButton;

    float mTime = 5f;
    bool timeBool = false;
    bool jsTime = false;

    private float value;

    //public GameObject vcr;

    public void Awake()
    {
        //value = float.Parse(LoadXml.ReadXml("Within", "time"));
        //UnityEngine.Cursor.visible = bool.Parse(LoadXml.ReadXml("Within", "mouseHidden"));
    }

    public void Start()
    {
        StartButton();
    }
	
	// Update is called once per frame
	void Update () {

        if (mTime <= 0 && jsTime)
        {
            StopCoroutine(Mting());
            timeBool = false;
            jsTime = false;
            //vcr.SetActive(true);
            //vcr.GetComponent<Video>().OnOpenVideoFile(0);
        }

	}

    public void StartButton()
    {
        if (!timeBool)
        {
            mTime = value;
            timeBool = true;
            jsTime = true;
            StartCoroutine(Mting());
        }
    }

    public void Ztime()
    {
        mTime = value;
    }

    IEnumerator Mting()
    {
        while (mTime >= 0)
        {
            mTime -= 1f;
            yield return new WaitForSeconds(1f);
            //print(mTime);
        }        
    }

    public void Stop()
    {
        timeBool = false;
        StopCoroutine(Mting());
    }
}
