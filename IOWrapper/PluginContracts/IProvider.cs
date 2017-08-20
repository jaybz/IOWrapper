﻿using System;
using System.Collections.Generic;

namespace Providers
{
    public interface IProvider : IDisposable
    {
        string ProviderName { get; }
        ProviderReport GetInputList();
        ProviderReport GetOutputList();

        bool SetProfileState(Guid profileGuid, bool state);
        bool SubscribeInput(InputSubscriptionRequest subReq);
        bool UnsubscribeInput(InputSubscriptionRequest subReq);
        bool SubscribeOutputDevice(OutputSubscriptionRequest subReq);
        bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq);
        //bool SetOutputButton(string dev, uint button, bool state);
        bool SetOutputState(OutputSubscriptionRequest subReq, InputType inputType, uint inputIndex, int state);
        //bool SubscribeAxis(string deviceHandle, uint axisId, dynamic callback);
    }

    public enum InputType { AXIS, BUTTON, POV };

    #region Subscriptions
    public class InputSubscriptionRequest : SubscriptionRequest
    {
        public InputType InputType { get; set; }
        //public string SubscriberId { get; set; }
        public uint InputIndex { get; set; }
        public dynamic Callback { get; set; }
        // used, eg, for DirectInput POV number
        public int InputSubId { get; set; } = 0;
        public Guid ProfileGuid { get; set; }
        public InputSubscriptionRequest Clone()
        {
            return (InputSubscriptionRequest)this.MemberwiseClone();
        }
    }

    public class OutputSubscriptionRequest : SubscriptionRequest { }

    public class SubscriptionRequest
    {
        public Guid SubscriberGuid { get; set; }
        public string ProviderName { get; set; }
        public string DeviceHandle { get; set; }
    }
    #endregion

    public class IOWrapperDevice
    {
        /// <summary>
        /// The human-friendly name of the device
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// A way to uniquely identify a device instance via it's API
        /// Note that ideally all providers implementing the same API should ideally generate the same device handles
        /// For something like RawInput or DirectInput, this would likely be based on VID/PID
        /// For an ordered API like XInput, this would just be controller number
        /// </summary>
        public string DeviceHandle { get; set; }

        /// <summary>
        /// The API implementation that handles this input
        /// This should be unique
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// The underlying API that handles this input
        /// It is intended that many providers could support a given API
        /// </summary>
        public string API { get; set; }

        /// <summary>
        /// Which buttons a device has
        /// </summary>
        public List<int> ButtonList { get; set; }

        /// <summary>
        /// The names of the buttons.
        /// If ommitted, buttons numbers will be communicated to the user
        /// </summary>
        public Dictionary<int, string> ButtonNames { get; set; }

        /// <summary>
        /// A List of Axis IDs that this axis supports, eg [1,3,5]
        /// This is basically a look-up table to AxisNames
        /// </summary>
        public List<int> AxisList { get; set; }

        /// <summary>
        /// A list of the names of each axis that this API reports
        /// </summary>
        public Dictionary<int, string> AxisNames { get; set; }
    }

    public class ProviderReport
    {
        public SortedDictionary<string, IOWrapperDevice> Devices { get; set; }
            = new SortedDictionary<string, IOWrapperDevice>();
    }
}
