using UnityEngine;

public class PotatoSlicer : MonoBehaviour
{
    public Sprite slicedPotatoSprite;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isSliced = false;

    [HideInInspector] public bool isOnBoard = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isSliced && other.CompareTag("Knife") && isOnBoard)
        {
            Animator knifeAnim = other.GetComponent<Animator>();
            if (knifeAnim != null)
            {
                knifeAnim.SetTrigger("doChop");
            }

            SlicePotato(other.gameObject);
        }
    }

    void SlicePotato(GameObject triggeredBy)
    {
        isSliced = true;
        this.gameObject.tag = "CutIngredient";

        // FIX: We no longer call NotifyBoardVacant here.
        // The board stays occupied because the object is still sitting on it.

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && slicedPotatoSprite != null)
        {
            spriteRenderer.sprite = slicedPotatoSprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = 5;
        }

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.Play();
        }

        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.ShowGoalPopUp(Localization.Get("VegetableSliced"));
        }

        TutorialController tutorial = TutorialController.Instance;
        if (tutorial != null && triggeredBy != null) tutorial.ProceedNextStep(triggeredBy);
    }

    // This stays as a safety measure in case the object is deleted
    private void OnDestroy()
    {
        if (isOnBoard)
        {
            GameObject boardObj = GameObject.Find("CuttingBoard");
            if (boardObj != null)
            {
                CuttingBoardStatus board = boardObj.GetComponent<CuttingBoardStatus>();
                if (board != null) board.SetOccupied(false);
            }
        }
    }
}