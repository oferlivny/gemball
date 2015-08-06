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
	public Camera camera;
	private Vector3 sphereInitPos;

	bool gemOn = true;
	// Use this for initialization
	void Start()
    {
		if (gemOn) {
			GemService.Instance.Connect ();
			gem = GemService.Instance.getGem ();
		}
		sphereInitPos = Sphere.transform.position; 
		rb = Sphere.GetComponent<Rigidbody>();
		tapDetected = false;
	}
	
	// Update is called once per frame
	void Update()
    {
        //transform.rotation = gem.rotation;
	//	transform.position += myVec;
		if (gemOn) {
			Vector3 angles = gem.rotation.eulerAngles;
			stateText.text = gem.state.ToString () + gem.acceleration.magnitude.ToString () + gem.acceleration.ToString ()+angles.ToString();
		} else {
			stateText.text = "GemOff";
		}



		bool tapped;
		if (gemOn) { 
			tapped = isTapped (gem.acceleration);
		} else {
			tapped = Input.GetMouseButton (0);
		}
		if (!tapDetected && tapped) {
			if (gemOn) 
				rb.velocity = tapDirection2(gem.acceleration, gem.rotation); //new Vector3 (0, jump_param, 0);
			else 
				rb.velocity = tapDirection (new Vector3(0.5f,-2.0f,0.0f));
			
			if (sound.isPlaying)
				sound.Stop ();
			sound.Play ();
			tapDetected = true;
		} else if (!tapped) {
			tapDetected = false;
		}
		Vector3 cameraOffset = new Vector3 (20, 22, 2);
		Vector3 vec = new Vector3 (1, 0, 1);
		Vector3 cameraScale = Sphere.transform.position;
		cameraScale.Scale (vec);
		Camera.main.transform.position = cameraScale  + cameraOffset;
	}
	
    void FixedUpdate()
    {
		if (Input.GetMouseButton (0)) {
			gem.Calibrate();
			Sphere.transform.position = sphereInitPos;
			rb.velocity = new Vector3(0,0,0);
		}
    }

    void OnApplicationQuit()
    {
        if (gemOn) GemService.Instance.Disconnect();
    }

    //For Android to unbind Gem Service when application not in focus
    void OnApplicationPause(bool paused)
    {
        if (gemOn && Application.platform == RuntimePlatform.Android)
		{
            if (paused)
                GemService.Instance.Disconnect();
            else
                GemService.Instance.Connect();
        }
    }
 
	bool isTapped(Vector3 acc)
	{
		bool magnitudeOk = acc.magnitude > magnitude_thresh;
		bool axisOk = (Mathf.Pow (acc.y, 2) > (Mathf.Pow (acc.x,2) + Mathf.Pow (acc.z,2)));
		bool directionOk = acc.y < 0;
		if (magnitudeOk && axisOk && directionOk) {
			print ("Tap! " + acc.magnitude.ToString() + acc.ToString()); 
			return true;
		}
		return false;
	}

	Vector3 tapDirection(Vector3 acc) {

		float y = jump_param;
		float x = acc.x / acc.y * jump_param;
		float z = acc.z / acc.y * jump_param;
		Vector3 dir = new Vector3(x,y,z);

		return dir;
	}
	Vector3 tapDirection2(Vector3 acc, Quaternion rot) {
		
		//float y = jump_param;
		//float x = acc.x / acc.y * jump_param;
		//float z = acc.z / acc.y * jump_param;
		Vector3 dir = new Vector3(0,jump_param,0);
		
		return rot*dir;
	}
}
