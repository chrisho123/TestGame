using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.IO;
#endif

/*
 * 
 *   測試用遊戲
 * 
 */
public class Main : MonoBehaviour
{
	// 每個Frame處理收到封包的次數
	private const int PROCESS_NETWORK_MESSAGE_COUNT = 2;

	#if USE_NGUI
	// 把NGUI的Root指定上去，之後所有UI邏輯由此物件管理
	// NOTE: Root必須有NGUI_Root的Script.
	public NGUI_Root mGUIRoot;
	public bool useNetWorkBase_Bool=false;
	public bool Google_X_Bool = false;

	private static NGUI_Root m_uiRoot;
	public static NGUI_Root g_uiRoot
	{
		get{ return m_uiRoot; }
	}
	#endif
	//Object of Ball
	private GameObject m_BallPrefab;
	//Player
	private GameObject m_PlayerPrefab;
	//Ball number
	private int m_BallNum = 0;
	//Frame Counter
	private int m_FrameCnt = 0;


	private static Main m_instance;
	public static Main getInstance
	{
		get{ return m_instance;}
	}


	void FixedUpdate()
	{

	}

	// Pause
	void OnApplicationPause(bool paused)
	{
#if USE_NGUI
		NGUIDebugPanel.Log("OnApplicationPause(): " + paused);
#endif
		// 在Android系統上某些機器(如HTC One)，當休眠或按下Home鈕回桌面後再進入遊戲，
		// opengl會重新建立，此種情況下，若是自行填入內容的貼圖(例如SysFontTexture_GUI)其內容會遺失，需要重新填入內容
		// 目前還沒有找到方法知道opengl是否有重新建立，所以不管有無重新建立都一律更新貼圖
		if (paused)
		{
			//Debug.Log("OnApplicationPause paused = true");
			
			// 中斷時設定需要更新SysFontTexture_GUI貼圖
			//SysFontTexture_GUI.SetNeedUpdateTexture();
		}
		else
		{
			
			//Debug.Log("OnApplicationPause paused = false");

			// 更新SysFontTexture_GUI貼圖
			//SysFontTexture_GUI.UpdateTextures();
		}

	}

	// On Every Time a Level loaded
	void OnLevelWasLoaded(int index)
	{
		// reset the BGM sound volume by current settings.
		//Settings.ApplyBGMVolume();
		//Settings.ApplySoundFxVolume ();
	}

	void OnEnable()
	{
#if UNITY_ANDROID
	#if GOOGLE_PLAY_IAB
		GoogleIABManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
		GoogleIABManager.purchaseSucceededEvent += purchaseSucceededEvent;
	#endif
#endif
	}

	void OnDisable()
	{
#if UNITY_ANDROID
	#if GOOGLE_PLAY_IAB
		GoogleIABManager.queryInventorySucceededEvent -= queryInventorySucceededEvent;
		GoogleIABManager.purchaseSucceededEvent -= purchaseSucceededEvent;
	#endif
#endif
	}
	
	/**
	initial
	**/
	private IEnumerator Start()
	{



#if !ORI_FIXEDUPDATE
		// 目前FPS限制30，物理計算也限制30
		//Time.fixedDeltaTime = 0.0333f;
#endif



#if !UNITY_EDITOR
		// while VSync==false in Quality settings, 
		// we can use this to lock Framerate, that will lengthen battery life for this game.
		Application.targetFrameRate = 60;
#endif

		// 不進入休眠模式
		Screen.sleepTimeout = SleepTimeout.NeverSleep;


		
		// 一開始沒有攝影機，先清buffer，避免有些機器會有雜訊畫面
		GL.Clear(true, true, Color.black);

		// 設定LoadFromCacheOrDownload的Cache限制大小，目前為2GB
		Caching.maximumAvailableDiskSpace = 2147483648;

#if USE_NGUI
		// 使用NGUI時，會由下面的Loading畫面取代原先流程，在LoadingStartCallback中載入場景
		// 這裡只會先去抓NGUI_Root
		mGUIRoot = GetComponent<NGUI_Root>();
		m_uiRoot = GetComponent<NGUI_Root>();
		m_uiRoot.showGUI(NGUI_Root.eUIType.LOGIN);
		Localization.ReLoad=true;
#endif


		//set this gameObject always on scene 
		DontDestroyOnLoad( gameObject );
		//DontDestroyOnLoadToAllChildren(gameObject);
		//set singleton access
		m_instance = this;
		Debug.Log ("Loading");

		//每隔一陣子發射
		//prefab = (GameObject)UnityEngine.Object.Instantiate (Resources.Load("Sprite/Ball"));
		m_BallPrefab = Resources.Load("Sprite/Ball") as GameObject;
		m_PlayerPrefab = GameObject.Find("Player");

		m_BallNum = 0;
		Debug.Log ("startAAAA");
		GameObject bg_prefab = GameObject.Find ("UI Root");

		UIEventListener.Get(bg_prefab).onDragEnd += OnDragEnd;

		yield return null;
	}


	private void OnDragEnd(GameObject go)
	{
		//Debug.Log("OnDragEnd:" +UICamera.currentTouch.totalDelta);


		//Sparn Enemy
		TweenPosition tweenP = m_PlayerPrefab.GetComponent<TweenPosition> ();
		float tx = m_PlayerPrefab.transform.localPosition.x;
		float ty = m_PlayerPrefab.transform.localPosition.y;

		//Reset to beginning
		tweenP.ResetToBeginning();

		//Set from
		tweenP.from = new Vector3(tx,ty, 0);
		//Set target
		tweenP.duration = 0.05f;
		float dx = UICamera.currentTouch.totalDelta.x;
		float dy = UICamera.currentTouch.totalDelta.y;

		//Check drag direction
		if (Mathf.Abs(dx) > Mathf.Abs(dy)) 
		{
			//Edge check
			if (dx > 0 && tx <285)
				tx += 117;
			if (dx < 0 && tx > -300)
				tx -= 117;
			
		} 
		else 
		{

			if (dy > 0 && ty<244)
				ty += 88;
			if (dy < 0 && ty>-196)
				ty -= 88;			
			
		}

		tweenP.to = new Vector3(tx,ty, 0);
		tweenP.PlayForward();
		EventDelegate.Add (tweenP.onFinished, PlayerMoveFinished);

		

	}

