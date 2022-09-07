
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DriftCar : UdonSharpBehaviour
{
    public bool isHandled; //Local player is driving the car
    public bool isHandledByOther; //Some other player is driving the car
    public float accelerate;
    public float brake;
    public float steerAngle;

    public bool goToReverse;

    public Rigidbody rb;
    [SerializeField] public float power;
    public GameObject centerOfMass;

    [UdonSynced] public float Speed;

    public WheelCollider FL, FR, BL, BR;
    public GameObject FLT, FRT, BLT, BRT;

    public AudioSource source;
    public AudioClip starting;
    public AudioClip rolling;
    public AudioClip stopping;
    public AudioClip skidding;
    public AudioSource skidSource;
    public AudioSource miscSource;

    [UdonSynced] public float SidewaysSlip;
    public ParticleSystem ps;

    public bool isDrift = false;

    void Start()
    {
        isHandled = false;
        isHandledByOther = false;
        accelerate = 0;
        brake = 0;
        rb.centerOfMass = centerOfMass.transform.localPosition;
        goToReverse = false;
        skidSource.clip = skidding;
        miscSource.loop = false;
        BL.brakeTorque = power * 3;
        BR.brakeTorque = power * 3;
        isDrift = false;
    }

    public void Handle() //Start vehicle for local player
    {
        isHandled = true;
        isHandledByOther = false;
        miscSource.clip = starting;
        miscSource.Play();

        source.pitch = 0.2f;
        ps.Play();
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
    }

    public void HandleByOther() //Start vehicle for remote player
    {
        isHandledByOther = true;
        isHandled = false;
        miscSource.clip = starting;
        miscSource.Play();

        source.pitch = 0.2f;
        ps.Play();
    }

    public void Stop()
    {
        isHandled = false;
        isHandledByOther = false;
        miscSource.clip = stopping;
        miscSource.Play();

        source.pitch = 0.2f;
        source.Stop();
        ps.Stop();
        BL.brakeTorque = power * 3;
        BR.brakeTorque = power * 3;
    }

    private void FixedUpdate()
    {
        if (isHandledByOther) //Handled by remote player
        {  //Play networked sounds
            if (!source.isPlaying)
            {
                source.clip = rolling;
                source.Play();
            }

            if (source.clip == rolling)
            {
                source.pitch = Mathf.Lerp(source.pitch, 0.2f + Mathf.Abs(Speed) / 30.0f, 0.05f);
            }
            
            if (!skidSource.isPlaying && SidewaysSlip > 0.1f)
            {
                skidSource.Play();
            }
            if (skidSource.isPlaying)
            {
                skidSource.volume = SidewaysSlip * 2;
                if (SidewaysSlip < 0.1f)
                {
                    skidSource.Stop();
                }
            }

            return;
        }

        if (isHandled) //Handled by local player
        { //Adjust steering based on speed
            float steerFactor = (((50.0f - Speed) / 20.0f) * 0.5f) + 0.5f;
            steerFactor = Mathf.Max(Mathf.Min(steerFactor, 1.0f), 0.5f);
            float adjustedSteerAngle = steerAngle;
            if (Networking.LocalPlayer.IsUserInVR() == true)
            {
                adjustedSteerAngle *= steerFactor;
            }
            FL.steerAngle = adjustedSteerAngle;
            FR.steerAngle = adjustedSteerAngle;

			//Drifting stuff
            Speed = rb.velocity.magnitude;
            Vector3 driftForce = -rb.transform.right;
            driftForce.y = 0.0f;
            driftForce.Normalize();

            driftForce *= rb.mass * adjustedSteerAngle * 0.5f;
            Vector3 driftTorque = rb.transform.up * 0.1f * adjustedSteerAngle * 0.5f;

            rb.AddForce(driftForce * Speed * 0.001f, ForceMode.Force);
            rb.AddTorque(driftTorque * Speed * 0.001f, ForceMode.VelocityChange);
            rb.AddForce(-rb.transform.up * Speed * 5);

            if (SidewaysSlip < 1.0f) //Shut off acceleration if slip exceeds certain amount so car won't spin
            {
                rb.AddForce(rb.transform.forward * accelerate * 100);
                BL.motorTorque = power * accelerate;
                BR.motorTorque = power * accelerate;
            }
            else
            {
                BL.motorTorque = 0;
                BR.motorTorque = 0;
            }

            if (brake > 0 && Speed < 0.1f)
            {
                goToReverse = true;
            }

            if (accelerate > 0)
            {
                goToReverse = false;
            }

            if (goToReverse == false)
            {
                rb.AddForce(-rb.transform.forward * brake * 1000);
                BL.brakeTorque = power * 3 * brake;
                BR.brakeTorque = power * 3 * brake;
            }
            else
            {
                if (brake > 0)
                {
                    rb.AddForce(-rb.transform.forward * brake * 100);
                    BL.motorTorque = power * -brake;
                    BR.motorTorque = power * -brake;

                    BL.brakeTorque = 0;
                    BR.brakeTorque = 0;
                }
                else
                {
                    BL.motorTorque = 0;
                    BR.motorTorque = 0;
                    BL.brakeTorque = power * 3;
                    BR.brakeTorque = power * 3;
                }
            }

            if (SidewaysSlip < 0.5f) //Default slipiness
            {
                isDrift = false;
                //float speedSlippiness = Mathf.Min((Speed / 50.0f), 1.0f);
                float rateOfSteering = Mathf.Max(Mathf.Min(((Mathf.Abs(steerAngle) - 5.0f) / 15.0f), 1.0f), 0);
                float stiffnessReduction = 2.0f - (rateOfSteering * 1.0f);
                var curve = BL.forwardFriction;
                curve.stiffness = 2.0f;
                BL.forwardFriction = curve;
                BR.forwardFriction = curve;

                curve = BL.sidewaysFriction;
                curve.stiffness = stiffnessReduction;
                BL.sidewaysFriction = curve;
                BR.sidewaysFriction = curve;

                curve = FL.forwardFriction;
                curve.stiffness = 2.0f;
                FL.forwardFriction = curve;
                FR.forwardFriction = curve;

                curve = FL.sidewaysFriction;
                curve.stiffness = stiffnessReduction;
                FL.sidewaysFriction = curve;
                FR.sidewaysFriction = curve;
            }
            else if (isDrift == false && SidewaysSlip < 0.8f) //Car is in drift mode, lower slip
            {
                var curve = BL.forwardFriction;
                curve.stiffness = 1.0f;
                BL.forwardFriction = curve;
                BR.forwardFriction = curve;

                curve = BL.sidewaysFriction;
                curve.stiffness = 1.0f;
                BL.sidewaysFriction = curve;
                BR.sidewaysFriction = curve;

                curve = FL.forwardFriction;
                curve.stiffness = 1.0f;
                FL.forwardFriction = curve;
                FR.forwardFriction = curve;

                curve = FL.sidewaysFriction;
                curve.stiffness = 1.0f;
                FL.sidewaysFriction = curve;
                FR.sidewaysFriction = curve;
            }
            else //Slip exceeds 0.8f if countersteering, make tires grip hard
            {
                isDrift = true;
                var curve = BL.forwardFriction;
                curve.stiffness = 3.0f;
                BL.forwardFriction = curve;
                BR.forwardFriction = curve;

                curve = BL.sidewaysFriction;
                curve.stiffness = 3.0f;
                BL.sidewaysFriction = curve;
                BR.sidewaysFriction = curve;

                curve = FL.forwardFriction;
                curve.stiffness = 3.0f;
                FL.forwardFriction = curve;
                FR.forwardFriction = curve;

                curve = FL.sidewaysFriction;
                curve.stiffness = 3.0f;
                FL.sidewaysFriction = curve;
                FR.sidewaysFriction = curve;
            }

            Vector3 wheelPos;
            Quaternion wheelRot;
            FL.GetWorldPose(out wheelPos, out wheelRot);
            FLT.transform.position = wheelPos;
            FLT.transform.rotation = wheelRot;

            FR.GetWorldPose(out wheelPos, out wheelRot);
            FRT.transform.position = wheelPos;
            FRT.transform.rotation = wheelRot;

            BL.GetWorldPose(out wheelPos, out wheelRot);
            BLT.transform.position = wheelPos;
            BLT.transform.rotation = wheelRot;

            BR.GetWorldPose(out wheelPos, out wheelRot);
            BRT.transform.position = wheelPos;
            BRT.transform.rotation = wheelRot;

			//Drift audio
            if (!source.isPlaying)
            {
                source.clip = rolling;
                source.Play();
            }

            if (source.clip == rolling)
            {
                source.pitch = Mathf.Lerp(source.pitch, 0.2f + Mathf.Abs(Speed) / 30.0f, 0.05f);
            }

			//Get slip from one of the wheel
            WheelHit wheelHit;
            BL.GetGroundHit(out wheelHit);
            SidewaysSlip = Mathf.Abs(wheelHit.sidewaysSlip);

            if (!skidSource.isPlaying && SidewaysSlip > 0.1f)
            {
                skidSource.Play();
            }
            if (skidSource.isPlaying)
            {
                skidSource.volume = SidewaysSlip * 2;
                if (SidewaysSlip < 0.1f)
                {
                    skidSource.Stop();
                }
            }
        }
    }
}
