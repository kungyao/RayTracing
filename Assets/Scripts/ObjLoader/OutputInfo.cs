using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputInfo : MonoBehaviour
{
    public Vector2 Resolution;
    public string ImageName;
    public string Sort;

    public OutputInfo()
    {
        Resolution = Vector2.zero;
        ImageName = "";
        Sort = "";
    }

    public OutputInfo(Vector2 _resolution, string _imageName, string _sort)
    {
        Resolution = _resolution;
        ImageName = _imageName;
        Sort = _sort;
    }

    public void Init()
    {
        Resolution = Vector2.zero;
        ImageName = "";
        Sort = "";
    }
}