using UnityEngine;
using System.Collections.Generic;
using Ortho4D;

/// <summary>
/// The ScoutBotManager uses the concept of avoindance in order to find the target while avoiding the obstacles in between
/// </summary>
public class AvoidancePathController: MonoBehaviour
{
    #region Fields

    private static AvoidancePathController _instance;

    public static AvoidancePathController Instance { get { return _instance; } }

    private const float UpperSliderY = 1.61f;
    private const float UpperSliderZ = 1.32f;
    private const float BottomSliderY = 0.1f;
    private const float BottomSliderZ = 1f;
    public GameObject Origin;
    public GameObject Target;
    public GameObject NewPoint;
    public GameObject HitPoint;
    public GameObject NewCenter;
    public Material ObjectMaterial;
    public List<Vector3> PathPoints = new List<Vector3>();
    public string ObjectGroupId = "";
    public string ObjectTag = "";
    private GameObject _auxiliaryTouchPointObject, _auxiliaryAvoidancePointObject, _auxiliaryIntermediatePointObject;
    private string _targetParentName, _targetGrandparentName;
    private bool _onSameArch;
    private bool _pathfound;
    private bool _buttonObjectInPath;
    private bool _allowRouteBackOfTooth;
    private bool _targetButtonIsLingual;

    private const int LayerMask = ~(1 << 10);

    #endregion

    #region Unity Operations

    protected void Awake()
    {
        _instance = this;
    }

    /// <summary>
    /// Start method instantiates 3 objects that will be used
    /// to build the path. It will reuse the objects and never
    /// instantiate new ones.
    /// </summary>
    public void Start()
    {
        if (GameObject.Find("TouchPoint") == null)
        {
            _auxiliaryTouchPointObject = Instantiate(HitPoint, Vector3.zero, Quaternion.identity) as GameObject;
            if (_auxiliaryTouchPointObject != null)
            {
                _auxiliaryTouchPointObject.name = "TouchPoint";
                _auxiliaryTouchPointObject.SetActive(false);
            }
        }
        else
            _auxiliaryTouchPointObject.SetActive(false);

        if (GameObject.Find("AvoidPoint") == null)
        {
            _auxiliaryAvoidancePointObject = Instantiate(NewPoint, Vector3.zero, Quaternion.identity) as GameObject;
            if (_auxiliaryAvoidancePointObject != null)
            {
                _auxiliaryAvoidancePointObject.name = "AvoidPoint";
                _auxiliaryAvoidancePointObject.SetActive(false);
            }
        }
        else if (_auxiliaryTouchPointObject != null) _auxiliaryTouchPointObject.SetActive(false);

        if (GameObject.Find("AdditionalPoint") == null)
        {
            _auxiliaryIntermediatePointObject = Instantiate(NewPoint, Vector3.zero, Quaternion.identity) as GameObject;
            if (_auxiliaryIntermediatePointObject != null)
            {
                _auxiliaryIntermediatePointObject.name = "AdditionalPoint";
                _auxiliaryIntermediatePointObject.SetActive(false);
            }
        }
        else if (_auxiliaryTouchPointObject != null) _auxiliaryTouchPointObject.SetActive(false);
    }

    #endregion

    #region Operations

    /// <summary>
    /// Return an array with the points calculated for the new path
    /// </summary>
    public Vector3[] PointsToDraw()
    {
        Vector3[] originalPoints = PathPoints.ToArray();
        Vector3[] interpolatedPoints = MakeSmoothCurve(originalPoints, 3.0f);
        return interpolatedPoints;
    }

