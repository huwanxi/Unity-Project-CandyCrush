using UnityEngine;

public class CubeView : MonoBehaviour
{
    public CubeController Controller { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }

    public void Init(CubeController controller)
    {
        Controller = controller;
        UpdateCoordinates();
    }

    public void UpdateCoordinates()
    {
        if (Controller != null)
        {
            X = Controller.X;
            Y = Controller.Y;
        }
    }
}
