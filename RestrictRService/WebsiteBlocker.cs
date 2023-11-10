using NetFwTypeLib; // For managing Windows Firewall rules
using Windows.ApplicationModel.VoiceCommands;

namespace RestrictRService
{
    public class WebsiteBlocker
    {
        private const string BlockAllSitesRuleName = "Block Internet";

        public void AddBlockAllInternetRule()
        {
            Type policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2")
                ?? throw new SystemException("Unable to retrieve firewall type HNetCfg.FwPolicy2");

            Type ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule")
                ?? throw new SystemException("Unable to retrieve firewall type HNetCfg.FWRule");

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
            firewallRule.Name = BlockAllSitesRuleName;

            firewallPolicy.Rules.Add(firewallRule);
        }

        public void RemoveBlockALlInternetRule()
        {
            Type policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2")
                ?? throw new SystemException("Unable to retrieve firewall type HNetCfg.FwPolicy2");

            object fwPolicyObj = Activator.CreateInstance(policyType)
                ?? throw new SystemException("Unable to create instance of a firewall policy");
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)fwPolicyObj;

            firewallPolicy.Rules.Remove(BlockAllSitesRuleName);
        }

        public void AddWebsiteBlockingRule(string websiteUrl)
        {
            throw new NotImplementedException();
        }

    }
}
