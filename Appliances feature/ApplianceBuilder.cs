using System.Collections.Generic;
using System.Linq;
using Ortho4D;
using UnityEngine;
using System.Collections;
using SimpleJSON;
using UnityEngine.UI;

public class ApplianceBuilder : MonoBehaviour
{

    private GameObject _applianceObject;
    private GameObject _appliancePanel;
    private ElementProperties _properties;
    private Sprite _uImage;
    private Sprite _lImage;
    private Sprite _ulImage;
    private Sprite _daytimeImg;
    private Sprite _nighttimeImg;
    private Sprite _fulltimeImg;
    public string ApplianceInfo;
    private float _originalPosX = 1200f;
    //1360
    private float _originalUpperPosX = 1200f;

    private string[] Frames =
    {
        "ApplianceRegionFrame", "ApplianceBondFrame", "ApplianceTurnsFrame", "ApplianceTimeFrame",
        "ApplianceWearingFrame", "ApplianceReplaceFrame", "ApplianceLshimFrame", "ApplianceRshimFrame"
    };


    public ApplianceBuilder(GameObject applianceObject, GameObject appliancePanel, ElementProperties properties,
        Sprite LImage, Sprite UImage, Sprite ULImage, Sprite Dtime, Sprite Ntime, Sprite Ftime)
    {
        this._applianceObject = applianceObject;
        this._appliancePanel = appliancePanel;
        this._properties = properties;
        this._uImage = UImage;
        this._lImage = LImage;
        this._ulImage = ULImage;
        this._daytimeImg = Dtime;
        this._nighttimeImg = Ntime;
        this._fulltimeImg = Ftime;
    }




    public GameObject BuildAppliance()
    {
        var origT = _applianceObject.GetComponent<RectTransform>();
        float newPosX = 0;
        Vector3[] v = new Vector3[4];
        origT.GetWorldCorners(v);
        var gapX = Screen.width - _applianceObject.transform.position.x + 50;
        if (ApplianceStack.Instance.ApplianceDictionary.Count > 0)
        {
            //newPosX = GetPositionLastAppliance().x - 160;
            ApplianceStack.Instance.ListAppliances();
            newPosX = _applianceObject.transform.position.x + gapX * ApplianceStack.Instance.ApplianceDictionary.Count;
        }
        else
        {
            newPosX = _applianceObject.transform.position.x;
        }
        
        var newApplianceObject = Instantiate(_applianceObject) as GameObject;
            
        newApplianceObject.name = _properties.ApplianceId;
        newApplianceObject.tag = ElementType.Appliance.ToString();
        newApplianceObject.SetActive(true);
        newApplianceObject.transform.position = new Vector3(newPosX, _applianceObject.transform.position.y - 21 ,
            _applianceObject.transform.position.z);
        var rectT = newApplianceObject.GetComponent<RectTransform>();
        
        rectT.anchoredPosition.Set(-82f, 81);
        newApplianceObject.transform.parent = _appliancePanel.transform;
        var newApp = newApplianceObject.AddComponent<O4DApplianceElement>() as O4DApplianceElement;
        newApp.AppProperties = _properties;
  
        return newApplianceObject;
    }

