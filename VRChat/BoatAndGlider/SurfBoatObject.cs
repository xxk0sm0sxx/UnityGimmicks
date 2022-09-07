
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

//Note: Boat gimmick for a world
public class SurfBoatObject : UdonSharpBehaviour
{
    public Underwater uw;
    public Collider colliderBase;
    int idx = 0;
    bool isUnderwater = false;
    bool isUnderwaterTemp = false;

    public float floatingForce;

    Vector3 originalPos;
    bool seated;
    public float targetHeight;

    VRCPlayerApi seatedPlayer;
    public bool canDrive;
    public Rigidbody rb;
    [SerializeField] public Vector3 rbOffset;
    public float accelerate;

    public Transform Handle;
    public Transform HandleTurn;
    public float bounce;

    Vector3 currentForce;
    float angle;

    public AudioSource audioSource;
    [UdonSynced] public float movementSpeed;

    public ParticleSystem splashFX;
    public AudioSource splashAudio;

    public VRCStation station;

    void Start()
    {
        canDrive = false;
        this.GetComponent<MeshRenderer>().enabled = true;
    }

    public override void Interact()
    {
        Networking.LocalPlayer.UseAttachedStation();
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        base.OnStationEntered(player);
        seatedPlayer = player;
        this.GetComponent<MeshRenderer>().enabled = false;
        
        if (player.isLocal)
        {
            seated = true;
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            Networking.SetOwner(Networking.LocalPlayer, rb.gameObject);
        }
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        base.OnStationExited(player);
        this.GetComponent<MeshRenderer>().enabled = true;
        seatedPlayer = null;        
        seated = false;
        Handle.localPosition = Vector3.zero;
        Handle.localRotation = Quaternion.identity;
        audioSource.Stop();
    }

    public override void InputJump(bool value, UdonInputEventArgs args)
    {
        station.ExitStation(Networking.LocalPlayer);
    }

    public void SetDrivable( bool v )
    {
        canDrive = false;

        if (v && seated)
        {
            canDrive = v;
        }
    }

    private void FixedUpdate()
    {
		//If not local
        if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject) == false)
        {
            CheckHitWater();
            PlayEffects();
            return;
        }

		//Calculate horizontal move speed
        movementSpeed = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);

        currentForce = Vector3.zero;
        Float();
        UpdateParentPosition();
        if (seated == false)
        {
            rb.AddForce(currentForce);
            return;
        }

		//Calculate boat steering based on handle rotation
        angle = Handle.localRotation.eulerAngles.y;
        if (angle < -180)
            angle += 360;
        if (angle > 180)
            angle -= 360;
        Quaternion rot = Quaternion.Euler(0, angle * 0.5f, 0);
        Vector3 forward = rot * -transform.forward;
        HandleTurn.localRotation = Quaternion.Euler(0, angle, 0);

        if (canDrive)
        {
            float leftInput = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");
            if (Input.GetKey(KeyCode.S))
                leftInput = 1;
            if (leftInput > 0.3)
            {
                currentForce += -forward * leftInput * accelerate;
            }

            float rightInput = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger");
            if (Input.GetKey(KeyCode.W))
                rightInput = 1;

            if (rightInput > 0.3)
                currentForce += forward * rightInput * accelerate;
            }

        }

        rb.AddForce(currentForce);
        PlayEffects();
    }

    public void PlayEffects()
    {
        if (movementSpeed > 0.1f)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            audioSource.pitch = Mathf.Lerp(audioSource.pitch, 1.0f + Mathf.Abs(movementSpeed) / 45.0f, 0.05f);
            audioSource.volume = Mathf.Abs(movementSpeed) / 30.0f;
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private void PlaySplash()
    {
        splashFX.Play();
        splashAudio.pitch = Mathf.Lerp(splashAudio.pitch, 1.0f + Mathf.Abs(rb.velocity.y) / 5.0f, 0.05f);
        splashAudio.volume = Mathf.Abs(rb.velocity.y) / 5.0f;
        splashAudio.Play();
    }

    private void CheckHitWater()
    {
		//Check if boat collided with water
        if (uw.waterObjects[0].bounds.Intersects(colliderBase.bounds))
        {
            if (isUnderwater == false)
            {
                isUnderwater = true;
                PlaySplash();
            }
        }
        if (rb.velocity.y > 0.5f)
        {
            isUnderwater = false;
        }
    }

    private void Float()
    {
		//Floats boat based on water
        if (uw.waterObjects[0].bounds.Intersects(colliderBase.bounds))
        {
            if (isUnderwater == false )
            {
                isUnderwater = true;
                PlaySplash();
            }
            float sqMag = movementSpeed * bounce;

            currentForce += Vector3.up * floatingForce + Vector3.up * sqMag;
        }

        if ( rb.velocity.y > 2.5f )
        {
            isUnderwater = false;
        }
    }

    public void UpdateParentPosition()
    {
        transform.position = rb.transform.position - rbOffset;

        if (angle > 15.0f)
        {
            transform.Rotate(Vector3.up, 30.0f * Time.deltaTime);
        }
        else if (angle < -15.0f)
        {
            transform.Rotate(Vector3.up, -30.0f * Time.deltaTime);
        }
    }
}
