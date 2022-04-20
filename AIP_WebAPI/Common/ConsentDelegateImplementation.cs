using Microsoft.InformationProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIP_WebAPI.Common
{
    public class ConsentDelegateImplementation : IConsentDelegate
    {
        public Consent GetUserConsent(string url)
        {
            return Consent.Accept;
        }
    }
}