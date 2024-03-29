﻿using DataPacketLibrary.Models;
using NetFwTypeLib; // For managing Windows Firewall rules
using System.Net;
using System.Net.Sockets;

namespace RestrictRService
{
    // ApplicationBlocker class deals with blocking websites by managing the Windows Firewall,
    // hosts file and providing methods to set which apps need to be blocked
    // Corresponds to the Tīmekļa bloķētājs (Website blocker) module in the documentation
    // Documentation function IDs - S_BLCK_SET, S_BLCK_FW_ADD, S_BLCK_FW_DEL, S_BLCK_FW_ALL, S_BLCK_HF_SYNC, S_BLCK_HF_ADD
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

        // Documentation function ID - S_BLCK_SET
        public void SetBlockedWebsites(BlockedWebsites blockedWebsites)
        {
            BlockedWebsites = blockedWebsites ?? throw new ArgumentNullException(nameof(blockedWebsites));

            SynchronizeRulesFromBlockedWebsites();
        }

        // Documentation function ID - S_BLCK_SET
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
                            // Documentation function ID - S_BLCK_SET
                            _firewallPolicy.Rules.Remove(rule.Name);
                        }
                    }

                    // similarily as with the firewall rules, find all the entries in the hosts file
                    // and remove those not mentioned in the list
                    SynchronizeHostsFile();

                }
            }
            else
            {
                AddBlockAllInternetRule();
            }
        }

        // adds a firewall rule for a specific website url
        // Documentation function ID - S_BLCK_FW_ADD
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

            // fallback mechanism if the firewall is not enough
            // add the url to hosts file
            try
            {
                ipaddresses = GetIpAddresses(websiteUrl);
                if (ipaddresses.Length > 0)
                {
                    AddEntryToHostsFile(websiteUrl);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        // adds a firewall rule to block all internet access
        // Documentation function ID - S_BLCK_FW_ALL
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
        // Documentation function ID - S_BLCK_FW_DEL
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
                catch (SocketException ex)
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

        // Documentation function ID - S_BLCK_HF_ADD
        private void AddEntryToHostsFile(string url)
        {
            var hostsFilePath = Path.Combine(Environment.SystemDirectory, "drivers/etc/hosts");
            var entry = $"127.0.0.1 {url} # Added by RestrictR";

            // add if it doesn't already exist
            if (!HostsFileContainsLine(hostsFilePath, entry))
            {
                using var sw = System.IO.File.AppendText(hostsFilePath);
                sw.WriteLine(entry);
            }
        }

        private bool HostsFileContainsLine(string filePath, string lineToCheck)
        {
            var lines = System.IO.File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.Trim().Equals(lineToCheck.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        // Documentation function ID - S_BLCK_HF_SYNC
        private void SynchronizeHostsFile()
        {
            var hostsFilePath = Path.Combine(Environment.SystemDirectory, "drivers/etc/hosts");
            var lines = System.IO.File.ReadAllLines(hostsFilePath).ToList();
            var updatedLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains("# Added by RestrictR"))
                {
                    var url = ExtractUrlFromHostsEntry(line);
                    if (BlockedWebsites.BlockedWebsiteUrls.Select(url => url.Url).Contains(url))
                    {
                        updatedLines.Add(line); // Keep the entry
                        continue;
                    }
                }
                else
                {
                    updatedLines.Add(line); // Keep non-service entries
                }
            }

            System.IO.File.WriteAllLines(hostsFilePath, updatedLines);
        }

        // Extracts the URL part of the entry based on your entry format
        private string ExtractUrlFromHostsEntry(string line)
        {
            return line.Split(' ')[1];
        }
    }
}
