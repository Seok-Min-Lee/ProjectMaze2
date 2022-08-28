using UnityEngine;

public class CircleRotation : MonoBehaviour
{
    public RevolutionAxisType axisType;
    public float rotateSpeed;
    public bool isReverse;

    private Vector3 axisVector;
    
    private void Start()
    {
        Vector3 vector = GetAxisVectorByRevolutionAxisType(type: this.axisType);

        this.axisVector = isReverse ? vector * (-1) : vector;
    }

    private void Update()
    {
        transform.Rotate(axis: this.axisVector, angle: this.rotateSpeed * Time.deltaTime);
    }

    private Vector3 GetAxisVectorByRevolutionAxisType(RevolutionAxisType type)
    {
        Vector3 vector;

        switch (type)
        {
            case RevolutionAxisType.XAxis:
                vector = Vector3.right;
                break;
            case RevolutionAxisType.YAxis:
                vector = Vector3.up;
                break;
            case RevolutionAxisType.ZAxis:
                vector = Vector3.forward;
                break;
            default:
                vector = Vector3.up;
                break;
        }

        return vector;
    }
}
