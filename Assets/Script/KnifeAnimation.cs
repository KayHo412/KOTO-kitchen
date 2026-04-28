using UnityEngine;
using System.Collections;

public class KnifeAnimation : MonoBehaviour
{
    private Vector3 originalPosition;
    public float chopDistance = 30f; // How far down it goes
    public float chopSpeed = 0.1f;    // How fast the motion is

    public void PlayChop()
    {
        StopAllCoroutines();
        StartCoroutine(ChopRoutine());
    }

    IEnumerator ChopRoutine()
    {
        originalPosition = transform.localPosition;

        // Move Down
        transform.localPosition = originalPosition + new Vector3(0, -chopDistance, 0);
        yield return new WaitForSeconds(chopSpeed);

        // Move Back Up
        transform.localPosition = originalPosition;
    }
}