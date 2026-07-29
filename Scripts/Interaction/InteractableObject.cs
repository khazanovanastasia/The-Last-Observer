using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public Transform cameraViewPoint;
    public Renderer renderer;
    public Material normalMaterial;
    public Material highlightMaterial;

    private bool canInteract = false;

    public void SetHighlight(bool highlight)
    {
        canInteract = highlight;
        renderer.material = highlight ? highlightMaterial : normalMaterial;
    }

    public Vector3 GetCameraPosition()
    {
        return cameraViewPoint.position;
    }

    public Quaternion GetCameraRotation()
    {
        return cameraViewPoint.rotation;
    }

    public virtual void Interact()
    {
        SetHighlight(false);
    }
}
