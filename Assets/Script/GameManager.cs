using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI goalText;
    public Vector3 cornerPosition = new Vector3(-400, 200, 0); // Adjust based on your screen
    public float animationSpeed = 5f;

    public void ShowGoalPopUp(string message)
    {
        goalText.text = message;
        StartCoroutine(AnimateGoal());
    }

    IEnumerator AnimateGoal()
    {
        // 1. POP UP (Scale from 0 to 1.2 for a little bounce)
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            goalText.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(1.2f, 1.2f, 1.2f), t);
            yield return null;
        }

        // Wait for a second so the player can read it
        yield return new WaitForSeconds(1f);

        // 2. MOVE TO CORNER & SHRINK
        Vector3 startPos = goalText.transform.localPosition;
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * (animationSpeed * 0.5f);
            // Move position
            goalText.transform.localPosition = Vector3.Lerp(startPos, cornerPosition, t);
            // Shrink to normal size (1.0)
            goalText.transform.localScale = Vector3.Lerp(new Vector3(1.2f, 1.2f, 1.2f), Vector3.one, t);
            yield return null;
        }
    }
}