using UnityEngine;

public static class VectorUtil
{


    public static (Vector3, float) calculateMovement(Vector3 from, Vector3 to, float speed)
    {
        Vector3 heading = to - from;
        Vector3 movement = heading.normalized * speed;
        return (movement, heading.magnitude / speed);
    }
    
    public static Vector3 calculateDirection(Vector3 from, Vector3 to)
    {
        return (to - from).normalized;
    }
}
