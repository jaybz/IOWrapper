﻿using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using HidWizards.IOWrapper.ProviderInterface;
using System.Collections.Generic;
using SharpDX.XInput;
using System.Threading;
using System.Diagnostics;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using Hidwizards.IOWrapper.Libraries.ProviderLogger;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace SharpDX_XInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_XInput : IInputProvider, IBindModeProvider
    {
        private readonly ConcurrentDictionary<DeviceDescriptor, IDeviceHandler<State>> _activeDevices
            = new ConcurrentDictionary<DeviceDescriptor, IDeviceHandler<State>>();
        private Action<ProviderDescriptor, DeviceDescriptor, BindingReport, int> _bindModeCallback;
        private readonly IInputDeviceLibrary<UserIndex> _deviceLibrary;

        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        private Logger logger;

        bool disposed;

        public SharpDX_XInput()
        {
            logger = new Logger(ProviderName);
            _deviceLibrary = new XiDeviceLibrary(new ProviderDescriptor{ProviderName = ProviderName});
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                //pollHandler.Dispose();
            }
            disposed = true;
            logger.Log("Disposed");
        }

        #region IProvider Members
        public string ProviderName { get { return typeof(SharpDX_XInput).Namespace; } }

        public ProviderReport GetInputList()
        {
            return _deviceLibrary.GetInputList();
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return _deviceLibrary.GetInputDeviceReport(subReq.DeviceDescriptor);
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            if (!_activeDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler))
            {
                deviceHandler = new XiDeviceHandlerBase(subReq.DeviceDescriptor, DeviceEmptyHandler, BindModeHandler, _deviceLibrary);
                _activeDevices.TryAdd(subReq.DeviceDescriptor, deviceHandler);
            }
            deviceHandler.SubscribeInput(subReq);
            return true;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            if (_activeDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler))
            {
                deviceHandler.UnsubscribeInput(subReq);
            }
            return true;
        }

        public void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingReport, int> callback = null)
        {
            if (!_activeDevices.TryGetValue(deviceDescriptor, out var deviceHandler))
            {
                deviceHandler = new XiDeviceHandlerBase(deviceDescriptor, DeviceEmptyHandler, BindModeHandler, _deviceLibrary);
                _activeDevices.TryAdd(deviceDescriptor, deviceHandler);
            }

            if (detectionMode == DetectionMode.Bind)
            {
                _bindModeCallback = callback;
            }

            if (detectionMode == DetectionMode.Subscription && deviceHandler.IsEmpty())
            {
                deviceHandler.Dispose();
                _activeDevices.TryRemove(deviceDescriptor, out _);
            }
            else
            {
                deviceHandler.SetDetectionMode(detectionMode);
            }
        }

        private void BindModeHandler(object sender, BindModeUpdate e)
        {
            _bindModeCallback?.Invoke(new ProviderDescriptor { ProviderName = ProviderName }, e.Device, e.Binding, e.Value);
        }

        public void RefreshLiveState()
        {
            // Built-in API, take no action
        }

        public void RefreshDevices()
        {
            _deviceLibrary.RefreshConnectedDevices();
        }

        private void DeviceEmptyHandler(object sender, DeviceDescriptor e)
        {
            _activeDevices[e].Dispose();
            _activeDevices.TryRemove(e, out _);
        }
        #endregion
    }
}
