
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Note: Grabable steering wheel with multiple turns supported
public class Steering : UdonSharpBehaviour
{
    Vector3 originalLocalPos;
    Quaternion prevRot;
    public GameObject pivot;
    public GameObject handle;
    public GameObject turnObject;
    public Steering otherHandle;
    public Quaternion diff;
    bool isPickedUp;
    public bool rotX;
    public bool rotY;
    public bool rotZ;

    [UdonSynced] public float currAngle;
    public float minAngle;
    public float maxAngle;

    public GameObject rightHandle;
    public GameObject handlePivot;

    public DriftCar driftCar;

    void Start()
    {
        originalLocalPos = transform.localPosition;
        isPickedUp = false;
    }

    public override void OnPickup()
    {
        base.OnPickup();
        isPickedUp = true;

		//On pickup, save initial steering wheel position to serve as pivot
        Vector3 currLocalPos = transform.localPosition;
        if (rotX)
        {
            currLocalPos.x = originalLocalPos.x;
        }
        else if (rotY)
        {
            currLocalPos.y = originalLocalPos.y;
        }
        else if (rotZ)
        {
            currLocalPos.z = originalLocalPos.z;
        }

        transform.localPosition = currLocalPos;
        //prevPos = currLocalPos;
        prevRot = transform.localRotation;

        Networking.SetOwner(Networking.LocalPlayer, turnObject);
    }

    public override void OnDrop()
    {
        base.OnPickupUseUp();
        transform.position = handle.transform.position;
        transform.localRotation = Quaternion.identity;
        isPickedUp = false;
    }

    private void LateUpdate()
    {
		//If controlled remotely, do networking
        if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject) == false)
        {
            if (rotX)
            {
                turnObject.transform.localRotation = Quaternion.Euler(currAngle, 0, 0);
            }
            else if (rotY)
            {
                turnObject.transform.localRotation = Quaternion.Euler(0, currAngle, 0);
            }
            else if (rotZ)
            {
                turnObject.transform.localRotation = Quaternion.Euler(0, 0, currAngle);
            }

            return;
        }

        if (isPickedUp) //If controlled locally
        {
			//Get rotation based on local z angle
            Quaternion currRot = transform.localRotation;
            float prevZ = prevRot.eulerAngles.z;
            if (prevZ > 180)
                prevZ -= 360;

            float currZ = currRot.eulerAngles.z;
            if (currZ > 180)
                currZ -= 360;
            
			//Get differene angle from previous frame so it can do multiple turns
            float rotAngle = currZ - prevZ;
            prevRot = currRot;
            
            if (rotAngle > 180)
                rotAngle -= 360;
            
            currAngle += rotAngle;

            otherHandle.currAngle = currAngle;
            
			//Lock maximum angle
            if (currAngle < minAngle)
                currAngle = minAngle;

            if (currAngle > maxAngle)
                currAngle = maxAngle;

            float steerAngle = 0;

            if (Networking.LocalPlayer.IsUserInVR() == true)
            {
                if (rotX)
                {
                    turnObject.transform.localRotation = Quaternion.Euler(currAngle, 0, 0);
                }
                else if (rotY)
                {
                    turnObject.transform.localRotation = Quaternion.Euler(0, currAngle, 0);
                }
                else if (rotZ)
                {
                    turnObject.transform.localRotation = Quaternion.Euler(0, 0, currAngle);
                }

                steerAngle = -currAngle * (30.0f / 270.0f);
            }

            if (Networking.LocalPlayer.IsUserInVR() == false)
            {
                if (Input.GetKey(KeyCode.A))
                {
                    steerAngle = -20.0f;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    steerAngle = 20.0f;
                }
            }

            driftCar.steerAngle = steerAngle;

			//This part is for acceleration and braking
            float leftInput = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");
            if (Input.GetKey(KeyCode.S))
                leftInput = 1;
            driftCar.brake = leftInput;

            float rightInput = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger");
            if (Input.GetKey(KeyCode.W))
                rightInput = 1;
            driftCar.accelerate = rightInput;
        }
    }
}
