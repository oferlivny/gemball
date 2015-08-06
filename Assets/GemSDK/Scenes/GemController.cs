using UnityEngine;
using UnityEngine.UI;
using GemSDK.Unity;

public class GemController : MonoBehaviour
{ 
    public Text stateText;
    private IGem gem;
	public GameObject Sphere;
	public Rigidbody rb;
	public AudioSource sound;
	public float jump_param;
	public float magnitude_thresh;
	private bool tapDetected;
	// Use this for initialization
	void Start()
    {
       	GemService.Instance.Connect();
       	gem = GemService.Instance.getGem();
		rb = Sphere.GetComponent<Rigidbody>();
		tapDetected = false;
	}
	
	// Update is called once per frame
	void Update()
    {
        //transform.rotation = gem.rotation;
	//	transform.position += myVec;
        stateText.text = gem.state.ToString() + gem.acceleration.magnitude.ToString();
	}

    void FixedUpdate()
    {
		bool tapped = isTapped (gem.acceleration);
		if (!tapDetected && tapped) {
			rb.velocity = new Vector3 (0, jump_param, 0);
			if (sound.isPlaying)
				sound.Stop ();
			sound.Play ();
			tapDetected = true;
		} else if (!tapped) {
			tapDetected = false;
		}
    }

    void OnApplicationQuit()
    {
        GemService.Instance.Disconnect();
    }

    //For Android to unbind Gem Service when application not in focus
    void OnApplicationPause(bool paused)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (paused)
                GemService.Instance.Disconnect();
            else
                GemService.Instance.Connect();
        }
    }

	bool isTapped(Vector3 acc)
	{
		if (acc.magnitude>magnitude_thresh)
			return true;
		return false;
	}
}
