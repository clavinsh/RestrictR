using Microsoft.VisualBasic;
using NetFwTypeLib; // For managing Windows Firewall rules
using System.Net;
using System.Net.Sockets;
using DataPacketLibrary.Models;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Networking.NetworkOperators;
using static System.Net.WebRequestMethods;

namespace RestrictRService
{
    public class WebsiteBlocker : IWebsiteBlocker
    {
        private const string BlockAllSitesRuleName = "Block Internet";
        private const string RuleGroupName = "RestrictR";

        private BlockedWebsites BlockedWebsites = new();

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

        public void SetBlockedWebsites(BlockedWebsites blockedWebsites)
        {
            BlockedWebsites = blockedWebsites ?? throw new ArgumentNullException(nameof(blockedWebsites));

            SynchronizeRulesFromBlockedWebsites();
        }

        public void RemoveBlockedWebsites()
        {
            BlockedWebsites = new()
            {
                BlockedWebsiteUrls = new(),
                BlockAllSites = false
            };
            SynchronizeRulesFromBlockedWebsites();
        }

        public IEnumerable<INetFwRule> GetCreatedFwRules()
        {
            return _firewallPolicy.Rules.Cast<INetFwRule>().Where(r => r.Grouping == RuleGroupName);
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
                    foreach (var rule in BlockedWebsites.BlockedWebsiteUrls.Select(website => website.Url))
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
                        if (!BlockedWebsites.BlockedWebsiteUrls.Select(website => website.Url).Contains(rule.Name))
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
            IPAddress[] ipaddresses;

            try
            {
                ipaddresses = GetIpAddresses(websiteUrl);
            }
            catch (Exception ex)
            {
                // LOG: warning
                return;
            }

            Type ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule")
                ?? throw new SystemException("Unable to retrieve firewall type HNetCfg.FWRule");

            object fwRuleObj = Activator.CreateInstance(ruleType)
                ?? throw new SystemException("Unable to create instance of a firewall rule");
            INetFwRule firewallRule = (INetFwRule)fwRuleObj;

            string addresses = CreateCsvFromIPs(ipaddresses);

            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallRule.Description = "Used to block internet access to a specific URL.";
            firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Name = $"{websiteUrl}";
            firewallRule.Grouping = RuleGroupName;
            firewallRule.RemoteAddresses = addresses;

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

        // removes the firewall rule that blocks all internet access
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
            return _firewallPolicy.Rules.Cast<INetFwRule>().Any(r => r.Name.StartsWith(ruleName));
        }

        private static IPAddress[] GetIpAddresses(string url)
        {
            string? hostName = ExtractHostName(url);

            if (hostName != null)
            {
                try
                {
                    var ips = Dns.GetHostAddresses(hostName);

                    return ips;
                }
                catch(SocketException ex)
                {
                    string msg = ex.Message; // LOG: error no such host is known 
                    throw;
                }
                
            }
            else            
            {
                throw new ArgumentException("Invalid URL specified");
            }
        }

        static string? ExtractHostName(string input)
        {
            var uriBuilder = new UriBuilder(input);

            return uriBuilder?.Uri.Host;
        }

        private static string CreateCsvFromIPs(IPAddress[] ips)
        {
            var csv_questionmark = ips.ToString();

            string csv = string.Join(",", ips.Select(x => x.ToString()).ToArray());

            return csv;
        }
    }
}
