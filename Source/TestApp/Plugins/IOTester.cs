﻿using HidWizards.IOWrapper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Wrappers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace TestApp.Plugins
{
    public class IOTester
    {
        private readonly InputSubscription _input;
        private OutputSubscription _output;
        private BindingDescriptor _bindingDescriptor;

        public IOTester(string name, ProviderDescriptor providerDescriptor, DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor)
        {
            // Input
            _input = new InputSubscription
            {
                ProviderDescriptor = providerDescriptor,
                DeviceDescriptor = deviceDescriptor,
                BindingDescriptor = bindingDescriptor,
                Callback = new Action<int>(value =>
                {
                    Console.WriteLine("{0} State: {1}", name, value);
                    if (_bindingDescriptor != null)
                    {
                        IOW.Instance.SetOutputstate(_output, _bindingDescriptor, value);
                    }
                })

            };
        }

        public IOTester SubscribeOutput(ProviderDescriptor providerDescriptor, DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor)
        {
            _output = new OutputSubscription()
            {
                ProviderDescriptor = providerDescriptor,
                DeviceDescriptor = deviceDescriptor
            };
            _bindingDescriptor = bindingDescriptor;
            if (!IOW.Instance.SubscribeOutput(_output))
            {
                throw new Exception("Could not subscribe to output");
            }
            return this;
        }

        public IOTester Subscribe()
        {
            if (!IOW.Instance.SubscribeInput(_input))
            {
                throw new Exception("Could not subscribe to input");
            }
            return this;    // allow chaining
        }

        public bool Unsubscribe()
        {
            if (!IOW.Instance.UnsubscribeInput(_input))
            {
                throw new Exception("Could not subscribe to SubReq");
            }
            return true;
        }
    }
}