    /// <summary>
    /// Main method that executes the algorithm to find the new path
    /// avoiding objects in between
    /// </summary>
    public bool FindPath()
    {
        int maxSteps = 25;
        bool calculatingPath = true;
        bool flagSuccess = false;
        //Debug.Log("origin is: " + Origin.name + ", Target: " + Target.name);
        var mouthAngle = Ortho4DManager.SceneDataStorage.DegreeOpen;

        _allowRouteBackOfTooth = false;
        _buttonObjectInPath = false;

        PathPoints.Clear();

        InitialSettings();

        _targetParentName = Target.transform.parent.name;
        _targetGrandparentName = Target.transform.parent.parent.name;

        Vector3 currentPoint = Origin.transform.position;
        var targetPoint = Target.transform.position;

        _pathfound = false;

        PathPoints.Add(currentPoint);

        if (Origin.transform.parent.parent.tag != "ElasticCut")
        {
            if (GetToothElement(Origin) != null && !_onSameArch)
            {
                if (PointInsideCollider(Origin, currentPoint))
                {
                    if (mouthAngle >= 50)
                    {
                        currentPoint += new Vector3(0f, 0f, -0.35f);
                        PathPoints.Add(currentPoint);
                    }
                    else if (mouthAngle >= 25 && mouthAngle < 50)
                    {
                        currentPoint += new Vector3(0f, 0f, -0.25f);
                        PathPoints.Add(currentPoint);
                    }
                }
            }
        }
        else
        {
            if (_targetGrandparentName.Contains("Lingual") || _targetButtonIsLingual)
                _allowRouteBackOfTooth = true;
        }

        /*----------------------
          Starts the Algorithm
        -----------------------*/
        while (calculatingPath)
        {
            RaycastHit hitPointInformation;
            if (Physics.Linecast(currentPoint, targetPoint, out hitPointInformation, LayerMask))
            {
                GameObject inBetweenObject = hitPointInformation.collider.gameObject;
                if (ObjectHitIsTarget(inBetweenObject, Target))
                {
                    PathPoints.Add(targetPoint);
                    calculatingPath = false;
                    flagSuccess = true;
                }
                else
                {
                    currentPoint = CreateAvoidancePoint(inBetweenObject, hitPointInformation.point, currentPoint);
                }
            }
            else
            {
                //End Reached
                PathPoints.Add(targetPoint);
                calculatingPath = false;
                flagSuccess = true;
            }

            maxSteps--;
            if (maxSteps <= 0)
            {
                calculatingPath = false;
                flagSuccess = false;
                Dbg.Trace("Path not found :(");
            }
        }

        _pathfound = flagSuccess;
        return _pathfound;
    }


    #endregion

    #region Helpers

