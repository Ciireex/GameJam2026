using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float health = 100f;

    private bool isMaskOn = true;
    

    private void Start()
    {
        GameInput.Instance.OnChangeMaskAction += GameInput_OnChangeMaskAction;
    }

    private void GameInput_OnChangeMaskAction(object sender, System.EventArgs e)
    {
        Debug.Log("Change Mask Action triggered");
    }

    private void Update()
    {
        HandleMovment();
    }

    private void HandleMovment() 
    {
        Vector2 inputVector = GameInput.Instance.GetMovmentVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, inputVector.y, 0f);

        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
    private bool IsMaskOn()
    {
        return isMaskOn;
    }
}
