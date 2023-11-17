using Microsoft.VisualBasic;
using NetFwTypeLib; // For managing Windows Firewall rules
using Windows.ApplicationModel.VoiceCommands;

namespace RestrictRService
{
    public class WebsiteBlocker
    {
        private const string BlockAllSitesRuleName = "Block Internet";
        private const string RuleGroupName = "RestrictR";

        private List<string> ManagedRuleNames = new();

        private DataPacketLibrary.ConfigurationPacket.BlockedWebsites BlockedWebsites = new();

        private Type _firewallPolicyType;
        private INetFwPolicy2 _firewallPolicy;

        public WebsiteBlocker()
        {
            _firewallPolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2")
                ?? throw new SystemException("Unable to retrieve firewall type HNetCfg.FwPolicy2");

            object fwPolicyObj = Activator.CreateInstance(_firewallPolicyType)
                ?? throw new SystemException("Unable to create instance of a firewall policy");

            _firewallPolicy = (INetFwPolicy2)fwPolicyObj;
        }


        public void AddBlockAllInternetRule()
        {
            //rule has already been added, do nothing
            if (BlockedWebsites.BlockAllSites)
            {
                return;
            }

            Type ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule")
                ?? throw new SystemException("Unable to retrieve firewall type HNetCfg.FWRule");

            object fwRuleObj = Activator.CreateInstance(ruleType)
                ?? throw new SystemException("Unable to create instance of a firewall rule");
            INetFwRule firewallRule = (INetFwRule)fwRuleObj;

            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallRule.Description = "Used to block all internet access.";
            firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Name = BlockAllSitesRuleName;
            firewallRule.Grouping = RuleGroupName;

            _firewallPolicy.Rules.Add(firewallRule);
            BlockedWebsites.BlockAllSites = true;
        }

        public void RemoveBlockALlInternetRule()
        {
            if(RuleExists(BlockAllSitesRuleName))
            {
                _firewallPolicy.Rules.Remove(BlockAllSitesRuleName);
            }
        }

        public void RemoveAllBlockingRules()
        {
            // remove the block all sites rule if it is implemented
            if (BlockedWebsites.BlockAllSites)
            {
                RemoveBlockALlInternetRule();
                BlockedWebsites.BlockAllSites = false;
                return;
            }
            // remove rules from the rule list and clear it
            else if (BlockedWebsites.BlockedWebsiteUrls != null)
            {
                foreach (var rule in BlockedWebsites.BlockedWebsiteUrls)
                {
                    if (RuleExists(rule))
                    {
                        _firewallPolicy.Rules.Remove(rule);
                    }
                    else
                    {
                        //log: warning
                    }
                }
                BlockedWebsites.BlockedWebsiteUrls.Clear();
                return;
            }
        }

        public void AddWebsiteBlockingRule(string websiteUrl)
        {
            throw new NotImplementedException();
        }

        // sets the rule from the input parameter (usually retrieved from the config file)
        // and checks against the actual firewall rules in windows
        // adds the ones currently not present in the firewall, but logs a warning
        private void SynchronizeRulesFromConfig(List<string> namesFromConfig)
        {
            ManagedRuleNames = namesFromConfig;

            foreach (string name in ManagedRuleNames)
            {
                if(!RuleExists(name))
                {
                    
                }
            }
        }


        // sets or removes rules based on the BlockedWebsites parameter
        // so that they match in the app and in the firewall
        private void SynchronizeRulesFromBlockedWebsites()
        {
            if (BlockedWebsites.BlockAllSites)
            {

            }
            else if (BlockedWebsites.BlockedWebsiteUrls != null)
            {
                foreach(var rule in BlockedWebsites.BlockedWebsiteUrls)
                {

                }
            }


        }

        private bool RuleExists(string ruleName)
        {
            return _firewallPolicy.Rules.Cast<INetFwRule>().Any(r => r.Name == ruleName);
        }
    }
}
