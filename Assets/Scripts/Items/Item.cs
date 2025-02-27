using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private bool autoCollect = true;
    [SerializeField] private float collectDelay = 0.5f;
    
    private float collectTime;
    private bool canCollect;

    private void Start()
    {
        collectTime = Time.time + collectDelay;
        
        // Set the sprite
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer && itemSprite)
        {
            spriteRenderer.sprite = itemSprite;
        }
    }

    private void Update()
    {
        if (!canCollect && Time.time >= collectTime)
        {
            canCollect = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canCollect || !autoCollect) return;
        
        if (other.CompareTag("Player"))
        {
            // TODO: Add to player's inventory
            Debug.Log($"Collected {itemName}");
            Destroy(gameObject);
        }
    }
} 