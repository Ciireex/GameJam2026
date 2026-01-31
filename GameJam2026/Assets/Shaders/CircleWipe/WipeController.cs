using UnityEngine;
using UnityEngine.UI;

public class WipeController : MonoBehaviour
{
    private Animator _animatior;
    private Image _image;
    private readonly int _circleSizeID = Shader.PropertyToID("_CircleSize");

    public float circleSize = 0f;

    private bool isIn;

    private void Awake()
    {
        _animatior = gameObject.GetComponent<Animator>();
        _image = gameObject.GetComponent<Image>();
    }

    private void Update()
    {
        _image.materialForRendering.SetFloat(_circleSizeID, circleSize);
    }

    public void AnimateIn()
    {
        _animatior.SetTrigger("In");
        isIn = true;
    }

    public void AnimateOut()
    {
        _animatior.SetTrigger("Out");
        isIn = false;
    }

    public bool IsIn() 
    {
        return isIn;
    }
}
