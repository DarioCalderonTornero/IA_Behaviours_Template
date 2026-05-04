using TMPro;
using UnityEngine;

public class PaintingInteract : MonoBehaviour
{
    private bool stolen = false;
    private bool playerInRange = false;

    public static TextMeshProUGUI pressSpaceText;

    public TextMeshProUGUI pressSpaceTextRef;

    private void Awake()
    {
        pressSpaceText = pressSpaceTextRef;
        pressSpaceText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && !stolen && Input.GetKeyDown(KeyCode.Space))
            Steal();
    }

    private void Steal()
    {
        stolen = true;
        pressSpaceText.gameObject.SetActive(false);
        GameManager.Instance.PaintingStolen();
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            pressSpaceText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            pressSpaceText.gameObject.SetActive(false);
        }
    }
}