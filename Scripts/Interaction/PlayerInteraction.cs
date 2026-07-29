using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 5f;
    public LayerMask interactionLayer;

    private Camera playerCamera;
    private InteractableObject interactableObject;

    private void Start()
    {
        playerCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (ViewManager.Instance.GetCurrentMode() == ViewMode.Panopticon)
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionRange, interactionLayer))
            {
                interactableObject = hit.collider.GetComponent<InteractableObject>();
                interactableObject.SetHighlight(true);
                if (Input.GetMouseButtonDown(0))
                    interactableObject.Interact();
            }
            else if (interactableObject != null)
            {
                interactableObject.SetHighlight(false);
                interactableObject = null;
            }
        }
    }
}