    public void ModifyAppliance(GameObject panel, string applianceId)
    {
        string applianceID = applianceId;
        GameObject applianceSelected = panel.transform.FindChild(applianceID).gameObject;
        //var ApplianceProperties = applianceSelected.GetComponent<O4DApplianceElement>();

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

    private void WearingTime(GameObject applianceObj)
    {
        //Sets the icon for wearing time
        GameObject applianceWearing = applianceObj.transform.FindChild("ApplianceWearing").gameObject;
        if (applianceWearing)
        {
            GameObject applianceWearingI = applianceWearing.transform.FindChild("ApplianceWearingImage").gameObject;
            if (applianceWearingI)
            {
                Sprite tmpSpriteW = null;

                if (_properties.Wearing == "fulltime")
                {
                    tmpSpriteW = _fulltimeImg;
                }
                else if (_properties.Wearing == "daytime")
                {
                    tmpSpriteW = _daytimeImg;
                }
                else
                {
                    tmpSpriteW = _nighttimeImg;
                }

                var spriteWe = applianceWearingI.GetComponent<Image>();
                spriteWe.sprite = tmpSpriteW;
            }
        }
    }

    private Vector3 GetPositionLastAppliance()
    {
        string ApplianceID = "";

        ApplianceID = ApplianceStack.Instance.ApplianceDictionary.ElementAt(ApplianceStack.Instance.ApplianceDictionary.Count - 1).Key;
        GameObject applianceSelected = _appliancePanel.transform.FindChild(ApplianceID).gameObject;        
        return applianceSelected.transform.position;
    }

    private void Shims(GameObject applianceObj)
    {
        //Sets the Left and Right Shims
        GameObject applianceBond = applianceObj.transform.FindChild("ApplianceBinding").gameObject;
        var labelTxt = applianceBond.GetComponent<Text>();
        labelTxt.text = (_properties.Bonded) ? "B" : "R";

        if (_properties.LeftShim > 0)
        {
            GameObject lShim = applianceObj.transform.Find("ApplianceLShim").gameObject;
            GameObject lShimF = applianceObj.transform.Find("ApplianceLshimFrame").gameObject;
            if (!lShim.activeSelf)
            {
                lShim.SetActive(true);
                lShimF.SetActive(true);
            }
            lShim.GetComponent<Text>().text = _properties.LeftShim.ToString();
        }


        if (_properties.RightShim > 0)
        {
            GameObject rShim = applianceObj.transform.Find("ApplianceRShim").gameObject;
            GameObject rShimF = applianceObj.transform.Find("ApplianceRshimFrame").gameObject;
            if (!rShim.activeSelf)
            {
                rShim.SetActive(true);
                rShimF.SetActive(true);
                rShim.GetComponent<Text>().text = _properties.RightShim.ToString();
            }
        }
    }

    private void Turns(GameObject applianceObj)
    {
        GameObject applianceTurns = applianceObj.transform.FindChild("ApplianceTurns").gameObject;
        var labelBackTurnsTxt = applianceTurns.GetComponent<Text>();

        GameObject applianceFrontTurns = applianceObj.transform.FindChild("ApplianceTurnsFront").gameObject;
        var labelFrontTurnsText = applianceFrontTurns.GetComponent<Text>();

        labelBackTurnsTxt.text = "B:0";
        labelFrontTurnsText.text = "F:0";

        if (_properties.BackTurns > 0)
        {
            labelBackTurnsTxt.text = "B:" + _properties.BackTurns.ToString();
            if (_properties.BackTurnsDuration != null)
            {
                labelBackTurnsTxt.text = labelBackTurnsTxt.text + "/" +
                                         _properties.BackTurnsDuration.Substring(0, 1).ToUpperInvariant();
                labelBackTurnsTxt.fontSize = 9;
            }
            else
            {
                labelBackTurnsTxt.fontSize = 10;
            }

        }


        if (_properties.FrontTurns > 0)
        {
            labelFrontTurnsText.text = "F:" + _properties.FrontTurns.ToString();
            if (_properties.FrontTurnsDuration != null)
            {
                labelFrontTurnsText.text = labelFrontTurnsText.text + "/" +
                                           _properties.FrontTurnsDuration.Substring(0, 1).ToUpperInvariant();
                labelFrontTurnsText.fontSize = 9;
            }
            else
            {
                labelFrontTurnsText.fontSize = 10;
            }


        }
    }

    private void Location(GameObject applianceObj)
    {
        //Sets the icon for appliance location upper/lower
        GameObject applianceUL = applianceObj.transform.FindChild("ApplianceUL").gameObject;
        Sprite tmpSprite = null;

        if (_properties.Region == "upper")
        {

            tmpSprite = _uImage;
        }
        else
        {
            tmpSprite = _lImage;
        }

        var spriteUL = applianceUL.GetComponent<Image>();
        spriteUL.sprite = tmpSprite;
    }
    

    private void FramesStyle(GameObject applianceObject)
    {
        foreach (var frame in Frames)
        {
            var regionFrame = applianceObject.transform.FindChild(frame).gameObject;
            ApplyColorToFrames(regionFrame);
        }
 
    }

    private void ApplyColorToFrames(GameObject regionFrame)
    {        
        var regionFrameImage = regionFrame.GetComponent<Image>();
        regionFrameImage.color = _properties.ApplianceColor;
    }




}

