using UnityEngine;
using UnityEngine.UI;
using GemSDK.Unity;

public class GemController : MonoBehaviour
{ 
    public Text stateText;
    private IGem gem;
	public GameObject Sphere;
	public Rigidbody rb;

	// Use this for initialization
	void Start()
    {
       	GemService.Instance.Connect();
       	gem = GemService.Instance.getGem();
		rb = Sphere.GetComponent<Rigidbody>();
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
		if (isTapped (gem.acceleration))
			rb.velocity = new Vector3 (0,2,0);
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
		if (acc.magnitude>1.1)
			return true;
		return false;
	}
}
