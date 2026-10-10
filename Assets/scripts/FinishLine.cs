using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && GameManager.Instance != null)
            GameManager.Instance.Win();
    }
}
