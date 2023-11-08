using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetFwTypeLib; // For managing Windows Firewall rules

namespace RestrictRService
{
    public class WebsiteBlocker
    {
        public static void AddBlockAllInternetRule()
        {
            Type policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2")
                ?? throw new SystemException("Unable to retrieve firewall type HNetCfg.FwPolicy2");

            Type ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule")
                ?? throw new SystemException("Unable to retrieve firewall type HNetCfg.FwPolicy2");

            object fwPolicyObj = Activator.CreateInstance(policyType)
                ?? throw new SystemException("Unable to create instance of a firewall policy");
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)fwPolicyObj;

            object fwRuleObj = Activator.CreateInstance(ruleType)
                ?? throw new SystemException("Unable to create instance of a firewall rule");
            INetFwRule firewallRule = (INetFwRule)fwRuleObj;

            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallRule.Description = "Used to block all internet access.";
            firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Name = "Block Internet";

            firewallPolicy.Rules.Add(firewallRule);
        }

        public static void AddWebsiteBlockingRule(string websiteUrl)
        {
            throw new NotImplementedException();
        }

    }
}
