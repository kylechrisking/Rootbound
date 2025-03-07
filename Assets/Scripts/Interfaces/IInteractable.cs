public interface IInteractable
{
    void OnInteractionStart(PlayerController player);
    void OnInteractionEnd(PlayerController player);
    void OnPointerEnter();
    void OnPointerExit();
} 