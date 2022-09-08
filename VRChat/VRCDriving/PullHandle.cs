
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class PullHandle : UdonSharpBehaviour
{
    Vector3 originalLocalPos; //The starting world position of the pickup (Same position as the handle but not it's child)
    public Vector3 prevPos; //Previous frame position
    public GameObject pivot; //The pivot object
    public GameObject handle; //The local position of the handle. This is a child of the turning object (Handle of the door)
    public GameObject turnObject; //The object to turn, like a door.
    public Quaternion diff;
    bool isPickedUp;
    public bool rotX; //Which direction should this object turn
    public bool rotY;
    public bool rotZ;

    [UdonSynced] public float currAngle = 0;
    public float minAngle = 0;
    public float maxAngle = 90;

    public AudioSource source;
    public AudioClip closing;

    void Start()
    {
        originalLocalPos = transform.localPosition;
        isPickedUp = false;
        currAngle = 0;
        source.clip = closing;
    }

    public override void OnPickup()
    {
        base.OnPickup();
        isPickedUp = true;

        Vector3 currLocalPos = transform.localPosition; //Stores local position to lock x,y or z axis
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
        prevPos = currLocalPos;

        Networking.SetOwner(Networking.LocalPlayer, turnObject);
    }

    public override void OnDrop()
    {
        base.OnPickupUseUp();
        transform.position = handle.transform.position; //Snaps pickup back to handle position
        transform.localRotation = Quaternion.identity;
        isPickedUp = false;
    }

    private void FixedUpdate()
    {
		//If owner is remote, do networking
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

        if (isPickedUp) //Is locally controlled
        {
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
			//First, lock down axis that should not change
            transform.localPosition = currLocalPos;

			//Find the angle difference between the previous frame and current frame
            transform.rotation = Quaternion.identity;
            Vector3 Dir1 = prevPos - pivot.transform.localPosition;
            Vector3 Dir2 = currLocalPos - pivot.transform.localPosition;
            diff = Quaternion.FromToRotation(Dir1, Dir2);

            prevPos = currLocalPos;

			//Get the difference angle
            float rotAngle = 0;
            if (rotX)
            {
                rotAngle = diff.eulerAngles.x;
            }
            else if (rotY)
            {
                rotAngle = diff.eulerAngles.y;
            }
            else if (rotZ)
            {
                rotAngle = diff.eulerAngles.z;
            }

            if (rotAngle > 180)
                rotAngle -= 360;
			//Add difference angle to current angle
            currAngle += rotAngle;

			//Plays a sound if door is closed, also snaps the angle
            if (rotAngle < 0 && currAngle < (minAngle + 3))
            {
                currAngle = minAngle;
                source.Play();
            }

            if (rotAngle > 0 && currAngle > (maxAngle - 3))
            {
                currAngle = maxAngle;
                source.Play();
            }

			//Turns the door object
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
        }
    }
}
