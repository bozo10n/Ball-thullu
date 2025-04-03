using UnityEngine;

public class BallHitEffect : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Color ballColor = GetComponent<Renderer>().material.color;

        // Create hit effect
       //  ChakraController.Instance.CreateBallHitEffect(collision.contacts[0].point, ballColor);
    }
}