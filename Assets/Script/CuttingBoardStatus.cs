using UnityEngine;

public class CuttingBoardStatus : MonoBehaviour
{
    public bool isOccupied = false;

    public void SetOccupied(bool status)
    {
        isOccupied = status;
        Debug.Log("Cutting Board Occupied: " + status);
    }
}