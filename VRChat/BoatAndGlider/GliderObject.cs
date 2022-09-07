
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

//Note: Glider object for a world
public class GliderObject : UdonSharpBehaviour
{
    public Collider colliderBase;
    int idx = 0;

    Vector3 originalPos;
    bool seated;

    VRCPlayerApi seatedPlayer;
    public bool canDrive;
    public Rigidbody rb;
    [SerializeField] public Vector3 rbOffset;
    public float accelerate;

    public Transform Handle;
    public Transform HandleTurn;

    Vector3 currentForce;
    float zAngle;
    float xAngle;

    public AudioSource audioSource;
    [UdonSynced] public float movementSpeed;

    public ParticleSystem splashFX;
    public AudioSource splashAudio;

    public VRCStation station;

    public float forwardSpeed;
    public float speedIncrease;
    public float wingAlignment;
    public float globalXAngle;
    public Vector3 gravityVec;
    public float gravitySpeed;
    public float slowDownAmt;

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

    public void SetDrivable(bool v)
    {
        canDrive = false;

        if (v && seated)
        {
            canDrive = v;
        }
    }

    private void FixedUpdate()
    {
        if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject) == false)
        {
            PlayEffects();
            return;
        }

        movementSpeed = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);

        currentForce = Vector3.zero;
        UpdateParentPosition();
        if (seated == false)
        {
            rb.AddTorque(currentForce);
            return;
        }

		//Calculate flight angle based on steering
        zAngle = Handle.localRotation.eulerAngles.z;
        if (zAngle < -180)
            zAngle += 360;
        if (zAngle > 180)
            zAngle -= 360;
        xAngle = Handle.localRotation.eulerAngles.x;
        if (xAngle < -180)
            xAngle += 360;
        if (xAngle > 180)
            xAngle -= 360;

        if (Networking.LocalPlayer.IsUserInVR() == false)
        {
            zAngle = 0;
            xAngle = 0;
            if (Input.GetKey(KeyCode.A))
            {
                zAngle = -45.0f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                zAngle = 45.0f;
            }
            if (Input.GetKey(KeyCode.W))
            {
                xAngle = -45.0f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                xAngle = 45.0f;
            }
        }

        Vector3 forward = -transform.forward;

        HandleTurn.localRotation = Quaternion.Euler(xAngle, 0, zAngle);

        if (canDrive)
        {
            RaycastHit hitInfo;
            if (rb.SweepTest(Vector3.down, out hitInfo, 1.0f) == false) //Is glider grounded?
            {   //Glider is in the air, do glider physics
                globalXAngle = HandleTurn.rotation.eulerAngles.x; //Wing angle
                if (globalXAngle < -180)
                    globalXAngle += 360;
                if (globalXAngle > 180)
                    globalXAngle -= 360;

                wingAlignment = globalXAngle / -90.0f; //How flat is the wing
                float adjustedAlignment = wingAlignment + 0.2f; //Magic number

                if (adjustedAlignment > 1)
                    adjustedAlignment = 1;
                if (adjustedAlignment > 0)
                {
                    forwardSpeed += speedIncrease * adjustedAlignment; //If wing is flat, increase speed
                }
                else
                {
                    forwardSpeed += speedIncrease * slowDownAmt * adjustedAlignment; //If wing is not flat, decrease speed
                }
                if (forwardSpeed < 0)
                    forwardSpeed = 0; //Speed do not go negative

                gravityVec = Vector3.down * gravitySpeed * (1.0f - (Mathf.Abs(wingAlignment) * 0.9f)); //Adjust gravity based on wing

                currentForce += forward * forwardSpeed;
                currentForce += gravityVec;
            }
            else //Glider hits ground
            {
                forwardSpeed = 0; //Reduce speed to 0
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            currentForce += Vector3.up * 100.0f;
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

            audioSource.pitch = Mathf.Lerp(audioSource.pitch, 1.0f + Mathf.Abs(movementSpeed) / 300.0f, 1.0f);
            audioSource.volume = Mathf.Abs(movementSpeed) / 300.0f;
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    public void UpdateParentPosition()
    {
        transform.position = rb.transform.position - rbOffset;

        if (zAngle > 5.0f)
        {
            transform.Rotate(Vector3.up, zAngle * Time.deltaTime);
        }
        else if (zAngle < -5.0f)
        {
            transform.Rotate(Vector3.up, zAngle * Time.deltaTime);
        }
        if (xAngle > 5.0f)
        {
            transform.Rotate(Vector3.right, xAngle * Time.deltaTime);
        }
        else if (xAngle < -5.0f)
        {
            transform.Rotate(Vector3.right, xAngle * Time.deltaTime);
        }
    }
}
