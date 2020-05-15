using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public NeuralNetwork net;
    private CharCollider charCollider;
    private float[] input = new float[8];

    private float speed;

    public Transform head, torso, L_Leg, R_Leg, L_upperleg, L_lowerleg, R_upperleg, R_lowerleg;

    public float score; // How far the agent was able to move before time runs out, or falls
    public int stepCount; // How many steps the agent took during the timeframe
    public bool collided; // Checks if the agent falls

    //Angle tracker for last step
    private float lastAT;
    private float lastARK;
    private float lastALK;
    private float lastALH;
    private float lastARH;

    private void Start()
    {
        speed = 50f;
        stepCount = 0;
        lastAT = 0f;
        lastARK = 0f;
        lastALK = 0f; 
        lastALH = 0f;
        lastARH = 0f;
        charCollider = head.GetComponent<CharCollider>();
    }

    private void FixedUpdate()
    {
        // Move forward for as long as the agent doesn't fall over
        if (!collided)
        {
            //Angles of joints
            float angleTorso = torso.transform.rotation.eulerAngles.z;
            float angleLKnee = L_lowerleg.gameObject.GetComponent<HingeJoint2D>().jointAngle;
            float angleRKnee = R_lowerleg.gameObject.GetComponent<HingeJoint2D>().jointAngle;
            float angleLHip = L_upperleg.gameObject.GetComponent<HingeJoint2D>().jointAngle;
            float angleRHip = R_upperleg.gameObject.GetComponent<HingeJoint2D>().jointAngle;

            //AngularVelocity of major bodyparts
            float velTorso = torso.gameObject.GetComponent<Rigidbody2D>().angularVelocity;
            float velLLeg = L_Leg.gameObject.GetComponent<Rigidbody2D>().angularVelocity;
            float velRLeg = R_Leg.gameObject.GetComponent<Rigidbody2D>().angularVelocity;

            //Step Tracker
            if (lastAT != 0 && lastARK != 0 && lastALK != 0 && lastARH != 0 && lastALH != 0)
            {
                float diffARH = Math.Abs(lastARH - angleRHip);
                float diffALH = Math.Abs(lastALH - angleLHip);
                if (diffARH >= 45.0f && diffALH >= 45.0f)
                {
                    stepCount++;
                    Debug.Log("Agent took a step! R:" + diffARH + ", L:" + diffALH);
                }
            }

            input = new float[8] { angleTorso, angleLKnee, angleRKnee, angleLHip, angleRHip, velTorso, velRLeg, velLLeg };

            //Get outputs
            float[] output = net.FeedForward(input);

            //Outputs are going to be the torque of the HingeJoint2D motors
            float tSpeed = output[0];
            float lhSpeed = output[1];
            float rhSpeed = output[2];
            float lkSpeed = output[3];
            float rkSpeed = output[4];

            torso.gameObject.GetComponent<Rigidbody2D>().AddTorque(tSpeed * 20 * Time.fixedDeltaTime);

            JointMotor2D lhMotor = L_upperleg.gameObject.GetComponent<HingeJoint2D>().motor;
            lhMotor.motorSpeed = lhSpeed * speed;
            L_upperleg.gameObject.GetComponent<HingeJoint2D>().motor = lhMotor;

            JointMotor2D rhMotor = R_upperleg.gameObject.GetComponent<HingeJoint2D>().motor;
            rhMotor.motorSpeed = rhSpeed * speed;
            R_upperleg.gameObject.GetComponent<HingeJoint2D>().motor = rhMotor;

            JointMotor2D lkMotor = L_lowerleg.gameObject.GetComponent<HingeJoint2D>().motor;
            lkMotor.motorSpeed = lkSpeed * speed;
            L_lowerleg.gameObject.GetComponent<HingeJoint2D>().motor = lkMotor;

            JointMotor2D rkMotor = R_lowerleg.gameObject.GetComponent<HingeJoint2D>().motor;
            rkMotor.motorSpeed = rkSpeed * speed;
            R_lowerleg.gameObject.GetComponent<HingeJoint2D>().motor = rkMotor;

            score = head.position.x; //Take the position of the agent's head and make that its score

            lastALH = angleLHip;
            lastALK = angleLKnee;
            lastARH = angleRHip;
            lastARK = angleRKnee;
            lastAT = angleTorso;

            if (charCollider.collided)
            {
                this.collided = true;
            }
        }
    }

    /*
     * Look out for when the agent collides with anything that affects its score.
     */
    private void OnCollisionEnter2D(Collision2D collision)
    {

        Debug.Log(collision);
        //Check for if the agent's head hits the ground. T = Death
       
    }

    public void UpdateFitness()
    {
        //Reward agents for taking steps
        //Assign rewards based on the number of times a joint passes a threshold.

        net.fitness = score + (stepCount * 5);

        //Reward agents for keeping their head up :)
        if (!collided)
        {
            net.fitness += 0.7f + (0.2f / head.position.y);
            //Debug.Log("Fitness Improved!");
        }

        else if (stepCount == 0)
        {
            net.fitness -= 2.5f;
        }

        //Penalize for hitting their head
        else
        {
            net.fitness -= 1.8f;
        }
    }
}
