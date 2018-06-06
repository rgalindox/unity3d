using System;
using System.Collections.Generic;
using System.Linq;
using Ortho4D;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using UnityEngine.EventSystems;


public class AppliancesController : MonoBehaviour
{

    public GameObject ApplianceObject;
    public GameObject AppliancePanel;
    public Sprite UpperImage;
    public Sprite LowerImage;
    public Sprite UpperLowerImage;
    public Sprite Daytime;
    public Sprite Nightime;
    public Sprite Fulltime;
    public GameObject LeftButton;
    public GameObject RightButton;
    public bool isHidden = false;
    public bool isPropertyHidden = false;
    public bool NeedLeftScroll = false;
    public bool NeedRightScroll = false;
    private float _originalPosX = 1360f;
    private float _intensity;
    private float _textOffset = 0;
    private int _initialScreenWidth;
    private bool _leftButtonClicked = false;
    private float _firstAppliancePosX = 0;
    private string _leftFlaggedAppliance;
    private string _rightFlaggedAppliance;
    private float _moveStep = 160;
    const float _rightBorder = 75;
    const float _leftBorder = 85;


    private string[] Frames =
    {
        "ApplianceRegionFrame", "ApplianceBondFrame", "ApplianceTurnsFrame", "ApplianceTimeFrame",
        "ApplianceWearingFrame", "ApplianceReplaceFrame", "ApplianceLshimFrame", "ApplianceRshimFrame"
    };

    private enum ApplianceGroup
    {
        removeallupper,
        removealllower        
    }

 

    void Start()
    {
        
        AddEventListeners();
        ShowHideApplianceButton(false);        
        _initialScreenWidth = Screen.width;
        ShowLeftScroll(false);
        ShowRightScroll(false);

    }

    void OnDestroy()
    {
        RemoveEventListeners();
        
    }
    

    void AddEventListeners()
    {
        string nowDateY = DateTime.Now.Year.ToString();
        string nowDateM = DateTime.Now.Month.ToString();
        string nowDateD = DateTime.Now.Day.ToString();

        nowDateM = (nowDateM.Length > 1) ? nowDateM : "0" + nowDateM;
        nowDateD = (nowDateD.Length > 1) ? nowDateD : "0" + nowDateD;


        ServiceProvider.Instance.ViewSettings.ControllerSettings.AppliancesDate =
            nowDateY + "-" + nowDateM + "-" + nowDateD;
        Ortho4DManager.Instance.EventsManager.Subscribe(ElementType.Appliance, ElementAction.Add, OnCreateAppliance);
        Ortho4DManager.Instance.EventsManager.Subscribe(ElementType.Appliance, ElementAction.Update, OnCreateAppliance);
        Ortho4DManager.Instance.EventsManager.Subscribe(ElementType.Appliance, ElementAction.Remove, OnRemoveAppliance);
        Ortho4DManager.Instance.EventsManager.Subscribe(ElementType.Appliance, ElementAction.Replace, OnReplaceAppliance);
        Ortho4DManager.Instance.EventsManager.Subscribe(ElementType.Appliance, ElementAction.Rebond, OnReplaceAppliance);
        Ortho4DManager.Instance.EventsManager.Subscribe(ElementType.Appliance, ElementAction.Repair, OnReplaceAppliance);

 
        Ortho4DManager.Instance.ClearScene += ClearScene;
        
    }





    void RemoveEventListeners()
    {
        if (Ortho4DManager.Instance == null) return;

        Ortho4DManager.Instance.EventsManager.UnSubscribe(ElementType.Appliance, ElementAction.Add, OnCreateAppliance);
        Ortho4DManager.Instance.EventsManager.UnSubscribe(ElementType.Appliance, ElementAction.Update, OnCreateAppliance);
        Ortho4DManager.Instance.EventsManager.UnSubscribe(ElementType.Appliance, ElementAction.Remove, OnRemoveAppliance);
        Ortho4DManager.Instance.EventsManager.UnSubscribe(ElementType.Appliance, ElementAction.Replace, OnReplaceAppliance);
        Ortho4DManager.Instance.EventsManager.UnSubscribe(ElementType.Appliance, ElementAction.Rebond, OnReplaceAppliance);
        Ortho4DManager.Instance.EventsManager.UnSubscribe(ElementType.Appliance, ElementAction.Repair, OnReplaceAppliance);

        Ortho4DManager.Instance.ClearScene -= ClearScene;

    }


    void RecalculateScreen()
    {        
        if (ApplianceStack.Instance.AppliancesCount() > 0)
        {
            HideAppliances();
            ShowAppliances();
        }
        
    }


    void Update()
    {
        if (Screen.width != _initialScreenWidth)
        {
            RecalculateScreen();
            _initialScreenWidth = Screen.width;
        }
        
    }

