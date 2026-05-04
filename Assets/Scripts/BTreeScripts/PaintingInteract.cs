using UnityEngine;

public class PaintingInteract : MonoBehaviour
{
    private bool stolen = false;
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && !stolen && Input.GetKeyDown(KeyCode.Space))
            Steal();
    }

    private void Steal()
    {
        Debug.Log("Stolen");
        stolen = true;
        GameManager.Instance.PaintingStolen();
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger detectado: " + other.gameObject.name);
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}