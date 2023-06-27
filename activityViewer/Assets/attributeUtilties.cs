using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum attributesEnum
    {
        step, fired, fired_fraction, activity, dampening, current_calcium, target_calcium,
        synaptic_input, background_input, grown_axons, connected_axons, grown_dendrites,
        connected_dendrites
    }
 public enum dataSetsEnum
    {
        viz_no_network, viz_calcium, viz_disable, viz_stimulus
    }

class AttributeUtils { 
    public static Dictionary<attributesEnum,string> attributeNames = new Dictionary<attributesEnum, string>{
        { attributesEnum.step, "step" },
        {attributesEnum.fired, "fired" }, 
        {attributesEnum.fired_fraction,"fired fraction" }, 
        {attributesEnum.activity,"activity" }, 
        {attributesEnum.dampening,"dampening" }, 
        {attributesEnum.current_calcium,"current calcium" }, 
        {attributesEnum.target_calcium,"target calcium" }, 
        {attributesEnum.synaptic_input,"synaptic input" }, 
        {attributesEnum.background_input,"background input" }, 
        {attributesEnum.grown_axons,"grown axons" }, 
        {attributesEnum.connected_axons,"connected axons" }, 
        {attributesEnum.grown_dendrites,"grown dendrites" },
        {attributesEnum.connected_dendrites, "connected dendrites" } };

        public static Dictionary<attributesEnum,string>  attributeIdentifiers =new Dictionary<attributesEnum, string>{
            {attributesEnum.step,"step" }, 
            {attributesEnum.fired,"fired" }, 
            {attributesEnum.fired_fraction,"fired_fraction" }, 
            {attributesEnum.activity,"activity" }, 
            {attributesEnum.dampening,"dampening" }, 
            {attributesEnum.current_calcium,"current_calcium" }, 
            {attributesEnum.target_calcium,"target_calcium" }, 
            {attributesEnum.synaptic_input,"synaptic_input" }, 
            {attributesEnum.background_input,"background_input" }, 
            {attributesEnum.grown_axons,"grown_axons" }, 
            {attributesEnum.connected_axons,"connected_axons" }, 
            {attributesEnum.grown_dendrites,"grown_dendrites" }, 
            {attributesEnum.connected_dendrites,"connected_dendrites" } };
    //strings of the paths for the possible datasets
    public static Dictionary<dataSetsEnum,string> dataSetNames =new Dictionary<dataSetsEnum,string> { 
        {dataSetsEnum.viz_no_network ,"viz-no-network" },
        {dataSetsEnum.viz_calcium, "viz-calcium" },
        {dataSetsEnum.viz_disable, "viz-disable" },
        {dataSetsEnum.viz_stimulus, "viz-stimulus" } };

}
