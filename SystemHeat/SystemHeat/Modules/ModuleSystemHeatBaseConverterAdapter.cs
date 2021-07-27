﻿using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace SystemHeat
{
  /// <summary>
  /// 
  /// </summary>
  public class ModuleSystemHeatBaseConverterAdapter : PartModule
  {

    /// <summary>
    /// Unique module ID
    /// </summary>
    [KSPField(isPersistant = false)]
    public string moduleID = "baseConverter";

    /// <summary>
    /// Name of the ModuleSystemHeat on the part
    /// </summary>
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID = "";

    /// <summary>
    /// Waste heat generated by the converter
    /// </summary>
    [KSPField(isPersistant = false)]
    public float systemPower = 0f;
    
    /// <summary>
    /// Index of the converter module
    /// </summary>
    [KSPField(isPersistant = false)]
    public int converterModuleIndex = -1;

    /// <summary>
    /// Converter automatic shutdown temperature
    /// </summary>
    [KSPField(isPersistant = false)]
    public float shutdownTemperature = 1000f;

    /// <summary>
    /// System outlet temperature
    /// </summary>
    [KSPField(isPersistant = false)]
    public float systemOutletTemperature = 1000f;

    protected ModuleSystemHeat heatModule;
    protected BaseConverter converterModule;



    public string EditConverterModuleInfo(string source)
    {
      string msg = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatBaseConverterAdapter_PartInfo", systemPower.ToString("F0"),
          systemOutletTemperature.ToString("F0"),
          shutdownTemperature.ToString("F0"));

      if (source.Contains(msg))
      return source;

      int pos = source.IndexOf("\n\n");
      if (pos < 0)
        return source;
      return source.Substring(0, pos) + msg + source.Substring(pos);
    }

    public override void OnIconCreate()
    {

      base.OnIconCreate();
    }
    public void Start()
    {
      heatModule = ModuleUtils.FindHeatModule(this.part, systemHeatModuleID);
      
      if (converterModuleIndex != -1)
      {
        base.part.partInfo.moduleInfos[converterModuleIndex].info = EditConverterModuleInfo(base.part.partInfo.moduleInfos[converterModuleIndex].info); 
        
        converterModule = base.part.Modules[converterModuleIndex] as BaseConverter;
        if (converterModule == null)
        {
          Debug.LogError("[ModuleSystemHeatBaseConverterAdapter]: Module at index " + converterModuleIndex +
            " is not a BaseConverter on part " + base.part.partName, base.gameObject);
          return;
        }
      }
    }

    public void LateUpdate()
    {
      if (heatModule != null && converterModule != null)
      {
        if (HighLogic.LoadedSceneIsFlight)
        {
          GenerateHeatFlight();
          UpdateSystemHeatFlight();
        }
        if (HighLogic.LoadedSceneIsEditor)
        {
          GenerateHeatEditor();
        }
      }
    }

    void GenerateHeatFlight()
    {
      if (converterModule.ModuleIsActive())
      {
        heatModule.AddFlux(moduleID, systemOutletTemperature, systemPower, true);
      }
      else
      {
        heatModule.AddFlux(moduleID, 0f, 0f, false);
      }
    }

    void GenerateHeatEditor()
    {
      if (heatModule)
      {
        if (converterModule.IsActivated)
          heatModule.AddFlux(moduleID, systemOutletTemperature, systemPower, true);
        else
          heatModule.AddFlux(moduleID, 0f, 0f, false);
      }
    }
    protected void UpdateSystemHeatFlight()
    {
      if (converterModule.ModuleIsActive())
      {
        if (heatModule.currentLoopTemperature > shutdownTemperature)
        {
          ScreenMessages.PostScreenMessage(
            new ScreenMessage(
              Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatBaseConverterAdapter_Message_Shutdown",
                                                             part.partInfo.title),
                                                             3.0f,
                                                             ScreenMessageStyle.UPPER_CENTER));
          converterModule.ToggleResourceConverterAction(new KSPActionParam(0, KSPActionType.Activate));

          Utils.Log("[ModuleSystemHeatBaseConverterAdapter]: Overheated, shutdown fired", LogType.Modules);

        }
      }
    }
  }
}
