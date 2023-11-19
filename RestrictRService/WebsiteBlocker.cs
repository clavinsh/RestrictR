using Microsoft.VisualBasic;
using NetFwTypeLib; // For managing Windows Firewall rules
using System.Net;
using Windows.ApplicationModel.VoiceCommands;

namespace RestrictRService
{
    public class WebsiteBlocker
    {
        private const string BlockAllSitesRuleName = "Block Internet";
        private const string RuleGroupName = "RestrictR";

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

        public void SetBlockedWebsites(DataPacketLibrary.ConfigurationPacket.BlockedWebsites blockedWebsites)
        {
            BlockedWebsites = blockedWebsites;

            SynchronizeRulesFromBlockedWebsites();
        }

        //public void RemoveAllBlockingRules()
        //{
        //    // remove the block all sites rule if it is implemented
        //    if (BlockedWebsites.BlockAllSites)
        //    {
        //        RemoveBlockALlInternetRule();
        //        BlockedWebsites.BlockAllSites = false;
        //        return;
        //    }
        //    // remove rules from the rule list and clear it
        //    else if (BlockedWebsites.BlockedWebsiteUrls != null)
        //    {
        //        foreach (var rule in BlockedWebsites.BlockedWebsiteUrls)
        //        {
        //            if (RuleExists(rule))
        //            {
        //                _firewallPolicy.Rules.Remove(rule);
        //            }
        //            else
        //            {
        //                //log: warning
        //            }
        //        }
        //        BlockedWebsites.BlockedWebsiteUrls.Clear();
        //        return;
        //    }
        //}

        // sets the rule from the input parameter (usually retrieved from the config file)
        // and checks against the actual firewall rules in windows
        // adds the ones currently not present in the firewall, but logs a warning
        private void SynchronizeRulesFromConfig(List<string> namesFromConfig)
        {
            throw new NotImplementedException();

            //ManagedRuleNames = namesFromConfig;

            //foreach (string name in ManagedRuleNames)
            //{
            //    if(!RuleExists(name))
            //    {
                    
            //    }
            //}
        }

        // sets or removes rules based on the BlockedWebsites property
        // so that they match in the app and in the firewall
        private void SynchronizeRulesFromBlockedWebsites()
        {
            if (!BlockedWebsites.BlockAllSites)
            {
                RemoveBlockALlInternetRule();

                if (BlockedWebsites.BlockedWebsiteUrls != null)
                {
                    // add missing rules to the firewall that exist in the list
                    foreach (var rule in BlockedWebsites.BlockedWebsiteUrls)
                    {
                        if (!RuleExists(rule))
                        {
                            AddWebsiteBlockingRule(rule);
                        }
                    }

                    // go through all of the firewall rules that were created by this service,
                    // if some rule exists that is not at this point in the list, then remove it - 
                    // this maintains synchronized firewall rules with the property BlockedWebsites
                    var appCreatedRules = _firewallPolicy.Rules.Cast<INetFwRule>().Where(r => r.Grouping == RuleGroupName);
                    foreach (var rule in appCreatedRules)
                    {
                        if (!BlockedWebsites.BlockedWebsiteUrls.Contains(rule.Name))
                        {
                            _firewallPolicy.Rules.Remove(rule.Name);
                        }
                    }
                }
            }
            else
            {
                AddBlockAllInternetRule();
            }
        }

        // adds a firewall rule for a specific website url
        private void AddWebsiteBlockingRule(string websiteUrl)
        {
            IPAddress ipadress = GetIpAdressFromUrl(websiteUrl);

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
            firewallRule.Name = websiteUrl; 
            firewallRule.Grouping = RuleGroupName;
            firewallRule.RemoteAddresses = ipadress.ToString();

            _firewallPolicy.Rules.Add(firewallRule);
        }

        // adds a firewall rule to block all internet access
        private void AddBlockAllInternetRule()
        {
            if (RuleExists(BlockAllSitesRuleName))
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
        }

        // removes the firewall rule that block all internet access
        private void RemoveBlockALlInternetRule()
        {
            if (RuleExists(BlockAllSitesRuleName))
            {
                _firewallPolicy.Rules.Remove(BlockAllSitesRuleName);
            }
        }

        // checks if a rule exists in the Windows Firewall by looking at the name
        private bool RuleExists(string ruleName)
        {
            return _firewallPolicy.Rules.Cast<INetFwRule>().Any(r => r.Name == ruleName);
        }

        // retrieves the IP Adress for the specified URL
        private static IPAddress GetIpAdressFromUrl(string url)
        {
            Uri uri = new(url);

            IPAddress ip = Dns.GetHostAddresses(uri.Host)[0];

            return ip;
        }
    }
}