	private void PlayerMoveFinished()
	{

		//Set depth
		UI2DSprite ui2dspr = m_PlayerPrefab.GetComponent<UI2DSprite> ();
		ui2dspr.depth = 10;
		float ty = m_PlayerPrefab.transform.localPosition.y;
		//bottom y
		ty += 196;
		int y = (int)ty; 
		int slot = 6-(y / 88);
		ui2dspr.depth += slot;
	
	}

	private void OnDestroy()
	{
		
	}
	
	private void Update()
	{
		//if (Input.GetKeyDown(KeyCode.N)) 
		if ((m_FrameCnt % 90)==0)
		{
			float duration=Random.Range(3f,5f);
			int count=Random.Range(1 , 5);
			for (int i=0;i<count;i++)
				SpawnEnemy(duration);

		}


		m_FrameCnt++;
	}

	//Spawn enemy from UPLR
	private void SpawnEnemy(float duration)
	{
		//L side x=-410, R side x=430, T side y=320,B side y=-310
		int x=0, y=0, tx=0, ty=0, dir=0, slot=0;
		//Decide L/R/T/B
		int []sp_pos_x = { -410, 430,   0,   0};
		int []sp_pos_y = {    0,   0, 320,-310};
		const int sp_x_distance = 117;
		const int sp_y_distance = 88;
		const int sp_LR_base_y = 226; //
		const int sp_TB_base_x = -298;//

		//Decide slot
		dir = Random.Range(0,3+1); //L=0,R=1,T=2,B=3
		slot = Random.Range(0,5+1); //L=0,R=1,T=2,B=3
		x=sp_pos_x[dir];
		y=sp_pos_y[dir];
		//Left or Right
		if (dir == 0 || dir == 1) 
		{			
			tx = (dir == 0?x+840:x-840);
			y = sp_LR_base_y - slot * sp_y_distance;
			ty = y;
		}
		//Top or Bottom
		if (dir == 2 || dir == 3) 
		{
			ty = (dir == 2?y-630:y+630);
			x = sp_TB_base_x + slot * sp_x_distance;
			tx = x;
		}			
		//Debug.Log ("decide dir :" + dir + ",slot:"+slot);
		//Find empty Enemy prefab, if any, using it or addChild.
		//Spawn Arrow(flash and disapear)
		GameObject prefab;
		m_BallPrefab.name = "Ball" +m_BallNum;
		NGUITools.AddChild(GameObject.Find("UI Root/Camera"),m_BallPrefab);
		m_BallPrefab.name = "Ball";
		//Find out the Ball(Clone)
		prefab = GameObject.Find ("Ball" + m_BallNum + "(Clone)");


		//Set depth
		UI2DSprite ui2dspr = prefab.GetComponent<UI2DSprite> ();

		ui2dspr.depth = 10;
		if (dir == 0 || dir == 1)
			ui2dspr.depth += slot;
		//Sparn Enemy
		TweenPosition tweenP = prefab.GetComponent<TweenPosition> ();

		//Reset to beginning
		tweenP.ResetToBeginning();
		//Set from
		tweenP.from = new Vector3(x,y, 0);
		//Set target
		tweenP.to = new Vector3(tx,ty, 0);
		tweenP.duration = duration;
		//tweenP.delay = 2;
		//Set callback when ending
		EventDelegate.Add (tweenP.onFinished, EnemyMoveFinished);

		tweenP.PlayForward();		
		m_BallNum++;
	}

	private void EnemyMoveFinished()
	{
		string current_name = UITweener.current.name;
		GameObject prefab = GameObject.Find (current_name);
		Destroy(prefab);
		//Debug.Log (current_name +" Fininshed!");
	}



	void QuitAppCallback(object obj, System.EventArgs args)
	{
		Application.Quit ();
	}

	 
	/*

	private static NGUI_SoundManager SoundManager=null;
	/// play UI Sound 
	/// set sound name , and loop model
	public static void PlaySound(string soundname , bool loop=false)
	{
		if (SoundManager==null)
			SoundManager = GameObject.FindObjectOfType<NGUI_SoundManager>();
		if (SoundManager!=null)
		{
			SoundManager.PlaySound(soundname,loop);
		}
	}

	/// Stop the UI Sound 
	public static void SoundStop()
	{
		if (SoundManager==null)
			SoundManager = GameObject.FindObjectOfType<NGUI_SoundManager>();
		if (SoundManager!=null)
			SoundManager.Stop();
	}

	/// Update UI Sound Value from Setting
	public static void UpdateSoundValue(GameObject obj)
	{
		if (SoundManager==null)
			SoundManager = GameObject.FindObjectOfType<NGUI_SoundManager>();
		if (SoundManager!=null)
			SoundManager.SoundValueUpdate(obj);
	}

	/// play the object animation
	public static void GameObjectFxPlay(GameObject obj,bool enable, bool loop)
	{
		if (Main.CheckObject("ObjectAnimationPlay",obj,"NGUI_ButtonFx"))
		{
			NGUI_ButtonFx obj_fx = obj.GetComponent<NGUI_ButtonFx>();
			if (obj_fx!=null)
				obj_fx.FxPlay(enable,loop);
		}
	}
	*/




}