    /// <summary>
    /// Replace an Appliance
    /// </summary>
    /// <param name="ctx">ElementActionContext</param>
    /// <param name="props">IList<ElementProperties/></param>
    /// <param name="onSuccess">ElementActionCallback</param>
    /// <param name="onError">ElementActionCallback</param>
    void OnReplace(ElementActionContext ctx, IList<ElementProperties> props, ElementActionCallback onSuccess, ElementActionCallback onError)
    {
 
    }

    void ClearScene(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Appliance:
                var appls = ElementRegistry.GetRegisteredByTag(elementType.ToString());
                foreach (var item in appls)
                    OnRemoveAppliance(new ElementActionContext(elementType, ElementAction.Remove), new[] { item.Properties }, JSONManager.ClearSuccessResult, JSONManager.ClearFailResult);
                break;
        }
    }

    private void MoveButtonsToFront()
    {
        LeftButton.transform.SetAsLastSibling();
        RightButton.transform.SetAsLastSibling();
        
    }
    public void HideShowAppliances()
    {
        var applianceButtonH = AppliancePanel.transform.Find("AppliancesButtonHolder").gameObject;
        var applianceButtonV = applianceButtonH.transform.Find("AppliancesViewButton").gameObject;
        var buttonView = applianceButtonV.GetComponent<Image>();                
        var colorT = new Color(0.196f, 0.745f, 1, 1);

        if (!isHidden)
        {
            HideAppliances();
            buttonView.color = Color.white;
            isHidden = true;
            
        }
        else
        {
            ShowAppliances();
            buttonView.color = colorT;
            isHidden = false;
        }
    }

    public void HideShowProperties()
    {
        var applianceButtonH = AppliancePanel.transform.Find("AppliancesButtonHolder").gameObject;
        var appliancePropButton = applianceButtonH.transform.Find("AppliancesPropertiesButton").gameObject;
        var buttonProps = appliancePropButton.GetComponent<Image>();
        var colorT = new Color(0.196f, 0.745f, 1, 1);

        if (!isPropertyHidden)
        {
            HideProperties();
            buttonProps.color = Color.white;
            isPropertyHidden = true;

        }
        else
        {
            ShowProperties();
            buttonProps.color = colorT;
            isPropertyHidden = false;
        }
    }



    public void HideProperties()
    {
        foreach (var applianceSelected in ApplianceStack.Instance.ApplianceDictionary)
        {
            var applianceKey = applianceSelected.Key;
            var applianceObj = AppliancePanel.transform.Find(applianceKey).gameObject;
            var applianceObjProperties = applianceObj.GetChildrenByTag("applianceProperty");
            foreach (var property in applianceObjProperties)
            {
                property.SetActive(false);
            }
            isPropertyHidden = true;

        }
    }

    public void ShowProperties()
    {
        foreach (var applianceSelected in ApplianceStack.Instance.ApplianceDictionary)
        {
            var applianceKey = applianceSelected.Key;
            var applianceObj = AppliancePanel.transform.Find(applianceKey).gameObject;
            var applianceObjProperties = applianceObj.GetChildrenByTag("applianceProperty");
            var aProperties = applianceObj.GetComponent<O4DApplianceElement>().AppProperties;
            foreach (var property in applianceObjProperties)
            {
                if (property.gameObject.name.ToLowerInvariant().Contains("lshim"))
                {                    
                    if (aProperties.LeftShim > 0)
                    {
                        property.SetActive(true);
                    }

                }
                else if (property.gameObject.name.ToLowerInvariant().Contains("rshim"))
                {
                    if (aProperties.RightShim > 0)
                    {
                        property.SetActive(true);
                    }
                }
                else
                {
                    property.SetActive(true);
                }
                
            }
            isPropertyHidden = false;

        }
    }
    public void ShowHideApplianceButton(bool state)
    {
        var applianceButtonH = AppliancePanel.transform.Find("AppliancesButtonHolder").gameObject;
        var applianceButton = applianceButtonH.transform.Find("AppliancesButton").gameObject;
        applianceButton.SetActive(state);
        var appliancePropButton = applianceButtonH.transform.Find("AppliancesPropertiesButton").gameObject;
        appliancePropButton.SetActive(state);
        var applianceViewButton = applianceButtonH.transform.Find("AppliancesViewButton").gameObject;
        applianceViewButton.SetActive(state);


        var buttonProps = applianceViewButton.GetComponent<Image>();        
        var colorT = new Color(0.196f, 0.745f, 1, 1);

        buttonProps.color = isHidden ? Color.white : colorT;

        buttonProps = appliancePropButton.GetComponent<Image>();
        buttonProps.color = isPropertyHidden ? Color.white : colorT;


    }

   
 

    private bool IsApplianceInViewport(GameObject appl)
    {
        var applPosX = appl.transform.position.x;
        return (applPosX > _leftBorder) && (applPosX < (Screen.width - _rightBorder));
    }

    private void HideSingleAppliance(GameObject appl)
    {
        
        appl.SetActive(false);
        return;
        
        foreach (Transform applTransform in appl.transform)
        {
            var applObj = applTransform.gameObject;
            var appImage = applObj.GetComponent<Image>();
            var tempColor = appImage.color;
            tempColor.a = 0f;
            appImage.color = tempColor;


        }
    }

    private void ShowSingleAppliance(GameObject appl)
    {
        
        appl.SetActive(true);
        return;
        foreach (Transform applTransform in appl.transform)
        {
            var applObj = applTransform.gameObject;
            var applImg = applObj.GetComponent<Image>();
            var applImgColor = applImg.GetComponent<Color>();
            //button.GetComponent<Image>().SetTransparency(float value);
            //applImgColor.a = 1;
        }

    }

    public void HideAppliances()
    {
        ShowLeftScroll(false);
        ShowRightScroll(false);
        _leftButtonClicked = false;
        foreach (var applianceSelected in ApplianceStack.Instance.ApplianceDictionary)
        {
            var applianceKey = applianceSelected.Key;
            var applianceObj = AppliancePanel.transform.Find(applianceKey).gameObject;
            applianceObj.SetActive(false);
        }
        isHidden = true;

    }

    public void ShowAppliances()
    {
        int applianceCount = 0;
        float newPosX = 0;
        ShowLeftScroll(false);
        ShowRightScroll(false);
        foreach (var applianceSelected in ApplianceStack.Instance.ApplianceDictionary)
        {
            var applianceKey = applianceSelected.Key;
            var applianceSelectedObj = AppliancePanel.transform.Find(applianceKey).gameObject;
            applianceSelectedObj.SetActive(true);

            var origT = ApplianceObject.GetComponent<RectTransform>();
            Vector3[] v = new Vector3[4];
            origT.GetWorldCorners(v);
            var gapX = Screen.width - ApplianceObject.transform.position.x + 50;
            if (applianceCount > 0)
            {
                newPosX = GetPositionLastAppliance(applianceCount).x - 160;
            }
            else
            {
                newPosX = ApplianceObject.transform.position.x;
            }

            applianceSelectedObj.transform.position = new Vector3(newPosX, applianceSelectedObj.transform.position.y, applianceSelectedObj.transform.position.z);


            if (!IsApplianceInViewport(applianceSelectedObj))
            {
                ShowLeftScroll(true);
                HideSingleAppliance(applianceSelectedObj);

            }

            applianceCount++;
        }

        isHidden = false;
    }

    public void ShowLeftScroll(bool status)
    {
        LeftButton.SetActive(status);
        MoveButtonsToFront();
    }

    public void ShowRightScroll(bool status)
    {
        RightButton.SetActive(status);
        MoveButtonsToFront();
    }

    /// <summary>
    /// Create Appliance
    /// </summary>
    /// <param name="ctx">ElementActionContext</param>
    /// <param name="elementList">IList<ElementProperties></param>
    /// <param name="onSuccess">ElementActionCallback</param>
    /// <param name="onError">ElementActionCallback</param>
    void OnCreateAppliance(ElementActionContext ctx, IList<ElementProperties> elementList,
        ElementActionCallback onSuccess, ElementActionCallback onError)
    {        
        foreach (var elementProperties in elementList)
        {
            if (!ApplianceStack.Instance.IsApplianceRegistered(elementProperties.ApplianceId))
            {
                ApplianceBuilder newAppliance = new ApplianceBuilder(ApplianceObject, AppliancePanel, elementProperties,
                    LowerImage, UpperImage, UpperLowerImage, Daytime, Nightime, Fulltime);

                var newApplianceObject = newAppliance.BuildAppliance();
                var aProperties = newApplianceObject.GetComponent<O4DApplianceElement>().AppProperties;

                Color32 colori = aProperties.ApplianceColor;
                _intensity = ((colori.r * 0.299f) + (colori.g * 0.587f) + (colori.b * 0.114f));
                //Set Icon properties
                IconColor(newApplianceObject, aProperties.ApplianceColor);
                Label(newApplianceObject, aProperties.Label, aProperties.ApplianceColor);
                Shims(newApplianceObject, aProperties, aProperties.ApplianceColor);
                Turns(newApplianceObject, aProperties);
                Location(newApplianceObject, aProperties);
                WearingTime(newApplianceObject, aProperties);
                FramesStyle(newApplianceObject, aProperties);
                FailuresProperties(newApplianceObject, aProperties);                
                WearingDuration(newApplianceObject, aProperties);
                ApplianceStack.Instance.ApplianceDictionary.Add(aProperties.ApplianceId, aProperties.Region);                
                onSuccess(ctx, elementProperties, "Success: " + ctx.ElementAction + " on " + ctx.ElementType);

            }
            else
            {
                string applianceID = elementProperties.ApplianceId;
                GameObject applianceSelected = AppliancePanel.transform.FindChild(applianceID).gameObject;
                if (applianceSelected != null)
                {
                    var aProperties = elementProperties;
                    //Set intensisty of the appliance color to set the labels color
                    Color32 colori = aProperties.ApplianceColor;
                    _intensity = ((colori.r * 0.299f) + (colori.g * 0.587f) + (colori.b * 0.114f));
                    //Update Icon properties
                    IconColor(applianceSelected, aProperties.ApplianceColor);
                    Label(applianceSelected, aProperties.Label, aProperties.ApplianceColor);
                    Shims(applianceSelected, aProperties, aProperties.ApplianceColor);
                    Turns(applianceSelected, aProperties);
                    Location(applianceSelected, aProperties);
                    WearingTime(applianceSelected, aProperties);
                    FramesStyle(applianceSelected, aProperties);                    
                    WearingDuration(applianceSelected, aProperties);
                    UpdateFailures(applianceSelected, aProperties);
                    applianceSelected.GetComponent<O4DApplianceElement>().AppProperties = aProperties;

                    onSuccess(ctx.WithAction(ElementAction.Update), aProperties, ctx.ElementType + " updated ");
                }
            }
        }

        ShowHideApplianceButton(ApplianceStack.Instance.AppliancesCount() > 0);
        HideAppliances();
        ShowAppliances();
        MoveButtonsToFront();
    }


    private void UpdateFailures(GameObject applianceSelected, ElementProperties props)
    {
        GameObject applianceReplace = applianceSelected.transform.FindChild("ApplianceReplace").gameObject;
        var labelReplaceTxt = applianceReplace.GetComponent<Text>();
        GameObject applianceRebond = applianceSelected.transform.FindChild("ApplianceRebond").gameObject;
        var labelRebondTxt = applianceRebond.GetComponent<Text>();
        GameObject applianceRepair = applianceSelected.transform.FindChild("ApplianceRepair").gameObject;
        var labelRepairTxt = applianceRepair.GetComponent<Text>();
        //====


        props.Rebond = Int32.Parse(labelRebondTxt.text);
        props.Repair = Int32.Parse(labelRepairTxt.text);
        props.Replace = Int32.Parse(labelReplaceTxt.text);

        Failures(applianceSelected);
        applianceSelected.GetComponent<O4DApplianceElement>().AppProperties = props;
    }



    private void OnReplaceAppliance(ElementActionContext ctx, IList<ElementProperties> elementList, ElementActionCallback onSuccess, ElementActionCallback onError)
    {
        foreach (var elementProperties in elementList)
        {
            string applianceID = elementProperties.ApplianceId;
            GameObject applianceSelected = AppliancePanel.transform.FindChild(applianceID).gameObject;
            if (applianceSelected != null)
            {
                GameObject applianceReplace = applianceSelected.transform.FindChild("ApplianceReplace").gameObject;
                var labelReplaceTxt = applianceReplace.GetComponent<Text>();
                GameObject applianceRebond = applianceSelected.transform.FindChild("ApplianceRebond").gameObject;
                var labelRebondTxt = applianceRebond.GetComponent<Text>();
                GameObject applianceRepair = applianceSelected.transform.FindChild("ApplianceRepair").gameObject;
                var labelRepairTxt = applianceRepair.GetComponent<Text>();

                switch (ctx.ElementAction)
                {
                    case ElementAction.Replace:
                        //elementProperties.Replace += 1;
                        labelReplaceTxt.text = (Int32.Parse(labelReplaceTxt.text) + 1).ToString();
                        break;
                    case ElementAction.Rebond:
                        //elementProperties.Rebond += 1;
                        labelRebondTxt.text = (Int32.Parse(labelRebondTxt.text) + 1).ToString();
                        break;
                    case ElementAction.Repair:
                        //elementProperties.Repair += 1;
                        labelRepairTxt.text = (Int32.Parse(labelRepairTxt.text) + 1).ToString();
                        break;
                }


                elementProperties.Rebond = Int32.Parse(labelRebondTxt.text);
                elementProperties.Repair = Int32.Parse(labelRepairTxt.text);
                elementProperties.Replace = Int32.Parse(labelReplaceTxt.text);
                               
                Failures(applianceSelected);
                applianceSelected.GetComponent<O4DApplianceElement>().AppProperties = elementProperties;


                onSuccess(ctx, elementProperties, "Success: " + ctx.ElementAction + " on " + ctx.ElementType);



            }
        }
    }




    private void WearingDuration(GameObject applianceObj, ElementProperties aProperties)
    {
        const double daysToMonths = 30.4368499;
        string durationSuffix = "D";
        double durationNumber = 0;
        string globalAppliancesDate = Ortho4DManager.SceneDataStorage.AppliancesDate;
        DateTime endDate = Convert.ToDateTime(globalAppliancesDate);
        
        if (aProperties.End != null)
            endDate = Convert.ToDateTime(aProperties.End);

        DateTime startDate = Convert.ToDateTime(aProperties.Start);
        TimeSpan diffTime = endDate.Subtract(startDate);
        durationNumber = diffTime.TotalDays;
        if (diffTime.TotalDays < 0)
            durationNumber = 0;
        else
            durationNumber++;

        
        if (durationNumber > 7)
        {
            durationSuffix = "W";
            durationNumber = durationNumber / 7;
            if (durationNumber >= 4)
            {
                durationSuffix = "M";
                durationNumber = (durationNumber*7) / daysToMonths;

            }
        }
        
        GameObject applianceTimeWearing= applianceObj.transform.FindChild("ApplianceTime").gameObject;
        var labelTimeWearingTxt = applianceTimeWearing.GetComponent<Text>();
        labelTimeWearingTxt.text = Math.Round(durationNumber) + durationSuffix;


    }


    private void Failures(GameObject applianceObj)
    {
        //Sets the icon for appliance location upper/lower
        GameObject applianceFailures = applianceObj.transform.FindChild("ApplianceFailures").gameObject;
        var labelFailuresTxt = applianceFailures.GetComponent<Text>();

        GameObject applianceRepair = applianceObj.transform.FindChild("ApplianceRepair").gameObject;
        var labelRepairTxt = applianceRepair.GetComponent<Text>();

        GameObject applianceRebond = applianceObj.transform.FindChild("ApplianceRebond").gameObject;
        var labelRebondTxt = applianceRebond.GetComponent<Text>();

        GameObject applianceReplace = applianceObj.transform.FindChild("ApplianceReplace").gameObject;
        var labelReplaceTxt = applianceReplace.GetComponent<Text>();

        int failureCount = Int32.Parse(labelRepairTxt.text) + Int32.Parse(labelRebondTxt.text) + Int32.Parse(labelReplaceTxt.text);
        
        labelFailuresTxt.text = failureCount.ToString();
        

    }

    private void InitFailures(GameObject applianceObj, ElementProperties aProperties)
    {
        //Sets the icon for appliance location upper/lower
        GameObject applianceFailures = applianceObj.transform.FindChild("ApplianceReplace").gameObject;
        var labelFailuresTxt = applianceFailures.GetComponent<Text>();
        var sum = aProperties.Replace + aProperties.Rebond + aProperties.Repair;
        labelFailuresTxt.text = sum.ToString();


    }


    private void OnRemoveAppliance(ElementActionContext ctx, IList<ElementProperties> elementList,
        ElementActionCallback onSuccess, ElementActionCallback onError)
    {        
        
        foreach (var elementProperties in elementList)
        {            
            if (elementProperties.TemplateName.ToLowerInvariant().Contains("remove all upper"))
            {
                
                var listUpper = ApplianceStack.Instance.ApplianceDictionary.Where(s => s.Value.Contains("upper")).ToList();
                var forcedProperties = elementProperties;
                foreach (var itemUpper in listUpper)
                {                    
                    string applianceID = itemUpper.Key;
                    GameObject applianceSelected = AppliancePanel.transform.FindChild(applianceID).gameObject;
                    if (RemoveAppliance(applianceSelected,applianceID))
                    {
                        var aProperties = applianceSelected.GetComponent<O4DApplianceElement>().AppProperties;
                        
                    }
                    
                }
                onSuccess(ctx, forcedProperties, "Success: " + ctx.ElementAction + " on " + ctx.ElementType);

            }
            else if (elementProperties.TemplateName.ToLowerInvariant().Contains("remove all lower"))
            {                
                var listLower = ApplianceStack.Instance.ApplianceDictionary.Where(s => s.Value.Contains("lower")).ToList();
                var forcedProperties = elementProperties;
                foreach (var itemLower in listLower)
                {                    
                    string applianceID = itemLower.Key;
                    GameObject applianceSelected = AppliancePanel.transform.FindChild(applianceID).gameObject;
                    if (RemoveAppliance(applianceSelected, applianceID))
                    {
                        var aProperties = applianceSelected.GetComponent<O4DApplianceElement>().AppProperties;
                        
                    }
                }
                onSuccess(ctx, forcedProperties, "Success: " + ctx.ElementAction + " on " + ctx.ElementType);
            }
            else if (elementProperties.TemplateName.ToLowerInvariant().Contains("remove all"))
            {
                var listAll = ApplianceStack.Instance.ApplianceDictionary.ToList();
                var forcedProperties = elementProperties;
                foreach (var itemSelected in listAll)
                {
                    string applianceID = itemSelected.Key;
                    GameObject applianceSelected = AppliancePanel.transform.FindChild(applianceID).gameObject;
                    if (RemoveAppliance(applianceSelected, applianceID))
                    {
                        var aProperties = applianceSelected.GetComponent<O4DApplianceElement>().AppProperties;
                    }
                }
                onSuccess(ctx, forcedProperties, "Success: " + ctx.ElementAction + " on " + ctx.ElementType);

            }
            else
            {
                string applianceID = elementProperties.ApplianceId;
                GameObject applianceSelected = AppliancePanel.transform.FindChild(applianceID).gameObject;
                if (RemoveAppliance(applianceSelected, elementProperties.ApplianceId))
                {
                    var aProperties = applianceSelected.GetComponent<O4DApplianceElement>().AppProperties;
                    onSuccess(ctx, aProperties, "Success: " + ctx.ElementAction + " on " + ctx.ElementType);                    
                }
            }
        }     
        HideAppliances();
        ShowAppliances();
        ShowHideApplianceButton(ApplianceStack.Instance.AppliancesCount() > 0);
        
    }

 

    private bool RemoveAppliance(GameObject applianceObj,string applianceId)
    {
        bool IsSuccess = false;
        GameObject applianceSelected = applianceObj;
        if (applianceSelected != null)
        {
            //ApplianceStack.Instance.DeregisterAppliance(applianceId);
            ApplianceStack.Instance.ApplianceDictionary.Remove(applianceId);
            Destroy(applianceSelected);
            IsSuccess = true;

        }
        return IsSuccess;
    }

    /// <summary>
    /// Removes all Appliances in the scene
    /// </summary>
    /// <param name="ctx">ElementActionContext</param>
    /// <param name="onError">ElementActionCallback</param>
    /// <param name="onSuccess">ElementActionCallback</param>
    /// <param name="appliances">GameObject[]</param>
    void RemoveAllAppliances(ElementActionContext ctx, ElementActionCallback onError, ElementProperties element, ElementActionCallback onSuccess, List<Ortho4DElement> appliances)
    {
        if (appliances == null)
            return;

        foreach (var appliance in appliances)
        {


        }
    }

    private void IconColor(GameObject applianceObj, Color color)
    {        
        var appImage = applianceObj.GetComponent<Image>();
        appImage.color = color;
    }

    private void Label(GameObject applianceObj, string text, Color color)
    {
        GameObject applianceLabel = applianceObj.transform.FindChild("ApplianceLabel").gameObject;
        Text labelTxt = applianceLabel.GetComponent<Text>();
        labelTxt.text = text;
        labelTxt.color = color;
    }

    private void WearingTime(GameObject applianceObj,ElementProperties aProperties)
    {
        //Sets the icon for wearing time
        GameObject applianceWearing = applianceObj.transform.FindChild("ApplianceWearing").gameObject;
        if (applianceWearing)
        {
            GameObject applianceWearingI = applianceWearing.transform.FindChild("ApplianceWearingImage").gameObject;
            if (applianceWearingI)
            {
                Sprite tmpSpriteW = null;

                if (aProperties.Wearing == "fulltime")
                {
                    tmpSpriteW = Fulltime;
                }
                else if (aProperties.Wearing == "daytime")
                {
                    tmpSpriteW = Daytime;
                }
                else
                {
                    tmpSpriteW = Nightime;
                }

                var spriteWe = applianceWearingI.GetComponent<Image>();
                spriteWe.sprite = tmpSpriteW;
            }
        }
    }

    private void FailuresProperties(GameObject applianceObj, ElementProperties aProperties)
    {

        GameObject applianceRepair = applianceObj.transform.FindChild("ApplianceRepair").gameObject;
        var labelRepairTxt = applianceRepair.GetComponent<Text>();

        GameObject applianceRebond = applianceObj.transform.FindChild("ApplianceRebond").gameObject;
        var labelRebondTxt = applianceRebond.GetComponent<Text>();

        GameObject applianceReplace = applianceObj.transform.FindChild("ApplianceReplace").gameObject;
        var labelReplaceTxt = applianceReplace.GetComponent<Text>();

        labelRepairTxt.text = aProperties.Repair.ToString();
        labelReplaceTxt.text = aProperties.Replace.ToString();
        labelRebondTxt.text = aProperties.Rebond.ToString();

        Failures(applianceObj);

    }

    private void UpdateActionValues(GameObject applianceObj, int rebond, int repair, int replace)
    {

        GameObject applianceRepair = applianceObj.transform.FindChild("ApplianceRepair").gameObject;
        var labelRepairTxt = applianceRepair.GetComponent<Text>();

        GameObject applianceRebond = applianceObj.transform.FindChild("ApplianceRebond").gameObject;
        var labelRebondTxt = applianceRebond.GetComponent<Text>();

        GameObject applianceReplace = applianceObj.transform.FindChild("ApplianceReplace").gameObject;
        var labelReplaceTxt = applianceReplace.GetComponent<Text>();

        labelRepairTxt.text = repair.ToString();
        labelReplaceTxt.text = replace.ToString();
        labelRebondTxt.text = rebond.ToString();

        Failures(applianceObj);

    }


    private void Shims(GameObject applianceObj, ElementProperties aProperties, Color aColor)
    {
        //Sets the Left and Right Shims
        GameObject applianceBond = applianceObj.transform.FindChild("ApplianceBinding").gameObject;
        var labelTxt = applianceBond.GetComponent<Text>();
        labelTxt.text = (aProperties.Bonded) ? "B" : "R";

        labelTxt.color = (_intensity) > 186 ? Color.black : Color.white;
            

        if (aProperties.LeftShim > 0)
        {
            GameObject lShim = applianceObj.transform.Find("ApplianceLShim").gameObject;
            GameObject lShimF = applianceObj.transform.Find("ApplianceLshimFrame").gameObject;
            if (!lShim.activeSelf)
            {
                lShim.SetActive(true);
                lShimF.SetActive(true);
            }
            lShim.GetComponent<Text>().text = aProperties.LeftShim.ToString();
            lShim.GetComponent<Text>().color = labelTxt.color;
        }


        if (aProperties.RightShim > 0)
        {
            GameObject rShim = applianceObj.transform.Find("ApplianceRShim").gameObject;
            GameObject rShimF = applianceObj.transform.Find("ApplianceRshimFrame").gameObject;
            if (!rShim.activeSelf)
            {
                rShim.SetActive(true);
                rShimF.SetActive(true);
            }
            rShim.GetComponent<Text>().text = aProperties.RightShim.ToString();
            rShim.GetComponent<Text>().color = labelTxt.color;
        }
    }

    private void Turns(GameObject applianceObj, ElementProperties aProperties)
    {
        
        GameObject applianceTurns = applianceObj.transform.FindChild("ApplianceTurns").gameObject;
        var labelBackTurnsTxt = applianceTurns.GetComponent<Text>();

        GameObject applianceFrontTurns = applianceObj.transform.FindChild("ApplianceTurnsFront").gameObject;
        var labelFrontTurnsText = applianceFrontTurns.GetComponent<Text>();
        

        if ((aProperties.BackTurns <= 0) && (aProperties.FrontTurns <= 0))
        {
            labelBackTurnsTxt.text = "N/A";
            labelFrontTurnsText.text = "";
            labelBackTurnsTxt.fontSize = 12;                        
            if (labelBackTurnsTxt.rectTransform.position.y > 25f)
                _textOffset = -5f;
            else
                _textOffset = 0;
        }
        else
        {
            labelBackTurnsTxt.text = "B:N/A";
            labelFrontTurnsText.text = "F:N/A";
            labelBackTurnsTxt.fontSize = 9;
            if (labelBackTurnsTxt.rectTransform.position.y < 25f)
                _textOffset = +5f;
            else
                _textOffset = 0;
        }
        
        labelBackTurnsTxt.rectTransform.position += new Vector3(0, _textOffset, 0);
        
        if (aProperties.BackTurns > 0)
        {
            labelBackTurnsTxt.text = "B:" + aProperties.BackTurns.ToString();
            if (aProperties.BackTurnsDuration != null)
            {
                labelBackTurnsTxt.text = labelBackTurnsTxt.text + "/";
                labelBackTurnsTxt.text += (aProperties.BackTurnsDuration == "2weeks") ? aProperties.BackTurnsDuration.Substring(0, 2).ToUpperInvariant() :
                                         aProperties.BackTurnsDuration.Substring(0, 1).ToUpperInvariant();
                labelBackTurnsTxt.fontSize = 9;
            }
            else
            {
                labelBackTurnsTxt.fontSize = 10;
            }

        }


        if (aProperties.FrontTurns > 0)
        {
            labelFrontTurnsText.text = "F:" + aProperties.FrontTurns.ToString();
            if (aProperties.FrontTurnsDuration != null)
            {
                labelFrontTurnsText.text = labelFrontTurnsText.text + "/";
                labelFrontTurnsText.text += (aProperties.FrontTurnsDuration == "2weeks") ? aProperties.FrontTurnsDuration.Substring(0, 2).ToUpperInvariant() : 
                    aProperties.FrontTurnsDuration.Substring(0, 1).ToUpperInvariant();
                labelFrontTurnsText.fontSize = 9;
            }
            else
            {
                labelFrontTurnsText.fontSize = 10;
            }
        }

    }

    private void Location(GameObject applianceObj, ElementProperties aProperties)
    {
        //Sets the icon for appliance location upper/lower
        GameObject applianceUL = applianceObj.transform.FindChild("ApplianceUL").gameObject;
        Sprite tmpSprite = null;

        if (aProperties.Region == "upper")
        {

            tmpSprite = UpperImage;
        }
        else
        {
            tmpSprite = LowerImage;
        }

        var spriteUL = applianceUL.GetComponent<Image>();
        spriteUL.sprite = tmpSprite;
        spriteUL.color = (_intensity) > 186 ? Color.black : Color.white;
    }

  


    private void FramesStyle(GameObject applianceObject, ElementProperties aProperties)
    {
        foreach (var frame in Frames)
        {
            var regionFrame = applianceObject.transform.FindChild(frame).gameObject;
            ApplyColorToFrames(regionFrame,aProperties);
        }

    }

    private void ApplyColorToFrames(GameObject regionFrame, ElementProperties aProperties)
    {
        var regionFrameImage = regionFrame.GetComponent<Image>();
        regionFrameImage.color = aProperties.ApplianceColor;
    }

    /// <summary>
    /// Move appliances
    /// </summary>

    public void MoveAppliancesToRight()
    {
        float offsetX = 75;
        GameObject FirstAppliance = null;
        GameObject LastAppliance = null;
        

        var firstIndex = ApplianceStack.Instance.ApplianceDictionary.ElementAt(0).Key;
        var lastIndex = ApplianceStack.Instance.ApplianceDictionary.ElementAt(ApplianceStack.Instance.ApplianceDictionary.Count - 1).Key;
        FirstAppliance = AppliancePanel.transform.Find(firstIndex).gameObject;
        LastAppliance = AppliancePanel.transform.Find(lastIndex).gameObject;

        foreach (var applianceSelected in ApplianceStack.Instance.ApplianceDictionary)
        {
            var applianceKey = applianceSelected.Key;
            var applianceSelectedObj = AppliancePanel.transform.Find(applianceKey).gameObject;


            var tempPosX = applianceSelectedObj.transform.position.x;
            var newPosX = applianceSelectedObj.transform.position.x + _moveStep;

            applianceSelectedObj.transform.position = new Vector3(newPosX, applianceSelectedObj.transform.position.y,
                applianceSelectedObj.transform.position.z);

            if (IsApplianceInViewport(applianceSelectedObj))
            {
                ShowSingleAppliance(applianceSelectedObj);
            }
            else
            {
                HideSingleAppliance(applianceSelectedObj);
            }

        }


        if (IsApplianceInViewport(LastAppliance))
        {
            ShowLeftScroll(false);
        }

        if (!IsApplianceInViewport(FirstAppliance))
        {
            ShowRightScroll(true);
        }
    }

    public void MoveAppliancesToLeft()
    {        
        GameObject FirstAppliance = null;
        GameObject LastAppliance = null;
        

        var firstIndex = ApplianceStack.Instance.ApplianceDictionary.ElementAt(0).Key;
        var lastIndex = ApplianceStack.Instance.ApplianceDictionary.ElementAt(ApplianceStack.Instance.ApplianceDictionary.Count - 1).Key;
        FirstAppliance = AppliancePanel.transform.Find(firstIndex).gameObject;
        LastAppliance = AppliancePanel.transform.Find(lastIndex).gameObject;

        foreach (var applianceSelected in ApplianceStack.Instance.ApplianceDictionary)
        {
            var applianceKey = applianceSelected.Key;
            var applianceSelectedObj = AppliancePanel.transform.Find(applianceKey).gameObject;



            var tempPosX = applianceSelectedObj.transform.position.x;
            var newPosX = applianceSelectedObj.transform.position.x - _moveStep;

            applianceSelectedObj.transform.position = new Vector3(newPosX, applianceSelectedObj.transform.position.y,
                applianceSelectedObj.transform.position.z);

            if (IsApplianceInViewport(applianceSelectedObj))
            {
                ShowSingleAppliance(applianceSelectedObj);
            }
            else
            {
                HideSingleAppliance(applianceSelectedObj);
            }

        }


        if (!IsApplianceInViewport(LastAppliance))
        {
            ShowLeftScroll(true);
        }

        if (IsApplianceInViewport(FirstAppliance))
        {
                ShowRightScroll(false);
        }

    }

    private Vector3 GetPositionLastAppliance(int indice)
    {
        string ApplianceID = "";

        ApplianceID = ApplianceStack.Instance.ApplianceDictionary.ElementAt(indice - 1).Key;
        GameObject applianceSelected = AppliancePanel.transform.FindChild(ApplianceID).gameObject;
        return applianceSelected.transform.position;
    }

    private Vector3 GetPositionNextAppliance(int indice)
    {
        string ApplianceID = "";
        

        ApplianceID = ApplianceStack.Instance.ApplianceDictionary.ElementAt(indice + 1).Key;
        GameObject applianceSelected = AppliancePanel.transform.FindChild(ApplianceID).gameObject;
        return applianceSelected.transform.position;
    }
}
