using UnityEngine;
using System.Collections;
#if UNITY_WINRT
using File = UnityEngine.Windows.File;
#else
using File = System.IO.File;
#endif

using UnityEngine.UI;

public class StartMenu : MonoBehaviour {

	GameObject oldScene;
	GameObject newScene;

	public bool transitionFinished;
	public bool back = false;
	
	void Awake() {
		Instantiate(Resources.Load("Scenes/Menu/MainMenuObject"));
		transitionFinished = true;
	}

	public void ChangeScenes(string oldScene, string newScene, bool back) {

		transitionFinished = false;

		this.oldScene = GameObject.Find (oldScene);
		this.newScene = Resources.Load (newScene) as GameObject;
		this.back = back;

		StartCoroutine (TakePicture ());

	}

	private IEnumerator TakePicture() {

		bool error = false;

		try {
			File.Delete (Application.persistentDataPath + "/menuScreenshot.png");
			Application.CaptureScreenshot(Application.persistentDataPath + "/menuScreenshot.png");
		} catch (System.IO.IOException e) {
			error = true;
			Debug.Log("Screenshot saving failed! - " + e.ToString());
		}

        Texture2D tex = null;
        if (!error)
        {
            //Load screenshot in try block, File actions should be always in try-catch.
            try
            {
                tex = null;
                byte[] fileData = File.ReadAllBytes(Application.persistentDataPath + "/menuScreenshot.png");
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
            }
            catch (System.Exception e)
            {
                error = true;
                Debug.Log("Screenshot loading Failed! - " + e.ToString());
            }
        }

		if (!error) {

			while (!System.IO.File.Exists(Application.persistentDataPath + "/menuScreenshot.png")) {
				yield return null;    
			}
			
            // Old: File.ReadAllBytes may be problematic!
			/*Texture2D tex = null;
			byte[] fileData = File.ReadAllBytes (Application.persistentDataPath + "/menuScreenshot.png");
			tex = new Texture2D (2, 2);
			tex.LoadImage (fileData);*/
			
			Sprite sprite = new Sprite ();
			sprite = Sprite.Create (tex, new Rect (0, 0, Screen.width, Screen.height), new Vector2 (0, 0), 100.0f);
			
			GameObject picture = Instantiate (Resources.Load ("Scenes/Menu/MenuPicture")) as GameObject;
			
			GameObject.Find ("MenuPictureImage").GetComponent<Image> ().sprite = sprite;
			
			Destroy (oldScene);
			
			GameObject scene = Instantiate (this.newScene) as GameObject;
			
			if (!back) {
				
				scene.transform.Translate (Vector3.up * Screen.height);
				
				float x = 0;
				
				while (x + Time.deltaTime * 1.6f < 1) {
					yield return 0;
					GameObject.Find ("MenuPicture(Clone)").transform.Translate (Vector3.down * Screen.height * Time.deltaTime * 1.6f);
					scene.transform.Translate (Vector3.down * Screen.height * Time.deltaTime * 1.6f);
					x += Time.deltaTime * 1.6f;
				}
				
				GameObject.Find ("MenuPicture(Clone)").transform.Translate (Vector3.down * Screen.height * (1 - x));
				scene.transform.Translate (Vector3.down * Screen.height * (1 - x));
				
			} else {
				
				scene.transform.Translate (Vector3.down * Screen.height);
				
				float x = 0;
				
				while (x + Time.deltaTime * 1.6f < 1) {
					yield return 0;
					GameObject.Find ("MenuPicture(Clone)").transform.Translate (Vector3.up * Screen.height * Time.deltaTime * 1.6f);
					scene.transform.Translate (Vector3.up * Screen.height * Time.deltaTime * 1.6f);
					x += Time.deltaTime * 1.6f;
				}
				
				GameObject.Find ("MenuPicture(Clone)").transform.Translate (Vector3.up * Screen.height * (1 - x));
				scene.transform.Translate (Vector3.up * Screen.height * (1 - x));
				
			}
			
			Destroy (GameObject.Find ("MenuPicture(Clone)"));
			transitionFinished = true;

		} else {

			Destroy (oldScene);
			GameObject scene = Instantiate(this.newScene) as GameObject;
			
			if (!back) {
				
				scene.transform.Translate (Vector3.up * Screen.height);
				
				float x = 0;
				
				while (x + Time.deltaTime * 1.6f < 1) {
					yield return 0;
					scene.transform.Translate (Vector3.down * Screen.height * Time.deltaTime * 1.6f);
					x += Time.deltaTime * 1.6f;
				}
				
				scene.transform.Translate (Vector3.down * Screen.height * (1 - x));
				
			} else {
				
				scene.transform.Translate (Vector3.down * Screen.height);
				
				float x = 0;
				
				while (x + Time.deltaTime * 1.6f < 1) {
					yield return 0;
					scene.transform.Translate (Vector3.up * Screen.height * Time.deltaTime * 1.6f);
					x += Time.deltaTime * 1.6f;
				}
				
				scene.transform.Translate (Vector3.up * Screen.height * (1 - x));
				
			}
			
			transitionFinished = true;

		}

	}

}