    /// <summary>
    /// Method to smooth the Line by Interpolating new points
    /// </summary>
    private Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
    {
        if (smoothness < 1.0f) smoothness = 1.0f;

        int pointsLength = arrayToCurve.Length;

        int curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
        List<Vector3> curvedPoints = new List<Vector3>(curvedLength);

        for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            var t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

            var points = new List<Vector3>(arrayToCurve);

            for (int j = pointsLength - 1; j > 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    points[i] = (1 - t) * points[i] + t * points[i + 1];
                }
            }
            curvedPoints.Add(points[0]);
        }
        return (curvedPoints.ToArray());
    }

    /// <summary>
    /// This method evaluates if hit point is the Target
    /// </summary>
    private bool ObjectHitIsTarget(GameObject touchedObject, GameObject targetObject)
    {
        bool isTarget = false;
        if (touchedObject.transform.parent == null || targetObject.transform.parent == null)
            return false;

        if (touchedObject.transform.parent.parent == null)
            return false;
            
        if (targetObject.transform.parent.parent == null)
            return false;


        GameObject parentOfTouchedObject = touchedObject.transform.parent.gameObject;
        GameObject granpaOfTouchedObject = touchedObject.transform.parent.parent.gameObject;
        GameObject parentOfTargetObject = targetObject.transform.parent.gameObject;
        GameObject granpaOfTargetObject = targetObject.transform.parent.parent.gameObject;
        

        /*---------------------------------------------------------------
        First case: elements names are the same but different parents        
        ---------------------------------------------------------------*/
        if (touchedObject.name == targetObject.name)
        {
            if (granpaOfTouchedObject != null && granpaOfTargetObject != null)
            {
                if (granpaOfTouchedObject.name == granpaOfTargetObject.name)
                    isTarget = true;
            }           
        }

        /*---------------------------------------------------------------
        Second case: object hit is "permanent", gets the granpa to see   
        if it's on the same bracket as the target elasticHook       
        ---------------------------------------------------------------*/
        if ((touchedObject.name == "permanent" || touchedObject.tag == "BracketHook") && (granpaOfTouchedObject != null))
        {
            if (granpaOfTouchedObject.name == _targetGrandparentName)
            {
                var selectedObject = parentOfTouchedObject;
                if (selectedObject.transform.Find("ElasticHook").gameObject != null)
                    isTarget = true;
            }
            else if (Target.name == "ElasticCatcher")
            {
                GameObject upLevel = granpaOfTouchedObject.transform.parent.gameObject;
                if (upLevel.name.Contains("MolarTube") && (Target.transform.parent.name == upLevel.transform.parent.name))
                    isTarget = true;
            }
        }
        

        if ((touchedObject.tag == "BracketHook") && (parentOfTargetObject.name == touchedObject.transform.parent.name))
        {
            isTarget = true;
        }



        /*-----------------------------------------------------------------
        Third case: object hit tagname is "TAD" or "WireHook"            
        -----------------------------------------------------------------*/
        if ((touchedObject.tag == "WireHook" || touchedObject.tag == "TAD") && (touchedObject.name == parentOfTargetObject.name) && (touchedObject.transform.Find("ElasticHolder").gameObject != null))
        {
            isTarget = true;
        }


        /*---------------------------------------------------------------
        Fourth case: target is ElasticCatcher, checks if there are     
        obstacles such as permanent of CchainHolder
        ---------------------------------------------------------------*/
        if (targetObject.name == "ElasticCatcher")
        {
            if (touchedObject.name == "permanent" || touchedObject.name == "CChainHolder")
            {
                if (parentOfTouchedObject.name == _targetParentName)
                    isTarget = true;
            }
            else
            {
                if (touchedObject.tag == "Bracket")
                    if (touchedObject.name == _targetParentName)
                        isTarget = true;
            }
        }


        /*-----------------------------------------------------------------
        Fifth case: ElasticCut on Aligners           
        -----------------------------------------------------------------*/
        if ((touchedObject.tag == "ElasticCut") && (touchedObject.name == _targetGrandparentName))        
            isTarget = true;


        /*-----------------------------------------------------------------
        Sixth case: object hit tagname is "Button"           
        -----------------------------------------------------------------*/
        if ((touchedObject.tag == "Button") && (touchedObject.name == parentOfTargetObject.name))
        {
            Button button = touchedObject.GetComponent<Button>();
            if (button != null)
            {
                bool buttonIsLingual = button.elementURI.Contains("Lingual");
                if (_targetButtonIsLingual == buttonIsLingual)
                {
                    if (touchedObject.transform.Find("ElasticHolder").gameObject != null)
                        isTarget = true;
                }
            }
        }
                
        

        //Seveth: CChainHolder
        if (touchedObject.name == "CChainHolder")
        {
            //if (parentOfTouchedObject.name == _targetFatherName)
            //    isTarget = true;
        }

        return isTarget;
    }

    private GameObject GetToothElement(GameObject childObject)
    {
        GameObject toothName=null;
        Transform childObjectTransform = childObject.transform;

        while (childObjectTransform.parent.parent != null)
        {
            if (childObjectTransform.parent.parent.name == "UpperLevel")
            {
                toothName = childObjectTransform.gameObject;
            }
            if (childObjectTransform.parent.name == "LowerLevel")
            {
                toothName = childObjectTransform.gameObject;
            }
            childObjectTransform = childObjectTransform.parent.transform;
        }

        return toothName;
    }

    private string GetFdi(GameObject elasticHanger)
    {
        string fdi = "";
        string elasticHangerFatherName = elasticHanger.transform.parent.name;
        string elasticHangerGrandFatherName = elasticHanger.transform.parent.parent.name;
        string elasticHangerFatherTag = elasticHanger.transform.parent.tag;
        string elasticHangerGrandFatherTag = elasticHanger.transform.parent.parent.tag;

        if (elasticHanger.tag == "ElasticHolder")
        {
            if (elasticHangerFatherTag == "WireHook")            
                fdi = elasticHangerFatherName.Substring(4, 2);                            

            if (elasticHangerFatherTag == "Button")            
                fdi = elasticHangerFatherName;
        }

        if (elasticHanger.name == "ElasticHook")
        {
            if (elasticHangerGrandFatherTag == "ElasticCut")            
                fdi = elasticHangerGrandFatherName.Substring(11, 2);
            

            if (elasticHangerGrandFatherTag == "Bracket")
                fdi = elasticHangerGrandFatherName;

        }

        if (elasticHanger.name == "ElasticCatcher")
        {
            if (elasticHangerFatherTag == "Bracket")
                fdi = elasticHangerFatherName;
        }

        return fdi;
    }

    private string GetArchOfMouth(GameObject childObject)
    {
        string parentName = "";
        Transform childObjectTransform = childObject.transform;

        while (childObjectTransform.parent != null)
        {
            if (childObjectTransform.parent.name == "UpperLevel")
            {
                parentName = "Upper";
            }
            if (childObjectTransform.parent.name == "LowerLevel")
            {
                parentName = "Lower";
            }
            childObjectTransform = childObjectTransform.parent.transform;
        }

        return parentName;
    }

    /// <summary>
    /// Inverts Origin and Target objects
    /// </summary>
    private void InvertOriginTarget()
    {
        GameObject auxiliaryObject = Origin;
        Origin = Target;
        Target = auxiliaryObject;
    }

    /// <summary>
    /// Validates if Button is Lingual or Buccal
    /// </summary>
    private bool IsLingualHolder(GameObject elasticHolder)
    {
        bool isLingual = false;

        GameObject elastichHolderFather = elasticHolder.transform.parent.gameObject;
        GameObject elasticHolderGrandFather = elastichHolderFather.transform.parent.gameObject;
        string elasticHolderFatherTag = elastichHolderFather.tag;
        string elasticHolderGrandFatherName = elasticHolderGrandFather.name;
        string elasticHolderGrandFatherTag = elasticHolderGrandFather.tag;

        switch (elasticHolder.name)
        {
            case "ElasticHolder":
                if (elasticHolderFatherTag == "Button")
                {
                    Button button = elastichHolderFather.GetComponent<Button>();
                    if (button != null)
                    {
                        if (button.elementURI.Contains("Lingual"))
                            isLingual = true;
                    }
                }
                break;

            case "ElasticHook":
                if (elasticHolderGrandFatherTag == "ElasticCut")
                {
                    if (elasticHolderGrandFatherName.Contains("Lingual"))
                        isLingual = true;
                }
                break;
        }


        return isLingual;
    }

    private bool HolderIsElasticCut(GameObject elasticHolder)
    {
        bool isElasticCut = false;
        GameObject elasticHolderFather = elasticHolder.transform.parent.gameObject;
        _targetParentName = elasticHolderFather.name;
        GameObject elasticHolderGrandFather = elasticHolderFather.transform.parent.gameObject;
        _targetGrandparentName = elasticHolderGrandFather.name;
        string grandFatherTag = elasticHolderGrandFather.tag;

        if (grandFatherTag == "ElasticCut")
        {
            isElasticCut = true;
        }
        return isElasticCut;
    }

    private bool HolderIsButton(GameObject elasticHolder)
    {
        bool isOnButton = false;
        GameObject elasticHolderFather = elasticHolder.transform.parent.gameObject;
        _targetParentName = elasticHolderFather.name;
        GameObject toothElement = elasticHolderFather.transform.parent.parent.gameObject;
        _targetGrandparentName = toothElement.name;
        string parentTag = elasticHolderFather.tag;

        if (parentTag == "Button")
        {
            isOnButton = true;
        }
        return isOnButton;
    }

    /// <summary>
    /// Sets the initial conditions to know the direction to follow
    /// </summary>
    private void InitialSettings()
    {
        string archOrigin = "";
        string archTarget = "";


        //Special Cases
        
        if (Origin.name == ElasticHolders.ElasticHolder.ToString())
        {
            if (Origin.transform.parent.tag == ElasticHolders.TAD.ToString())
            {
                archOrigin = GetArchOfMouth(Origin);
            }
        }
        else
        {
            archOrigin = ElementUtils.GetUpperLower(GetFdi(Origin)).ToString();
        }
        if (Target.name == ElasticHolders.ElasticHolder.ToString())
        {
            if (Target.transform.parent.tag == ElasticHolders.TAD.ToString())
            {
                archTarget = GetArchOfMouth(Target);
            }
        }
        else
        {
            archTarget = ElementUtils.GetUpperLower(GetFdi(Target)).ToString();
        }


        _onSameArch = archOrigin == archTarget;


        if (!_onSameArch)
        {            
            //If Origin and Target are on different arches, it starts to draw from the Upper one
            if (archTarget == "Lower" && archOrigin == "Upper")
            {
                if (HolderIsButton(Origin))
                {
                    if (IsLingualHolder(Origin))
                        _allowRouteBackOfTooth = true;
                }
                else
                {
                    if (HolderIsElasticCut(Origin))
                    {
                        if (IsLingualHolder(Origin))
                            _allowRouteBackOfTooth = true;
                    }
                    else
                        InvertOriginTarget();
                }
                    
            }
            else
            {
                if (HolderIsButton(Target))
                {
                    InvertOriginTarget();
                    if (IsLingualHolder(Origin))
                        _allowRouteBackOfTooth = true;
                }
                if (HolderIsElasticCut(Target))
                {
                    InvertOriginTarget();
                    if (IsLingualHolder(Origin))
                        _allowRouteBackOfTooth = true;
                }
            }
                


            if (IsLingualHolder(Origin))
            {
                _allowRouteBackOfTooth = true;
            }

            var mouthAngle = Ortho4DManager.SceneDataStorage.DegreeOpen;
            if (mouthAngle > 30 && !_allowRouteBackOfTooth)
            {
                _allowRouteBackOfTooth = true;
            }
        }
        else
        {
            _onSameArch = true;
            if (IsLingualHolder(Target) && !IsLingualHolder(Origin))
                InvertOriginTarget();

            if (IsLingualHolder(Origin))
                _allowRouteBackOfTooth = true;
        }

        if (HolderIsButton(Target))
        {
            _targetButtonIsLingual = IsLingualHolder(Target);
        }
    }

    /// <summary>
    /// Calculates a new avoidance point based on the Vector3 point where the Raycast hits
    /// and the width of the object hit
    /// </summary>
    private Vector3 CreateAvoidancePoint(GameObject objectHit, Vector3 pointHit, Vector3 currentPoint)
    {
        RaycastHit hitNewPointInformation;
        float moveX = 0;
        float moveY = 0;
        float moveZ = 0;

        var mouthAngle = Ortho4DManager.SceneDataStorage.DegreeOpen;
        
        if (!_auxiliaryTouchPointObject.activeSelf)
        {
            _auxiliaryTouchPointObject.SetActive(true);
        }
        _auxiliaryTouchPointObject.transform.position = pointHit;
        _auxiliaryTouchPointObject.transform.LookAt(GameObject.Find("UpperLevel").gameObject.transform);

        if (!_auxiliaryAvoidancePointObject.activeSelf)
        {
            _auxiliaryAvoidancePointObject.SetActive(true);
        }

        //Parents a new auxiliary object to keep the axis rotation correctly when mouth angle changes
        _auxiliaryAvoidancePointObject.transform.position = _auxiliaryTouchPointObject.transform.position;
        _auxiliaryAvoidancePointObject.transform.parent = _auxiliaryTouchPointObject.transform;
        _auxiliaryAvoidancePointObject.transform.localRotation = Quaternion.identity;
        _auxiliaryAvoidancePointObject.transform.localPosition = Vector3.zero;
        
        string tagOfObjectHit = IsAlignerElement(objectHit) ? "Aligner" : objectHit.tag;        

        switch (tagOfObjectHit)
        {
            case "Bracket":
                moveZ = -ObjectProperty.GetObjectWidth(ElementAxis.BracketZ);
                break;
            case "BracketHook":              
                moveZ = -ObjectProperty.GetObjectWidth(ElementAxis.BracketZ);
                break;
            case "Tooth":
                if (_allowRouteBackOfTooth)
                {
                    if (objectHit.transform.parent.parent != null)
                    {
                        if (objectHit.transform.parent.parent.name == "LowerLevel")
                            moveY = ObjectProperty.GetObjectWidth(ElementAxis.ToothY);
                        if (objectHit.transform.parent.parent.name == "UpperLevel")
                            moveY = -ObjectProperty.GetObjectWidth(ElementAxis.ToothY);
                    }
                    else
                        moveZ = -ObjectProperty.GetObjectWidth(ElementAxis.ToothZ);

                    if (Target.transform.parent.parent.tag == ElasticHolders.ElasticCut.ToString())
                        moveY = moveY * 1.2f;                    
                }
                else
                {
                   moveZ = -ObjectProperty.GetObjectWidth(ElementAxis.ToothZ);
                }

                if ((mouthAngle > 25) && !_onSameArch && !_buttonObjectInPath)                
                    moveZ = moveZ * 0.005f * mouthAngle;
                                
                break;
            case "Button":
                if (!IsLingualHolder(objectHit))
                {
                    moveZ = -ObjectProperty.GetObjectWidth(ElementAxis.ButtonZ);
                    if (ElementUtils.GetUpperLower(objectHit.name).ToString() == "Upper")
                        moveY = -ObjectProperty.GetObjectWidth(ElementAxis.ToothY);
                    if (ElementUtils.GetUpperLower(objectHit.name).ToString() == "Lower")
                        moveY = ObjectProperty.GetObjectWidth(ElementAxis.ToothY);
                }
                else
                {
                    moveZ = ObjectProperty.GetObjectWidth(ElementAxis.ButtonZ);
                    if (ElementUtils.GetUpperLower(objectHit.name).ToString() == "Upper")
                        moveY = -ObjectProperty.GetObjectWidth(ElementAxis.ToothY);
                    if (ElementUtils.GetUpperLower(objectHit.name).ToString() == "Lower")
                        moveY = ObjectProperty.GetObjectWidth(ElementAxis.ToothY);
                }
                    
                break;
            case "Aligner":
                if (_allowRouteBackOfTooth)
                {
                    float offsetMoveY;
                    float offsetMoveZ;

                    if (_onSameArch)
                    {
                        offsetMoveY = (GetAlignerUpperLower(objectHit) == "Upper") ? -1f : 2.01f;
                        offsetMoveZ = (GetAlignerUpperLower(objectHit) == "Upper") ? -1f : -1.08f;
                        moveY = ObjectProperty.GetObjectWidth(ElementAxis.ToothY) * offsetMoveY;
                        moveZ = ObjectProperty.GetObjectWidth(ElementAxis.GenericZ) * offsetMoveZ;
                    }
                    else
                    {
                        offsetMoveY = (GetAlignerUpperLower(objectHit) == "Upper") ? -UpperSliderY : BottomSliderY;
                        offsetMoveZ = (GetAlignerUpperLower(objectHit) == "Upper") ? -UpperSliderZ : BottomSliderZ;
                        moveY = ObjectProperty.GetObjectWidth(ElementAxis.ToothY) * offsetMoveY;
                        moveZ = ObjectProperty.GetObjectWidth(ElementAxis.GenericZ) * offsetMoveZ;
                    }
                }
                else
                {                                        
                    float offsetMoveZ = (_onSameArch) ?  - 0.5f : -1f;
                    moveZ = ObjectProperty.GetObjectWidth(ElementAxis.GenericZ) * offsetMoveZ;
                }

                if ((mouthAngle > 25) && !_buttonObjectInPath)                
                    moveZ = moveZ * 0.005f * mouthAngle;
                
                break;
            default:
                moveZ = -ObjectProperty.GetObjectWidth(ElementAxis.GenericZ);
                break;
        }

        if (objectHit.name == ElasticHolders.CChainHolder.ToString())
            moveZ = -ObjectProperty.GetObjectWidth(ElementAxis.CChainHolderZ);

        _auxiliaryAvoidancePointObject.transform.localPosition += new Vector3(moveX, moveY, moveZ);
        Vector3 avoidancePointCalculated = _auxiliaryAvoidancePointObject.transform.position;
        //GameObject myop1 = Instantiate(NewCenter, _auxiliaryAvoidancePointObject.transform.position, _auxiliaryAvoidancePointObject.transform.rotation) as GameObject;
        //myop1.layer = 10;
        _auxiliaryTouchPointObject.SetActive(false);
        _auxiliaryAvoidancePointObject.SetActive(false);

        //If objects are between the current point and the new avoidance point, it adds an additionl midpoint
        if (Physics.Linecast(currentPoint, avoidancePointCalculated, out hitNewPointInformation, LayerMask))
        {
            AddAditionalControlPoint(avoidancePointCalculated, hitNewPointInformation, currentPoint);
            PathPoints.Add(avoidancePointCalculated);
        }
        else
        {
            PathPoints.Add(avoidancePointCalculated);
        }

        return avoidancePointCalculated;
    }

    private string GetAlignerUpperLower(GameObject alingerElementHit)
    {
        string alginerElementName = alingerElementHit.name;
        string fdi = "";
        string alignerPosition = "";

        if (alginerElementName.Contains("AlignerSegment"))        
            fdi = alginerElementName.Substring(15, 2);        
        if (alginerElementName.Contains("Buccal-Cutout"))
            fdi = alginerElementName.Substring(0, 2);
        if (alginerElementName.Contains("Lingual-Cutout"))
            fdi = alginerElementName.Substring(0, 2);        
        if (fdi != "")        
            alignerPosition = ElementUtils.GetUpperLower(fdi).ToString();        

        return alignerPosition;
    }

    /// <summary>
    /// Creates an in-between point as the fastest way to calculate an additional point.
    /// Additional points are required when even when a new Avoidance Point is created
    /// there are objects in between. This approach is fast and works pretty well.
    /// </summary>
    private void AddAditionalControlPoint(Vector3 avoidancePointCalculated, RaycastHit hittingPoint, Vector3 currentPoint)
    {
        float moveX = 0;
        float moveY = 0;
        float moveZ = 0;

        string objectTag = hittingPoint.collider.gameObject.tag;        
        switch (objectTag)
        {
            case "BracketHook":
                moveZ = -ObjectProperty.GetObjectWidthWorld(ElementAxis.BracketHookZ);
                break;
            case "Tooth":
                if (_allowRouteBackOfTooth)
                {
                    if (hittingPoint.collider.gameObject.transform.parent.parent != null)
                    {
                        if (hittingPoint.collider.gameObject.transform.parent.parent.name == "LowerLevel")
                            moveY = ObjectProperty.GetObjectWidthWorld(ElementAxis.ToothY);
                        if (hittingPoint.collider.gameObject.transform.parent.parent.name == "UpperLevel")
                            moveY = -ObjectProperty.GetObjectWidthWorld(ElementAxis.ToothY);
                    }
                    else
                        moveZ = -ObjectProperty.GetObjectWidthWorld(ElementAxis.ToothZ);
                }
                else
                    moveZ = -ObjectProperty.GetObjectWidthWorld(ElementAxis.ToothZ);
                break;
            case "Button":
                if (IsLingualHolder(hittingPoint.collider.gameObject))                
                    moveZ = -ObjectProperty.GetObjectWidthWorld(ElementAxis.ButtonZ);                
                else                
                    moveZ = ObjectProperty.GetObjectWidthWorld(ElementAxis.ButtonZ);
                
                break;
            case "Aligner":
                if (_allowRouteBackOfTooth)
                {
                    moveY = (GetAlignerUpperLower(hittingPoint.collider.gameObject) == "Upper") ? -ObjectProperty.GetObjectWidth(ElementAxis.ToothY) * UpperSliderY : ObjectProperty.GetObjectWidth(ElementAxis.ToothY) * BottomSliderY;
                    moveZ = (GetAlignerUpperLower(hittingPoint.collider.gameObject) == "Upper") ? -ObjectProperty.GetObjectWidth(ElementAxis.GenericZ) * UpperSliderZ : ObjectProperty.GetObjectWidth(ElementAxis.GenericZ) * BottomSliderZ;
                }
                else                
                    moveZ = (_onSameArch) ? -ObjectProperty.GetObjectWidth(ElementAxis.GenericZ) / 2 : -ObjectProperty.GetObjectWidth(ElementAxis.GenericZ);                
                break;
            default:
                moveZ = -ObjectProperty.GetObjectWidthWorld(ElementAxis.GenericZ);
                break;
        }

        Vector3 inBetweenPoint = Vector3.Lerp(currentPoint, avoidancePointCalculated, 0.5f);        
        inBetweenPoint += new Vector3(moveX, moveY, moveZ);        
        //GameObject myop2 = Instantiate(NewPoint, inBetweenPoint, Quaternion.identity) as GameObject;
        //myop2.layer = 10;
        PathPoints.Add(inBetweenPoint);
    }


    /// <summary>
    /// Validates if point is inside a Collider, necessary for Teeth
    /// </summary>
    private bool PointInsideCollider(GameObject originObject, Vector3 actualPoint)
    {
        bool insideCollider = false;

        var tempObject = GameObject.Find(GetToothElement(originObject).gameObject.name).gameObject;
        var tempObjectCollider = tempObject.GetComponent<Collider>();
        if (tempObjectCollider != null)
        {
            if (tempObjectCollider.bounds.Contains(actualPoint))
            {
                insideCollider = true;
            }
        }
        return insideCollider;
    }

    /// <summary>
    /// Evaluates if the elemant hit is an aligner element.
    /// Removes some elements on aligners from the Raycast by adding them on layer 10
    /// Layer 10 is not taked in consideration for the Raycast
    /// </summary>
    private bool IsAlignerElement(GameObject elementHit)
    {
        bool isAligner = false;
        string elementHitName = elementHit.name;
        
        if (elementHitName.Contains("Buccal-Cutout") || elementHitName.Contains("Lingual-Cutout"))
            elementHit.layer = 10;

        if (elementHitName.Contains("Aligner") || elementHitName.Contains("Buccal-Cutout") ||
            elementHitName.Contains("Lingual-Cutout"))
            isAligner= true;

        return isAligner;
    }
    
    #endregion

}
